using Cysharp.Threading.Tasks;
using Game.Utilities.Lifetimes;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;
using Unit = JetBrains.Core.Unit;

namespace Game.Gameplay.Services
{
    public enum TutorialState
    {
        Inactive,
        ShowingHand,
        WaitingForDrag,
        Completed
    }

    public interface ITutorialService
    {
        IReadonlyProperty<bool> IsActive { get; }
        IReadonlyProperty<TutorialState> CurrentState { get; }
        IReadonlyProperty<Vector3> HandTargetPosition { get; }
        IReadonlyProperty<Vector3> HandDestinationPosition { get; }
        ISource<Unit> OnTutorialComplete { get; }
        
        void StartTutorial();
    }

    public class TutorialService : ITutorialService, ILifetimeInitializable
    {
        private readonly IPlayerStacksService _playerStacksService;
        private readonly IGridService _gridService;
        private readonly IDragDropService _dragDropService;

        private readonly ViewableProperty<bool> _isActive = new(false);
        private readonly ViewableProperty<TutorialState> _currentState = new(TutorialState.Inactive);
        private readonly ViewableProperty<Vector3> _handTargetPosition = new(Vector3.zero);
        private readonly ViewableProperty<Vector3> _handDestinationPosition = new(Vector3.zero);
        private readonly Signal<Unit> _onTutorialComplete = new();

        private const float InactivityTimeout = 2f;
        private float _inactivityTimer;
        private bool _hasPlayerInteracted;
        private Lifetime _tutorialLifetime;
        private LifetimeDefinition _tutorialLifetimeDefinition;

        public IReadonlyProperty<bool> IsActive => _isActive;
        public IReadonlyProperty<TutorialState> CurrentState => _currentState;
        public IReadonlyProperty<Vector3> HandTargetPosition => _handTargetPosition;
        public IReadonlyProperty<Vector3> HandDestinationPosition => _handDestinationPosition;
        public ISource<Unit> OnTutorialComplete => _onTutorialComplete;

        public TutorialService(
            IPlayerStacksService playerStacksService,
            IGridService gridService,
            IDragDropService dragDropService)
        {
            _playerStacksService = playerStacksService;
            _gridService = gridService;
            _dragDropService = dragDropService;
        }

        public void Initialize(Lifetime lifetime)
        {
            _dragDropService.CurrentState.Advise(lifetime, state =>
            {
                if (state == DragState.Dragging && _isActive.Value)
                {
                    OnPlayerStartedDrag();
                }
            });

            _dragDropService.OnStackDropped.Advise(lifetime, _ =>
            {
                if (_isActive.Value)
                {
                    OnPlayerPlacedStack();
                }
            });
        }

        public void StartTutorial()
        {
            if (_currentState.Value == TutorialState.Completed)
                return;

            _tutorialLifetimeDefinition?.Terminate();
            _tutorialLifetimeDefinition = new LifetimeDefinition();
            _tutorialLifetime = _tutorialLifetimeDefinition.Lifetime;

            _isActive.Value = true;
            _hasPlayerInteracted = false;
            _inactivityTimer = 0f;

            UpdateHandPositions();
            _currentState.Value = TutorialState.ShowingHand;

            StartInactivityTimer(_tutorialLifetime).Forget();
        }

        private void UpdateHandPositions()
        {
            var stacks = _playerStacksService.CurrentStacks.Value;
            if (stacks != null && stacks.Count > 0)
            {
                _handTargetPosition.Value = stacks[0].transform.position + Vector3.up * 0.5f;
            }

            var emptyCells = _gridService.GetEmptyCells();
            if (emptyCells.Count > 0)
            {
                var bestCell = emptyCells[0];
                var bestDistance = float.MaxValue;

                foreach (var cell in emptyCells)
                {
                    var distance = cell.Coord.R;
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestCell = cell;
                    }
                }

                _handDestinationPosition.Value = _gridService.GetWorldPosition(bestCell.Coord) + Vector3.up * 0.5f;
            }
        }

        public void OnPlayerStartedDrag()
        {
            _hasPlayerInteracted = true;
            _inactivityTimer = 0f;
            _currentState.Value = TutorialState.WaitingForDrag;
        }

        public void OnPlayerPlacedStack()
        {
            CompleteTutorial();
        }

        private void CompleteTutorial()
        {
            _currentState.Value = TutorialState.Completed;
            _isActive.Value = false;
            _onTutorialComplete.Fire(Unit.Instance);
            _tutorialLifetimeDefinition?.Terminate();
        }

        private async UniTask StartInactivityTimer(Lifetime lifetime)
        {
            while (lifetime.IsAlive && _isActive.Value)
            {
                await UniTask.Delay(100, cancellationToken: lifetime);

                if (_hasPlayerInteracted)
                {
                    _inactivityTimer = 0f;
                    _hasPlayerInteracted = false;
                }
                else
                {
                    _inactivityTimer += 0.1f;
                }

                if (_inactivityTimer >= InactivityTimeout && _currentState.Value == TutorialState.WaitingForDrag)
                {
                    _currentState.Value = TutorialState.ShowingHand;
                    _inactivityTimer = 0f;
                }
            }
        }
    }
}