using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Gameplay.Configs;
using Game.Gameplay.HexGrid;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using PrimeTween;
using UnityEngine;

namespace Game.Gameplay.Services
{
    public struct MergeResult
    {
        public bool MergeOccurred;
        public int PiecesRemoved;
        public HexColorType ColorType;
    }
    
    public interface IMergeService
    {
        IReadonlyProperty<bool> IsMerging { get; }
        ISource<MergeResult> OnMergeComplete { get; }
        
        UniTask<MergeResult> TryMerge(HexStack sourceStack, GridCell targetCell, Lifetime lifetime);
    }
    
    public class MergeService : IMergeService
    {
        private readonly GameplayConfig _gameplayConfig;
        private readonly IGridService _gridService;
        private readonly ViewableProperty<bool> _isMerging = new(false);
        private readonly Signal<MergeResult> _onMergeComplete = new();

        public IReadonlyProperty<bool> IsMerging => _isMerging;
        public ISource<MergeResult> OnMergeComplete => _onMergeComplete;

        public MergeService(GameplayConfig gameplayConfig, IGridService gridService)
        {
            _gameplayConfig = gameplayConfig;
            _gridService = gridService;
        }
        
        public async UniTask<MergeResult> TryMerge(HexStack sourceStack, GridCell targetCell, Lifetime lifetime)
        {
            var result = new MergeResult { MergeOccurred = false, PiecesRemoved = 0 };

            if (!targetCell.CanPlaceStack)
                return result;

            CleanupCell(targetCell);

            targetCell.PlaceStack(sourceStack);

            var placedStack = targetCell.Stack;
            var hasDirectMerge = HasDirectMergeWithNeighbors(targetCell.Coord);
            var requiresRemoval = placedStack != null && placedStack.GetTopColorCount() >= _gameplayConfig.MergeThreshold;
            
            if (!hasDirectMerge && !requiresRemoval)
                return result;

            // Передаем координату размещенной стопки как приоритетную цель для слияния
            await ProcessChainReaction(lifetime, targetCell.Coord, targetCell.Coord);

            result.MergeOccurred = true;
            return result;
        }
        
        private async UniTask ProcessChainReaction(Lifetime lifetime, HexCoord startCoord, HexCoord playerPlacedCoord)
        {
            _isMerging.Value = true;
            
            var currentSpeed = 1f;
            var safetyCounter = 0;
            const int maxIterations = 50;

            var activeCoords = new HashSet<HexCoord> { startCoord };

            while (lifetime.IsAlive && safetyCounter < maxIterations && activeCoords.Count > 0)
            {
                safetyCounter++;

                // Собираем все слияния для активных ячеек
                var mergeOperations = CollectAllMerges(activeCoords, playerPlacedCoord);
                var nextActive = new HashSet<HexCoord>();
                
                if (mergeOperations.Count == 0)
                {
                    // Нет слияний — проверяем удаления, но откладываем удаление для стопки игрока, если вокруг ещё возможны слияния
                    var removeOperations = CollectAllRemovals(activeCoords);

                    // Проверяем, остались ли потенциальные мёржи вокруг playerPlacedCoord
                    bool anchorHasPotentialMerges = HasPotentialMerges(playerPlacedCoord);

                    if (anchorHasPotentialMerges)
                    {
                        // Фильтруем удаление верхнего слоя для стопки игрока
                        removeOperations.RemoveAll(op => op.Coord == playerPlacedCoord);
                    }
                    
                    if (removeOperations.Count == 0)
                        break;
                    
                    await ExecuteRemovals(removeOperations, currentSpeed, lifetime);
                    foreach (var op in removeOperations)
                    {
                        nextActive.Add(op.Coord);
                    }
                    activeCoords = nextActive;
                    
                    // Ускоряем анимацию
                    currentSpeed *= _gameplayConfig.SpeedMultiplier;
                    continue;
                }

                // Выполняем логическую часть слияний
                var animations = PrepareAndExecuteMerges(mergeOperations);

                // Анимируем с текущей скоростью
                float duration = _gameplayConfig.HexFlyDuration / currentSpeed;
                await AnimateMerges(animations, duration, lifetime);

                // Финализируем
                FinalizeMerges(animations);

                foreach (var anim in animations)
                {
                    nextActive.Add(anim.TargetCoord);
                    nextActive.Add(anim.SourceCoord);
                }

                // После слияний удаляем завершённые слои, но не трогаем стопку игрока, если она ещё может принимать слияния
                var removals = CollectAllRemovals(nextActive);
                bool anchorHasPotentialMergesAfter = HasPotentialMerges(playerPlacedCoord);
                if (anchorHasPotentialMergesAfter)
                {
                    removals.RemoveAll(op => op.Coord == playerPlacedCoord);
                }
                if (removals.Count > 0)
                {
                    await ExecuteRemovals(removals, currentSpeed, lifetime);
                    foreach (var op in removals)
                    {
                        nextActive.Add(op.Coord);
                    }
                }

                activeCoords = nextActive;
                
                // Увеличиваем скорость на 30% после итерации
                currentSpeed *= _gameplayConfig.SpeedMultiplier;
                
                await UniTask.Yield(cancellationToken: lifetime);
            }
            
            _isMerging.Value = false;
        }

        private bool HasPotentialMerges(HexCoord coord)
        {
            if (!_gridService.TryGetCell(coord, out var cell)) return false;
            if (!cell.IsOccupied || cell.Stack == null || cell.Stack.IsDestroyed || cell.Stack.IsEmpty) return false;
            var topColor = cell.Stack.TopColorType;
            if (!topColor.HasValue) return false;
            var neighbors = _gridService.GetNeighbors(coord);
            foreach (var neighbor in neighbors)
            {
                if (neighbor.IsOccupied && neighbor.Stack != null && !neighbor.Stack.IsDestroyed && !neighbor.Stack.IsEmpty)
                {
                    if (neighbor.Stack.TopColorType == topColor)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #region Сбор операций

        private List<MergeOperation> CollectAllMerges(HashSet<HexCoord> activeCoords, HexCoord playerPlacedCoord)
        {
            var operations = new List<MergeOperation>();
            var processedPairs = new HashSet<(HexCoord, HexCoord)>();

            foreach (var coord in activeCoords)
            {
                if (!_gridService.TryGetCell(coord, out var cell))
                    continue;
                
                if (!cell.IsOccupied || cell.Stack == null || cell.Stack.IsEmpty || cell.Stack.IsDestroyed)
                    continue;

                var stack = cell.Stack;
                var topColor = stack.TopColorType;
                
                if (!topColor.HasValue)
                    continue;

                // Проверяем всех соседей
                foreach (var neighborCoord in coord.GetAllNeighbors())
                {
                    // Пропускаем уже обработанные пары
                    var pairKey = GetPairKey(coord, neighborCoord);
                    if (processedPairs.Contains(pairKey))
                        continue;

                    if (!_gridService.TryGetCell(neighborCoord, out var neighborCell))
                        continue;

                    if (!neighborCell.IsOccupied || neighborCell.Stack == null || 
                        neighborCell.Stack.IsEmpty || neighborCell.Stack.IsDestroyed)
                        continue;

                    var neighborStack = neighborCell.Stack;
                    
                    // Проверяем совпадение цветов
                    if (neighborStack.TopColorType != topColor)
                        continue;

                    // Проверяем что это действительно соседи (расстояние = 1)
                    var distance = coord.DistanceTo(neighborCoord);
                    if (distance != 1)
                    {
                        Debug.LogError($"[MergeService] Invalid neighbor distance: {distance} between {coord} and {neighborCoord}");
                        continue;
                    }

                    // Определяем направление слияния
                    // Если одна из стопок - размещенная игроком, она становится целью
                    var op = ResolveOperationDirection(coord, stack, neighborCoord, neighborStack, topColor.Value, playerPlacedCoord);
                    if (op.HasValue)
                    {
                        operations.Add(op.Value);
                        processedPairs.Add(pairKey);
                    }
                }
            }

            return operations;
        }

        private List<RemovalOperation> CollectAllRemovals(HashSet<HexCoord> activeCoords)
        {
            var operations = new List<RemovalOperation>();

            foreach (var coord in activeCoords)
            {
                if (!_gridService.TryGetCell(coord, out var cell))
                    continue;

                if (!cell.IsOccupied || cell.Stack == null || cell.Stack.IsEmpty || cell.Stack.IsDestroyed)
                    continue;

                var stack = cell.Stack;
                
                if (stack.GetTopColorCount() >= _gameplayConfig.MergeThreshold)
                {
                    operations.Add(new RemovalOperation
                    {
                        Stack = stack,
                        Coord = cell.Coord
                    });
                }
            }

            return operations;
        }

        #endregion

        #region Выполнение слияний

        private List<MergeAnimation> PrepareAndExecuteMerges(List<MergeOperation> operations)
        {
            var animations = new List<MergeAnimation>();
            var usedStacks = new HashSet<HexStack>();

            foreach (var op in operations)
            {
                // Пропускаем если стопки уже использованы в этой итерации
                if (usedStacks.Contains(op.SourceStack) || usedStacks.Contains(op.TargetStack))
                    continue;

                // Повторная валидация
                if (!ValidateOperation(op))
                    continue;

                // Забираем гексы из source
                var pieces = op.SourceStack.RemoveTopPiecesOfColor(op.ColorType);
                if (pieces.Count == 0)
                    continue;

                // Запоминаем начальные позиции для анимации
                var startPositions = new List<Vector3>();
                foreach (var piece in pieces)
                {
                    startPositions.Add(piece.transform.position);
                    // Отсоединяем от родителя для анимации
                    piece.transform.SetParent(null);
                }

                // Обновляем source
                op.SourceStack.RefreshPiecePositions();

                // Помечаем стопки как использованные
                usedStacks.Add(op.SourceStack);
                usedStacks.Add(op.TargetStack);

                // Получаем контейнер гексов целевой стопки для вычисления позиций
                var targetPiecesContainer = op.TargetStack.transform;
                
                animations.Add(new MergeAnimation
                {
                    Pieces = pieces,
                    StartPositions = startPositions,
                    TargetStack = op.TargetStack,
                    SourceStack = op.SourceStack,
                    PiecesContainer = targetPiecesContainer,
                    BaseTargetIndex = op.TargetStack.Count,
                    SourceCoord = op.SourceCoord,
                    TargetCoord = op.TargetCoord
                });
            }

            return animations;
        }

        private async UniTask AnimateMerges(List<MergeAnimation> animations, float duration, Lifetime lifetime)
        {
            var allTasks = new List<UniTask>();

            foreach (var anim in animations)
            {
                for (int i = 0; i < anim.Pieces.Count; i++)
                {
                    var piece = anim.Pieces[i];
                    if (piece == null) continue;

                    // Вычисляем целевую позицию, используя контейнер целевой стопки
                    // Локальная позиция: (0, baseOffset + index * stackOffset, 0)
                    var localPos = new Vector3(0, anim.TargetStack.GetBaseOffset() + (anim.BaseTargetIndex + i) * anim.TargetStack.GetStackOffset(), 0);
                    var targetPos = anim.PiecesContainer.TransformPoint(localPos);

                    // Задержка между запуском анимации каждого гекса
                    var delay = i * 0.05f;

                    allTasks.Add(AnimatePieceFlyWithArc(piece, anim.StartPositions[i], targetPos, duration, delay, lifetime));
                }
            }

            if (allTasks.Count > 0)
            {
                await UniTask.WhenAll(allTasks);
            }
        }

        private void FinalizeMerges(List<MergeAnimation> animations)
        {
            foreach (var anim in animations)
            {
                // Проверяем что target всё ещё валиден
                if (anim.TargetStack == null || anim.TargetStack.IsDestroyed)
                {
                    // Target уничтожен - уничтожаем гексы
                    foreach (var piece in anim.Pieces)
                    {
                        piece?.Destroy();
                    }
                    continue;
                }

                // Добавляем гексы в target
                foreach (var piece in anim.Pieces)
                {
                    if (piece != null)
                    {
                        anim.TargetStack.AddPiece(piece);
                    }
                }

                // Обновляем позиции
                anim.TargetStack.RefreshPiecePositions();

                // Удаляем пустую source стопку
                CleanupEmptyStack(anim.SourceStack);

                // Отправляем событие
                _onMergeComplete.Fire(new MergeResult
                {
                    MergeOccurred = true,
                    PiecesRemoved = anim.Pieces.Count,
                    ColorType = anim.Pieces[0]?.ColorType ?? HexColorType.Green
                });
            }
        }

        #endregion

        #region Удаление завершённых слоёв

        private async UniTask ExecuteRemovals(List<RemovalOperation> operations, float speedMultiplier, Lifetime lifetime)
        {
            foreach (var op in operations)
            {
                if (op.Stack == null || op.Stack.IsEmpty || op.Stack.IsDestroyed)
                    continue;

                // Проверяем ещё раз порог
                while (op.Stack.GetTopColorCount() >= _gameplayConfig.MergeThreshold)
                {
                    var colorType = op.Stack.TopColorType;
                    if (!colorType.HasValue) break;

                    var pieces = op.Stack.RemoveTopPiecesOfColor(colorType.Value);
                    if (pieces.Count == 0) break;

                    op.Stack.RefreshPiecePositions();

                    // Анимация исчезновения
                    var duration = _gameplayConfig.HexFlyDuration / speedMultiplier;
                    await AnimatePiecesDisappear(pieces, duration, lifetime);

                    _onMergeComplete.Fire(new MergeResult
                    {
                        MergeOccurred = true,
                        PiecesRemoved = pieces.Count,
                        ColorType = colorType.Value
                    });
                }

                // Удаляем пустую стопку
                CleanupEmptyStack(op.Stack);
            }
        }

        #endregion

        #region Вспомогательные методы

        private bool ValidateOperation(MergeOperation op)
        {
            if (op.SourceStack == null || op.TargetStack == null)
                return false;

            if (op.SourceStack.IsEmpty || op.SourceStack.IsDestroyed)
                return false;

            if (op.TargetStack.IsEmpty || op.TargetStack.IsDestroyed)
                return false;

            if (!op.SourceStack.IsOnGrid || !op.TargetStack.IsOnGrid)
                return false;

            if (op.SourceStack.GridPosition != op.SourceCoord || op.TargetStack.GridPosition != op.TargetCoord)
                return false;

            if (op.SourceStack.TopColorType != op.TargetStack.TopColorType)
                return false;

            var distance = op.SourceCoord.DistanceTo(op.TargetCoord);
            if (distance != 1)
                return false;

            return true;
        }

        private void CleanupCell(GridCell cell)
        {
            if (cell.Stack != null && (cell.Stack.IsEmpty || cell.Stack.IsDestroyed))
            {
                var stack = cell.RemoveStack();
                if (!stack.IsDestroyed)
                {
                    stack.Destroy();
                }
            }
        }

        private void CleanupEmptyStack(HexStack stack)
        {
            if (stack == null || !stack.IsEmpty || stack.IsDestroyed || !stack.IsOnGrid)
                return;

            if (_gridService.TryGetCell(stack.GridPosition, out var cell))
            {
                if (cell.Stack == stack)
                {
                    cell.RemoveStack();
                }
            }

            stack.Destroy();
        }

        private static (HexCoord, HexCoord) GetPairKey(HexCoord a, HexCoord b)
        {
            // Нормализуем порядок для уникальности
            if (a.Q < b.Q || (a.Q == b.Q && a.R < b.R))
                return (a, b);
            return (b, a);
        }

        private async UniTask AnimatePieceFlyWithArc(HexPiece piece, Vector3 startPos, Vector3 endPos, float duration, float delay, Lifetime lifetime)
        {
            if (!lifetime.IsAlive || piece == null) return;

            piece.transform.position = startPos;
            piece.transform.rotation = Quaternion.identity;

            // Ждём задержку перед стартом анимации
            if (delay > 0)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(delay), cancellationToken: lifetime);
                if (!lifetime.IsAlive || piece == null) return;
            }

            // Вычисляем направление и высоту дуги
            var midPoint = (startPos + endPos) / 2f;
            var arcHeight = _gameplayConfig.HexArcHeight; // Используем высоту из конфига
            midPoint += Vector3.up * arcHeight;
            
            // Вычисляем ось вращения: перпендикулярна направлению движения и направлена вверх
            // Это создаст эффект "переворачивания котлеты"
            var moveDirection = (endPos - startPos).normalized;
            var rotationAxis = Vector3.Cross(moveDirection, Vector3.up).normalized;
            
            // Если направление почти вертикальное, используем другую ось
            if (rotationAxis.magnitude < 0.01f)
            {
                rotationAxis = Vector3.right;
            }

            // Запускаем анимации движения и поворота параллельно
            var movementTask = Tween.Custom(0f, 1f, duration, onValueChange: t =>
            {
                if (piece == null) return;
                
                // Квадратичная интерполяция через три точки (дуга)
                var p0 = startPos;
                var p1 = midPoint;
                var p2 = endPos;
                
                var a = Vector3.Lerp(p0, p1, t);
                var b = Vector3.Lerp(p1, p2, t);
                piece.transform.position = Vector3.Lerp(a, b, t);
            }, ease: Ease.OutQuad).ToUniTask(cancellationToken: lifetime);

            // Поворот на 180 градусов вокруг оси, перпендикулярной движению
            var rotationTask = Tween.Custom(0f, 1f, duration, onValueChange: t =>
            {
                if (piece == null) return;
                
                // Интерполируем угол от 0 до -180 градусов (инвертированный поворот)
                var angle = Mathf.Lerp(0f, -180f, t);
                piece.transform.rotation = Quaternion.AngleAxis(angle, rotationAxis);
            }, ease: Ease.InOutSine).ToUniTask(cancellationToken: lifetime);

            await UniTask.WhenAll(movementTask, rotationTask);
            
            // В конце сбрасываем ротацию в identity для корректного положения в стопке
            if (piece != null && !piece.IsDestroyed)
            {
                piece.transform.rotation = Quaternion.identity;
            }
        }

        private static async UniTask AnimatePiecesDisappear(List<HexPiece> pieces, float duration, Lifetime lifetime)
        {
            if (!lifetime.IsAlive || pieces.Count == 0) return;

            var tasks = new List<UniTask>();

            foreach (var piece in pieces)
            {
                if (piece == null) continue;
                piece.transform.SetParent(null);

                tasks.Add(Tween.Scale(
                    piece.transform,
                    endValue: Vector3.zero,
                    duration: duration,
                    ease: Ease.InBack
                ).ToUniTask(cancellationToken: lifetime));
            }

            await UniTask.WhenAll(tasks);

            foreach (var piece in pieces)
            {
                piece?.Destroy();
            }
        }

        private bool HasDirectMergeWithNeighbors(HexCoord coord)
        {
            if (!_gridService.TryGetCell(coord, out var cell))
                return false;

            if (!cell.IsOccupied || cell.Stack == null || cell.Stack.IsEmpty || cell.Stack.IsDestroyed)
                return false;

            var stack = cell.Stack;
            var topColor = stack.TopColorType;

            if (!topColor.HasValue)
                return false;

            // Проверяем всех соседей
            foreach (var neighborCoord in coord.GetAllNeighbors())
            {
                if (!_gridService.TryGetCell(neighborCoord, out var neighborCell))
                    continue;

                if (!neighborCell.IsOccupied || neighborCell.Stack == null || 
                    neighborCell.Stack.IsEmpty || neighborCell.Stack.IsDestroyed)
                    continue;

                var neighborStack = neighborCell.Stack;
                
                // Проверяем совпадение цветов
                if (neighborStack.TopColorType == topColor)
                    return true;
            }

            return false;
        }

        private static MergeOperation? ResolveOperationDirection(HexCoord coord, HexStack stack, HexCoord neighborCoord, HexStack neighborStack, HexColorType colorType, HexCoord playerPlacedCoord)
        {
            // ПРИОРИТЕТ: Если одна из стопок - размещенная игроком, она становится целью слияния
            if (neighborCoord == playerPlacedCoord)
            {
                // Соседняя стопка размещена игроком -> вливаем в неё
                return new MergeOperation
                {
                    SourceStack = stack,
                    TargetStack = neighborStack,
                    SourceCoord = coord,
                    TargetCoord = neighborCoord,
                    ColorType = colorType
                };
            }

            if (coord == playerPlacedCoord)
            {
                // Текущая стопка размещена игроком -> вливаем в неё
                return new MergeOperation
                {
                    SourceStack = neighborStack,
                    TargetStack = stack,
                    SourceCoord = neighborCoord,
                    TargetCoord = coord,
                    ColorType = colorType
                };
            }

            // Если ни одна из стопок не размещена игроком, используем стандартную логику
            // Вливаем из меньшей стопки в большую по количеству верхних гексов целевого цвета
            var stackCount = stack.GetTopColorCount();
            var neighborCount = neighborStack.GetTopColorCount();

            if (stackCount == neighborCount)
            {
                // При равенстве сохраняем направление coord -> neighbor
                return new MergeOperation
                {
                    SourceStack = stack,
                    TargetStack = neighborStack,
                    SourceCoord = coord,
                    TargetCoord = neighborCoord,
                    ColorType = colorType
                };
            }

            var stackIsSmaller = stackCount < neighborCount;
            return new MergeOperation
            {
                SourceStack = stackIsSmaller ? stack : neighborStack,
                TargetStack = stackIsSmaller ? neighborStack : stack,
                SourceCoord = stackIsSmaller ? coord : neighborCoord,
                TargetCoord = stackIsSmaller ? neighborCoord : coord,
                ColorType = colorType
            };
        }
        
        #endregion

        #region Вспомогательные структуры

        private struct MergeOperation
        {
            public HexStack SourceStack;
            public HexStack TargetStack;
            public HexCoord SourceCoord;
            public HexCoord TargetCoord;
            public HexColorType ColorType;
        }

        private struct RemovalOperation
        {
            public HexStack Stack;
            public HexCoord Coord;
        }

        private class MergeAnimation
        {
            public List<HexPiece> Pieces;
            public List<Vector3> StartPositions;
            public HexStack TargetStack;
            public HexStack SourceStack;
            public Transform PiecesContainer;
            public int BaseTargetIndex;
            public HexCoord SourceCoord;
            public HexCoord TargetCoord;
        }

        #endregion
    }
}