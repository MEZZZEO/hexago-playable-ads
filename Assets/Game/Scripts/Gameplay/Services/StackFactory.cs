using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Gameplay.Configs;
using Game.Gameplay.HexGrid;
using Game.Utilities.Addressables;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Game.Gameplay.Services
{
    public interface IStackFactory
    {
        UniTask<HexStack> CreateStack(Lifetime lifetime, List<HexColorType> colors, Vector3 position, bool isPlayerStack = false);
        UniTask<HexStack> CreateRandomStack(Lifetime lifetime, int minSize, int maxSize, int maxColors, Vector3 position, bool isPlayerStack = false);
    }

    public class StackFactory : IStackFactory
    {
        private readonly GameplayConfig _gameplayConfig;
        private readonly HexColorConfig _hexColorConfig;

        public StackFactory(GameplayConfig gameplayConfig, HexColorConfig hexColorConfig)
        {
            _gameplayConfig = gameplayConfig;
            _hexColorConfig = hexColorConfig;
        }

        public async UniTask<HexStack> CreateStack(Lifetime lifetime, List<HexColorType> colors, Vector3 position, bool isPlayerStack = false)
        {
            var stackLifetimeDef = lifetime.CreateNested();
            
            var stack = await _gameplayConfig.HexStackPrefab.RentLocal(stackLifetimeDef.Lifetime);
            stack.Initialize(_gameplayConfig.HexStackOffset, _gameplayConfig.HexBaseOffset, isPlayerStack);
            stack.SetLifetimeDefinition(stackLifetimeDef);
            stack.transform.position = position;

            foreach (var colorType in colors)
            {
                var pieceLifetimeDef = lifetime.CreateNested();
                
                var piece = await _gameplayConfig.HexPiecePrefab.RentLocal(pieceLifetimeDef.Lifetime);
                var color = _hexColorConfig.GetColor(colorType);
                piece.SetColor(colorType, color);
                piece.SetLifetimeDefinition(pieceLifetimeDef);
                
                stack.AddPiece(piece);
            }

            return stack;
        }

        public async UniTask<HexStack> CreateRandomStack(Lifetime lifetime, int minSize, int maxSize, int maxColors, Vector3 position, bool isPlayerStack = false)
        {
            var colors = GenerateRandomColors(minSize, maxSize, maxColors);
            return await CreateStack(lifetime, colors, position, isPlayerStack);
        }

        private List<HexColorType> GenerateRandomColors(int minSize, int maxSize, int maxColors)
        {
            var enabledColors = _hexColorConfig.GetEnabledColorTypes();
            if (enabledColors.Count == 0)
            {
                enabledColors.Add(HexColorType.Green);
            }

            var stackSize = Random.Range(minSize, maxSize + 1);
            var colors = new List<HexColorType>(stackSize);

            var colorSegments = Mathf.Min(maxColors, Random.Range(1, maxColors + 1));
            colorSegments = Mathf.Min(colorSegments, stackSize);

            // Если включен баланс сегментов в конфиге — используем выровненное распределение
            var segmentSizes = _gameplayConfig.BalanceColorSegments
                ? DistributeSizeBalanced(stackSize, colorSegments)
                : DistributeSize(stackSize, colorSegments);

            var usedColors = new List<HexColorType>();

            foreach (var segmentSize in segmentSizes)
            {
                var availableColors = new List<HexColorType>(enabledColors);
                if (usedColors.Count < enabledColors.Count)
                {
                    availableColors.RemoveAll(c => usedColors.Contains(c));
                }
                
                var segmentColor = availableColors[Random.Range(0, availableColors.Count)];
                usedColors.Add(segmentColor);

                for (int i = 0; i < segmentSize; i++)
                {
                    colors.Add(segmentColor);
                }
            }

            return colors;
        }

        private static List<int> DistributeSize(int totalSize, int segments)
        {
            var sizes = new List<int>();
            var remaining = totalSize;

            for (int i = 0; i < segments; i++)
            {
                int minSegmentSize = 1;
                int maxSegmentSize = remaining - (segments - i - 1);
                
                if (i == segments - 1)
                {
                    sizes.Add(remaining);
                }
                else
                {
                    int size = Random.Range(minSegmentSize, Mathf.Max(minSegmentSize + 1, maxSegmentSize / 2 + 1));
                    sizes.Add(size);
                    remaining -= size;
                }
            }

            return sizes;
        }

        private static List<int> DistributeSizeBalanced(int totalSize, int segments)
        {
            var sizes = new List<int>();
            int remaining = totalSize;
            for (int i = 0; i < segments; i++)
            {
                int segmentsLeft = segments - i;
                if (i == segments - 1)
                {
                    sizes.Add(remaining);
                }
                else
                {
                    int avg = Mathf.Max(1, Mathf.FloorToInt((float)remaining / segmentsLeft));
                    int min = Mathf.Max(1, avg - 1);
                    int max = Mathf.Max(min + 1, avg + 1);
                    int size = Random.Range(min, Mathf.Min(max, remaining - (segmentsLeft - 1)) + 1);
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