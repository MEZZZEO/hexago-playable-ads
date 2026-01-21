using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Gameplay.Configs;
using Game.Gameplay.HexGrid;
using Game.Utilities.Lifetimes;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Game.Gameplay.Services
{
    public interface IPlayerStacksService
    {
        IReadonlyProperty<IReadOnlyList<HexStack>> CurrentStacks { get; }
        IReadonlyProperty<int> RemainingStacks { get; }
        ISource<Unit> OnAllStacksPlaced { get; }
        
        UniTask SpawnNewStacks(Lifetime lifetime);
        void OnStackPlaced(HexStack stack);
    }
    
    public class PlayerStacksService : IPlayerStacksService, ILifetimeInitializable
    {
        private readonly GameplayConfig _gameplayConfig;
        private readonly IStackFactory _stackFactory;
        private readonly IGridService _gridService;
        private readonly HexColorConfig _hexColorConfig;
        private readonly Transform _playerStacksContainer;
        private readonly List<HexStack> _currentStacks = new();
        private readonly ViewableProperty<IReadOnlyList<HexStack>> _currentStacksProperty;
        private readonly ViewableProperty<int> _remainingStacks = new(0);
        private readonly Signal<Unit> _onAllStacksPlaced = new();

        private Lifetime _levelLifetime;
        private LifetimeDefinition _stacksLifetimeDefinition;

        public IReadonlyProperty<IReadOnlyList<HexStack>> CurrentStacks => _currentStacksProperty;
        public IReadonlyProperty<int> RemainingStacks => _remainingStacks;
        public ISource<Unit> OnAllStacksPlaced => _onAllStacksPlaced;

        public PlayerStacksService(
            GameplayConfig gameplayConfig,
            IStackFactory stackFactory,
            IGridService gridService,
            HexColorConfig hexColorConfig,
            Transform playerStacksContainer)
        {
            _gameplayConfig = gameplayConfig;
            _stackFactory = stackFactory;
            _gridService = gridService;
            _hexColorConfig = hexColorConfig;
            _playerStacksContainer = playerStacksContainer;
            _currentStacksProperty = new ViewableProperty<IReadOnlyList<HexStack>>(_currentStacks);
        }

        public void Initialize(Lifetime lifetime)
        {
            _levelLifetime = lifetime;
        }
        
        public async UniTask SpawnNewStacks(Lifetime parentLifetime)
        {
            _stacksLifetimeDefinition?.Terminate();
            _stacksLifetimeDefinition = new LifetimeDefinition();
            
            parentLifetime.OnTermination(() => _stacksLifetimeDefinition?.Terminate());

            _currentStacks.Clear();

            var spacing = 2f;
            var totalWidth = (_gameplayConfig.PlayerStacksCount - 1) * spacing;
            var startX = -totalWidth / 2f;

            for (int i = 0; i < _gameplayConfig.PlayerStacksCount; i++)
            {
                var position = _playerStacksContainer.position + new Vector3(startX + i * spacing, 0, 0);

                var friendlyCount = Mathf.Clamp(_gameplayConfig.TargetMovesToSolve, 1, _gameplayConfig.PlayerStacksCount);
                var makeFriendly = i < friendlyCount;

                List<HexColorType> colors;
                if (makeFriendly)
                {
                    colors = GenerateFriendlyPlayerStackColors();
                }
                else
                {
                    var minSize = _gameplayConfig.MinStackSize;
                    var maxSize = _gameplayConfig.MaxStackSize;
                    var maxColorsPerStack = Mathf.Clamp(_gameplayConfig.MaxColorsPerStack, 1, 3);
                    var balance = _gameplayConfig.BalanceColorSegments;
                    colors = GenerateNeutralPlayerStackColors(minSize, maxSize, maxColorsPerStack, balance);
                }

                var stack = await _stackFactory.CreateStack(
                    _levelLifetime,
                    colors,
                    position,
                    isPlayerStack: true
                );

                stack.transform.SetParent(_playerStacksContainer);
                stack.SetInteractable(true);
                _currentStacks.Add(stack);
            }

            _remainingStacks.Value = _currentStacks.Count;
            _currentStacksProperty.Value = _currentStacks;
        }
        
        public void OnStackPlaced(HexStack stack)
        {
            if (_currentStacks.Contains(stack))
            {
                _currentStacks.Remove(stack);
                _remainingStacks.Value = _currentStacks.Count;
                _currentStacksProperty.Value = _currentStacks;

                if (_currentStacks.Count == 0)
                {
                    _onAllStacksPlaced.Fire(Unit.Instance);
                }
            }
        }

        private List<HexColorType> GenerateFriendlyPlayerStackColors()
        {
            var minSize = _gameplayConfig.MinStackSize;
            var maxSize = _gameplayConfig.MaxStackSize;
            var maxColorsPerStack = Mathf.Clamp(_gameplayConfig.MaxColorsPerStack, 1, 3);
            var balance = _gameplayConfig.BalanceColorSegments;

            var enabledColors = _hexColorConfig.GetEnabledColorTypes();
            if (enabledColors.Count == 0)
                enabledColors.Add(HexColorType.Green);

            var occupied = _gridService.GetOccupiedCells();
            var topCounts = new Dictionary<HexColorType, int>();
            foreach (var cell in occupied)
            {
                var top = cell.Stack?.TopColorType;
                if (top.HasValue)
                {
                    topCounts.TryAdd(top.Value, 0);
                    topCounts[top.Value]++;
                }
            }

            int stackSize = Random.Range(minSize, maxSize + 1);
            int segments = Random.Range(1, Mathf.Min(maxColorsPerStack, stackSize) + 1);
            var sizes = balance ? DistributeSizeBalanced(stackSize, segments) : DistributeSize(stackSize, segments);

            var result = new List<HexColorType>(stackSize);
            var used = new HashSet<HexColorType>();

            for (int i = 0; i < sizes.Count; i++)
            {
                HexColorType color;
                if (i == sizes.Count - 1)
                {
                    color = PickTopColor();
                }
                else
                {
                    // Для нижних сегментов не используем слишком много разных цветов — выбираем до 2
                    var candidates = new List<HexColorType>(enabledColors);
                    if (used.Count >= 2)
                    {
                        // Сужаем к уже использованным, чтобы не раздувать разнообразие
                        candidates = new List<HexColorType>(used);
                    }
                    color = candidates[Random.Range(0, candidates.Count)];
                }

                used.Add(color);
                for (int j = 0; j < sizes[i]; j++)
                {
                    result.Add(color);
                }
            }
            return result;

            HexColorType PickTopColor()
            {
                if (topCounts.Count > 0)
                {
                    var best = HexColorType.Green;
                    var bestCount = -1;
                    foreach (var kvp in topCounts)
                    {
                        if (kvp.Value > bestCount)
                        {
                            best = kvp.Key;
                            bestCount = kvp.Value;
                        }
                    }
                    return best;
                }
                return enabledColors[Random.Range(0, enabledColors.Count)];
            }
        }

        private List<HexColorType> GenerateNeutralPlayerStackColors(int minSize, int maxSize, int maxColorsPerStack, bool balance)
        {
            var enabledColors = _hexColorConfig.GetEnabledColorTypes();
            if (enabledColors.Count == 0)
                enabledColors.Add(HexColorType.Green);

            var stackSize = Random.Range(minSize, maxSize + 1);
            var segments = Random.Range(1, Mathf.Min(maxColorsPerStack, stackSize) + 1);
            var sizes = balance ? DistributeSizeBalanced(stackSize, segments) : DistributeSize(stackSize, segments);

            var result = new List<HexColorType>(stackSize);
            var used = new HashSet<HexColorType>();
            for (int i = 0; i < sizes.Count; i++)
            {
                var candidates = new List<HexColorType>(enabledColors);
                if (used.Count < enabledColors.Count)
                {
                    candidates.RemoveAll(c => used.Contains(c));
                }
                var color = candidates[Random.Range(0, candidates.Count)];
                used.Add(color);
                for (int j = 0; j < sizes[i]; j++)
                {
                    result.Add(color);
                }
            }
            return result;
        }

        private static List<int> DistributeSize(int totalSize, int segments)
        {
            var sizes = new List<int>();
            var remaining = totalSize;
            for (int i = 0; i < segments; i++)
            {
                if (i == segments - 1)
                {
                    sizes.Add(remaining);
                }
                else
                {
                    var size = Random.Range(1, Mathf.Max(2, remaining - (segments - i - 1)));
                    sizes.Add(size);
                    remaining -= size;
                }
            }
            return sizes;
        }

        private static List<int> DistributeSizeBalanced(int totalSize, int segments)
        {
            var sizes = new List<int>();
            var remaining = totalSize;
            for (int i = 0; i < segments; i++)
            {
                var segmentsLeft = segments - i;
                if (i == segments - 1)
                {
                    sizes.Add(remaining);
                }
                else
                {
                    var avg = Mathf.Max(1, Mathf.FloorToInt((float)remaining / segmentsLeft));
                    var min = Mathf.Max(1, avg - 1);
                    var max = Mathf.Max(min + 1, avg + 1);
                    var size = Random.Range(min, Mathf.Min(max, remaining - (segmentsLeft - 1)) + 1);
                    size = Mathf.Clamp(size, 1, remaining - (segmentsLeft - 1));
                    sizes.Add(size);
                    remaining -= size;
                }
            }
            for (int i = 0; i < sizes.Count - 1; i++)
            {
                if (sizes[i] == 1 && sizes[i + 1] > 2)
                {
                    sizes[i]++;
                    sizes[i + 1]--;
                }
            }
            return sizes;
        }
    }
}
