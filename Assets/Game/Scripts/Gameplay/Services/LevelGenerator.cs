using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Gameplay.Configs;
using Game.Gameplay.HexGrid;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Game.Gameplay.Services
{
    public interface ILevelGenerator
    {
        UniTask GenerateLevel(Lifetime lifetime);
    }

    public class LevelGenerator : ILevelGenerator
    {
        private readonly GameplayConfig _gameplayConfig;
        private readonly HexColorConfig _hexColorConfig;
        private readonly IGridService _gridService;
        private readonly IStackFactory _stackFactory;

        public LevelGenerator(
            GameplayConfig gameplayConfig,
            HexColorConfig hexColorConfig,
            IGridService gridService,
            IStackFactory stackFactory)
        {
            _gameplayConfig = gameplayConfig;
            _hexColorConfig = hexColorConfig;
            _gridService = gridService;
            _stackFactory = stackFactory;
        }

        public async UniTask GenerateLevel(Lifetime lifetime)
        {
            while (!_gridService.IsInitialized.Value && lifetime.IsAlive)
            {
                await UniTask.Yield();
            }

            if (!lifetime.IsAlive) return;

            var emptyCells = _gridService.GetEmptyCells();
            
            if (emptyCells.Count == 0)
            {
                Debug.LogWarning("[LevelGenerator] No empty cells available");
                return;
            }

            ShuffleList(emptyCells);

            var totalCells = emptyCells.Count + _gridService.GetOccupiedCells().Count;
            var ratio = _gameplayConfig.InitialFillRatio;
            var desiredStacksByRatio = Mathf.Clamp(Mathf.RoundToInt(totalCells * ratio), 1, emptyCells.Count);

            var difficulty = _gameplayConfig.Difficulty;
            var minSize = _gameplayConfig.MinInitialStackSize;
            var maxSize = _gameplayConfig.MaxInitialStackSize;
            var maxColorsPerStack = _gameplayConfig.MaxColorsPerStack;

            switch (difficulty)
            {
                case Difficulty.Easy:
                    maxColorsPerStack = Mathf.Max(1, Mathf.RoundToInt(maxColorsPerStack * 0.6f));
                    maxSize = Mathf.Max(minSize + 1, Mathf.RoundToInt(maxSize * 0.85f));
                    break;
                case Difficulty.Normal:
                    break;
                case Difficulty.Hard:
                    maxColorsPerStack = Mathf.Clamp(maxColorsPerStack + 1, 1, _gameplayConfig.MaxColorsPerStack);
                    maxSize = Mathf.Clamp(maxSize + 1, minSize + 1, _gameplayConfig.MaxInitialStackSize);
                    break;
            }

            var targetMoves = Mathf.Clamp(_gameplayConfig.TargetMovesToSolve, 1, 5);
            var requiredTopPairs = Mathf.Max(2, targetMoves);

            var baseStackCount = desiredStacksByRatio;
            
            var minStacksForPairs = requiredTopPairs * 2;
            var stackCount = Mathf.Max(minStacksForPairs, baseStackCount);
            stackCount = Mathf.Min(stackCount, emptyCells.Count);

            var generatedStacks = await GenerateSolvableStacks(lifetime, stackCount, minSize, maxSize, maxColorsPerStack);

            for (int i = 0; i < generatedStacks.Count && i < emptyCells.Count; i++)
            {
                var cell = emptyCells[i];
                var stack = generatedStacks[i];
                
                var worldPos = _gridService.GetWorldPosition(cell.Coord);
                stack.transform.position = worldPos;
                cell.PlaceStack(stack);
            }
        }
        
        private async UniTask<List<HexStack>> GenerateSolvableStacks(Lifetime lifetime, int count, int minSize, int maxSize, int maxColorsPerStack)
        {
            var stacks = new List<HexStack>();
            var enabledColors = _hexColorConfig.GetEnabledColorTypes();
            var colorPairs = new Dictionary<HexColorType, int>();

            float pairingProbability = _gameplayConfig.GetPairingProbability();
            bool balanceSegments = _gameplayConfig.BalanceColorSegments;

            for (int i = 0; i < count; i++)
            {
                var colors = GenerateStackColors(colorPairs, enabledColors, minSize, maxSize, maxColorsPerStack, pairingProbability, balanceSegments);

                var stack = await _stackFactory.CreateStack(
                    lifetime,
                    colors,
                    Vector3.zero,
                    isPlayerStack: false
                );
                
                stacks.Add(stack);
                var topColor = colors[^1];
                colorPairs.TryAdd(topColor, 0);
                colorPairs[topColor]++;
            }

            return stacks;
        }

        private static List<HexColorType> GenerateStackColors(
            Dictionary<HexColorType, int> existingTopColors,
            List<HexColorType> enabledColors,
            int minSize,
            int maxSize,
            int maxColorsPerStack,
            float pairingProbability,
            bool balanceSegments)
        {
            var stackSize = Random.Range(minSize, maxSize + 1);
            var colorSegments = Random.Range(1, Mathf.Min(maxColorsPerStack, stackSize) + 1);

            var segmentSizes = balanceSegments
                ? DistributeSizeBalanced(stackSize, colorSegments)
                : DistributeSize(stackSize, colorSegments);

            var colors = new List<HexColorType>(stackSize);
            for (int i = 0; i < segmentSizes.Count; i++)
            {
                HexColorType color;
                if (i == segmentSizes.Count - 1)
                {
                    color = ChooseTopColor(existingTopColors, enabledColors, pairingProbability);
                }
                else
                {
                    color = enabledColors[Random.Range(0, enabledColors.Count)];
                }

                for (int j = 0; j < segmentSizes[i]; j++)
                {
                    colors.Add(color);
                }
            }

            return colors;
        }

        private static HexColorType ChooseTopColor(
            Dictionary<HexColorType, int> existingTopColors,
            List<HexColorType> enabledColors,
            float pairingProbability)
        {
            if (Random.value < pairingProbability && existingTopColors.Count > 0)
            {
                var unpaired = existingTopColors
                    .Where(kvp => kvp.Value % 2 != 0)
                    .Select(kvp => kvp.Key)
                    .ToList();

                if (unpaired.Count > 0)
                {
                    return unpaired[Random.Range(0, unpaired.Count)];
                }
            }

            return enabledColors[Random.Range(0, enabledColors.Count)];
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

        public bool ValidateLevelSolvability()
        {
            var occupiedCells = _gridService.GetOccupiedCells();
            if (occupiedCells.Count == 0)
                return true;

            foreach (var cell in occupiedCells)
            {
                if (cell.Stack == null) continue;
                
                var topColor = cell.Stack.TopColorType;
                if (!topColor.HasValue) continue;

                var neighbors = _gridService.GetNeighbors(cell.Coord);
                foreach (var neighbor in neighbors)
                {
                    if (neighbor.Stack != null && neighbor.Stack.TopColorType == topColor)
                    {
                        return true;
                    }
                }
            }

            var emptyCells = _gridService.GetEmptyCells();
            if (emptyCells.Count == 0)
                return false;

            var topColors = occupiedCells
                .Where(c => c.Stack?.TopColorType != null)
                .Select(c => c.Stack.TopColorType.Value)
                .ToList();

            return topColors.GroupBy(c => c).Any(g => g.Count() > 1);
        }

        private static void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}