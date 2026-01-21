using Cysharp.Threading.Tasks;
using Game.Gameplay.Configs;
using Game.Gameplay.HexGrid;
using Game.Utilities.Lifetimes;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using PrimeTween;
using R3;
using UnityEngine;

namespace Game.Gameplay.Services
{
    public enum DragState
    {
        Idle,
        Dragging,
        Hovering
    }
    
    public interface IDragDropService
    {
        IReadonlyProperty<DragState> CurrentState { get; }
        IReadonlyProperty<HexStack> DraggedStack { get; }
        IReadonlyProperty<GridCell> HoveredCell { get; }
        ISource<HexStack> OnStackDropped { get; }
    }
    
    public class DragDropService : IDragDropService, ILifetimeInitializable
    {
        private readonly GameplayConfig _gameplayConfig;
        private readonly IGridService _gridService;
        private readonly IMergeService _mergeService;
        private readonly IPlayerStacksService _playerStacksService;
        private readonly Camera _camera;

        private readonly ViewableProperty<DragState> _currentState = new(DragState.Idle);
        private readonly ViewableProperty<HexStack> _draggedStack = new(null);
        private readonly ViewableProperty<GridCell> _hoveredCell = new(null);
        private readonly Signal<HexStack> _onStackDropped = new();

        private Vector3 _dragStartPosition;
        private Vector3 _dragOffset;
        private Lifetime _dragLifetime;
        private LifetimeDefinition _dragLifetimeDefinition;
        private Lifetime _serviceLifetime;

        public IReadonlyProperty<DragState> CurrentState => _currentState;
        public IReadonlyProperty<HexStack> DraggedStack => _draggedStack;
        public IReadonlyProperty<GridCell> HoveredCell => _hoveredCell;
        public ISource<HexStack> OnStackDropped => _onStackDropped;

        public DragDropService(
            GameplayConfig gameplayConfig,
            IGridService gridService,
            IMergeService mergeService,
            IPlayerStacksService playerStacksService,
            Camera camera)
        {
            _gameplayConfig = gameplayConfig;
            _gridService = gridService;
            _mergeService = mergeService;
            _playerStacksService = playerStacksService;
            _camera = camera;
        }

        public void Initialize(Lifetime lifetime)
        {
            _serviceLifetime = lifetime;
            
            Observable.EveryUpdate()
                .Subscribe(_ => ProcessInput())
                .AddTo(lifetime);
        }

        private void ProcessInput()
        {
            if (_mergeService.IsMerging.Value) return;

            if (Input.GetMouseButtonDown(0))
            {
                TryStartDrag();
            }
            else if (Input.GetMouseButton(0) && _currentState.Value is DragState.Dragging or DragState.Hovering)
            {
                UpdateDrag();
            }
            else if (Input.GetMouseButtonUp(0) && _currentState.Value is DragState.Dragging or DragState.Hovering)
            {
                EndDrag();
            }
        }

        private void TryStartDrag()
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out var hit, 100f))
            {
                var stack = hit.collider.GetComponentInParent<HexStack>();
                
                if (stack != null && stack.IsPlayerStack)
                {
                    StartDrag(stack);
                }
            }
        }

        private void StartDrag(HexStack stack)
        {
            _dragLifetimeDefinition?.Terminate();
            _dragLifetimeDefinition = new LifetimeDefinition();
            _dragLifetime = _dragLifetimeDefinition.Lifetime;

            _draggedStack.Value = stack;
            _dragStartPosition = stack.transform.position;
            _currentState.Value = DragState.Dragging;

            var plane = new Plane(Vector3.up, Vector3.zero);
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            
            if (plane.Raycast(ray, out float distance))
            {
                var hitPoint = ray.GetPoint(distance);
                _dragOffset = stack.transform.position - hitPoint;
            }

            Tween.PositionY(
                stack.transform,
                _dragStartPosition.y + _gameplayConfig.DragHeight,
                0.2f,
                Ease.OutQuad
            );

            stack.SetInteractable(false);
        }

        private void UpdateDrag()
        {
            if (_draggedStack.Value == null) return;

            var plane = new Plane(Vector3.up, new Vector3(0, _gameplayConfig.DragHeight, 0));
            var ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out float distance))
            {
                var hitPoint = ray.GetPoint(distance);
                var targetPos = hitPoint + _dragOffset;
                targetPos.y = _dragStartPosition.y + _gameplayConfig.DragHeight;
                
                _draggedStack.Value.transform.position = targetPos;

                UpdateCellHighlight(new Vector3(targetPos.x, 0, targetPos.z));
            }
        }

        private void UpdateCellHighlight(Vector3 worldPosition)
        {
            _gridService.ClearAllHighlights();
            
            var coord = _gridService.GetNearestCoord(worldPosition);
            
            if (_gridService.TryGetCell(coord, out var cell))
            {
                var cellWorldPos = _gridService.GetWorldPosition(coord);
                var distanceToCell = Vector3.Distance(
                    new Vector3(worldPosition.x, 0, worldPosition.z),
                    new Vector3(cellWorldPos.x, 0, cellWorldPos.z)
                );

                if (distanceToCell <= _gameplayConfig.SnapDistance)
                {
                    bool isValid = cell.CanPlaceStack;
                    cell.SetHighlight(true, isValid);
                    _hoveredCell.Value = cell;
                    _currentState.Value = DragState.Hovering;
                    return;
                }
            }

            _hoveredCell.Value = null;
            _currentState.Value = DragState.Dragging;
        }

        private void EndDrag()
        {
            if (_draggedStack.Value == null) return;

            var stack = _draggedStack.Value;
            var hoveredCell = _hoveredCell.Value;

            _gridService.ClearAllHighlights();

            var canPlace = hoveredCell != null && hoveredCell.CanPlaceStack;
            if (canPlace)
            {
                PlaceStack(stack, hoveredCell).Forget();
            }
            else
            {
                ReturnStackToStart(stack);
            }

            _draggedStack.Value = null;
            _hoveredCell.Value = null;
            _currentState.Value = DragState.Idle;
            _dragLifetimeDefinition?.Terminate();
        }

        private async UniTask PlaceStack(HexStack stack, GridCell cell)
        {
            if (cell.Stack != null && (cell.Stack.IsEmpty || cell.Stack.IsDestroyed))
            {
                var emptyStack = cell.RemoveStack();
                if (!emptyStack.IsDestroyed)
                {
                    emptyStack.Destroy();
                }
            }
            
            var targetPos = _gridService.GetWorldPosition(cell.Coord);
            
            await Tween.Position(
                stack.transform,
                targetPos,
                0.2f,
                Ease.OutQuad
            ).ToUniTask(cancellationToken: _serviceLifetime);
            
            await _mergeService.TryMerge(stack, cell, _serviceLifetime);

            _playerStacksService.OnStackPlaced(stack);
            _onStackDropped.Fire(stack);
        }

        private void ReturnStackToStart(HexStack stack)
        {
            Tween.Position(
                stack.transform,
                _dragStartPosition,
                0.3f,
                Ease.OutBack
            );

            stack.SetInteractable(true);
        }
    }
}
