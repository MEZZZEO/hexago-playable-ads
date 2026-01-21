using System.Collections.Generic;
using Game.Gameplay.Configs;
using UnityEngine;

namespace Game.Gameplay.HexGrid
{
    public static class GridShapeGenerator
    {
        public static List<HexCoord> GenerateCoords(GridConfig config)
        {
            return config.Shape switch
            {
                GridShape.Hexagon => GenerateHexagon(config.Radius),
                GridShape.Rectangle => GenerateRectangle(config.Width, config.Height),
                GridShape.Diamond => GenerateDiamond(config.Radius),
                GridShape.Triangle => GenerateTriangle(config.Radius),
                _ => GenerateHexagon(config.Radius)
            };
        }

        private static List<HexCoord> GenerateHexagon(int radius)
        {
            var coords = new List<HexCoord>();
            
            for (int q = -radius; q <= radius; q++)
            {
                var r1 = Mathf.Max(-radius, -q - radius);
                var r2 = Mathf.Min(radius, -q + radius);
                
                for (int r = r1; r <= r2; r++)
                {
                    coords.Add(new HexCoord(q, r));
                }
            }
            
            return coords;
        }

        private static List<HexCoord> GenerateRectangle(int width, int height)
        {
            var coords = new List<HexCoord>();
            
            var halfHeight = height / 2;
            var isEvenHeight = height % 2 == 0;
            
            for (int r = -halfHeight; r <= halfHeight - (isEvenHeight ? 1 : 0); r++)
            {
                // Определяем ширину ряда
                int rowWidth = width;
                bool isOffsetRow = r % 2 != 0;
                
                // Для четной высоты: крайние ряды широкие
                // Для нечётной высоты: крайние ряды узкие
                if (isEvenHeight)
                {
                    // Чётная высота - начинаем и заканчиваем широким рядом
                    if (!isOffsetRow)
                        rowWidth = width;
                    else
                        rowWidth = width - 1;
                }
                else
                {
                    // Нечётная высота - начинаем и заканчиваем узким рядом  
                    if (isOffsetRow)
                        rowWidth = width;
                    else
                        rowWidth = width - 1;
                }
                
                // Центрирование по горизонтали
                int offset = r / 2;
                int halfWidth = rowWidth / 2;
                
                for (int i = 0; i < rowWidth; i++)
                {
                    int q = i - halfWidth - offset;
                    coords.Add(new HexCoord(q, r));
                }
            }
            
            return coords;
        }

        private static List<HexCoord> GenerateDiamond(int radius)
        {
            var coords = new List<HexCoord>();
            
            for (int q = 0; q <= radius * 2; q++)
            {
                for (int r = 0; r <= radius * 2; r++)
                {
                    coords.Add(new HexCoord(q - radius, r - radius));
                }
            }
            
            return coords;
        }

        private static List<HexCoord> GenerateTriangle(int radius)
        {
            var coords = new List<HexCoord>();
            
            for (int q = 0; q <= radius; q++)
            {
                for (int r = 0; r <= radius - q; r++)
                {
                    coords.Add(new HexCoord(q, r));
                }
            }
            
            return coords;
        }

        public static Vector3 CalculateCenter(List<HexCoord> coords, float cellSize)
        {
            if (coords.Count == 0) return Vector3.zero;
            
            Vector3 sum = Vector3.zero;
            foreach (var coord in coords)
            {
                sum += coord.ToWorldPosition(cellSize);
            }
            
            return sum / coords.Count;
        }
    }
}