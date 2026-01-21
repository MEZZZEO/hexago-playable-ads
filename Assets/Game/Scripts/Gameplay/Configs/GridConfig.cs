using System;
using UnityEngine;

namespace Game.Gameplay.Configs
{
    public enum GridShape
    {
        Hexagon = 0,
        Rectangle = 1,
        Diamond = 2,
        Triangle = 3
    }

    [CreateAssetMenu(fileName = "GridConfig", menuName = "Game/Configs/GridConfig")]
    public class GridConfig : ScriptableObject
    {
        [Header("Grid Shape")]
        [SerializeField] private GridShape _shape = GridShape.Hexagon;
        
        [Header("Size Settings")]
        [Tooltip("Радиус для гексагональной формы")]
        [SerializeField, Range(1, 10)] private int _radius = 3;
        
        [Tooltip("Ширина для прямоугольной формы")]
        [SerializeField, Range(3, 15)] private int _width = 5;
        
        [Tooltip("Высота для прямоугольной формы")]
        [SerializeField, Range(3, 15)] private int _height = 5;
        
        [Header("Cell Settings")]
        [Tooltip("Размер ячейки")]
        [SerializeField, Range(0.5f, 3f)] private float _cellSize = 1f;
        
        [Tooltip("Отступ между ячейками")]
        [SerializeField, Range(0f, 0.5f)] private float _cellSpacing = 0.1f;
        
        [Header("Visual Settings")]
        [SerializeField] private Color _emptyCellColor = new(0.3f, 0.3f, 0.3f, 0.5f);
        [SerializeField] private Color _highlightColor = new(0.5f, 1f, 0.5f, 0.7f);
        [SerializeField] private Color _invalidPlacementColor = new(1f, 0.3f, 0.3f, 0.7f);

        public GridShape Shape => _shape;
        public int Radius => _radius;
        public int Width => _width;
        public int Height => _height;
        public float CellSize => _cellSize;
        public float CellSpacing => _cellSpacing;
        public Color EmptyCellColor => _emptyCellColor;
        public Color HighlightColor => _highlightColor;
        public Color InvalidPlacementColor => _invalidPlacementColor;

        public float TotalCellSize => _cellSize + _cellSpacing;

        public float HorizontalSpacing => TotalCellSize * Mathf.Sqrt(3f);

        public float VerticalSpacing => TotalCellSize * 1.5f;
    }
}

