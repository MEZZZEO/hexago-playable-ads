using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Gameplay.Configs;
using Game.Gameplay.HexGrid;
using Game.Utilities.Addressables;
using Game.Utilities.Lifetimes;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Game.Gameplay.Services
{
    public interface IGridService
    {
        IReadOnlyDictionary<HexCoord, GridCell> Cells { get; }
        IReadonlyProperty<bool> IsInitialized { get; }
        
        GridCell GetCell(HexCoord coord);
        bool TryGetCell(HexCoord coord, out GridCell cell);
        List<GridCell> GetEmptyCells();
        List<GridCell> GetOccupiedCells();
        List<GridCell> GetNeighbors(HexCoord coord);
        Vector3 GetWorldPosition(HexCoord coord);
        HexCoord GetNearestCoord(Vector3 worldPosition);
        void HighlightCell(HexCoord coord, bool highlight, bool isValid = true);
        void ClearAllHighlights();
    }
    
    public class GridService : IGridService, ILifetimeInitializable
    {
        private readonly GridConfig _gridConfig;
        private readonly GameplayConfig _gameplayConfig;
        private readonly Transform _gridContainer;
        private readonly Dictionary<HexCoord, GridCell> _cells = new();
        private readonly ViewableProperty<bool> _isInitialized = new(false);
        
        private Vector3 _centerOffset;

        public IReadOnlyDictionary<HexCoord, GridCell> Cells => _cells;
        public IReadonlyProperty<bool> IsInitialized => _isInitialized;

        public GridService(GridConfig gridConfig, GameplayConfig gameplayConfig, Transform gridContainer)
        {
            _gridConfig = gridConfig;
            _gameplayConfig = gameplayConfig;
            _gridContainer = gridContainer;
        }

        public void Initialize(Lifetime lifetime)
        {
            BuildGrid(lifetime).Forget();
        }

        private async UniTask BuildGrid(Lifetime lifetime)
        {
            var coords = GridShapeGenerator.GenerateCoords(_gridConfig);
            _centerOffset = GridShapeGenerator.CalculateCenter(coords, _gridConfig.TotalCellSize);

            foreach (var coord in coords)
            {
                var cell = await _gameplayConfig.GridCellPrefab.RentLocal(lifetime);
                var worldPos = GetWorldPosition(coord);
                
                cell.Initialize(
                    coord, 
                    worldPos,
                    _gridConfig.EmptyCellColor,
                    _gridConfig.HighlightColor,
                    _gridConfig.InvalidPlacementColor
                );
                
                cell.SetBackgroundScale(_gameplayConfig.CellBackgroundScale);
                cell.transform.SetParent(_gridContainer);
                _cells[coord] = cell;
            }

            _isInitialized.Value = true;
        }

        public GridCell GetCell(HexCoord coord)
        {
            return _cells.GetValueOrDefault(coord);
        }

        public bool TryGetCell(HexCoord coord, out GridCell cell)
        {
            return _cells.TryGetValue(coord, out cell);
        }

        public bool IsCellEmpty(HexCoord coord)
        {
            return TryGetCell(coord, out var cell) && cell.IsEmpty;
        }

        public List<GridCell> GetEmptyCells()
        {
            var emptyCells = new List<GridCell>();
            foreach (var cell in _cells.Values)
            {
                if (cell.IsEmpty)
                    emptyCells.Add(cell);
            }
            return emptyCells;
        }

        public List<GridCell> GetOccupiedCells()
        {
            var occupiedCells = new List<GridCell>();
            foreach (var cell in _cells.Values)
            {
                if (cell.IsOccupied)
                    occupiedCells.Add(cell);
            }
            return occupiedCells;
        }

        public List<GridCell> GetNeighbors(HexCoord coord)
        {
            var neighbors = new List<GridCell>();
            foreach (var neighborCoord in coord.GetAllNeighbors())
            {
                if (TryGetCell(neighborCoord, out var cell))
                {
                    neighbors.Add(cell);
                }
            }
            return neighbors;
        }

        public Vector3 GetWorldPosition(HexCoord coord)
        {
            return coord.ToWorldPosition(_gridConfig.TotalCellSize) - _centerOffset + _gridContainer.position;
        }

        public HexCoord GetNearestCoord(Vector3 worldPosition)
        {
            var localPos = worldPosition - _gridContainer.position + _centerOffset;
            return HexCoord.FromWorldPosition(localPos, _gridConfig.TotalCellSize);
        }

        public void HighlightCell(HexCoord coord, bool highlight, bool isValid = true)
        {
            if (TryGetCell(coord, out var cell))
            {
                cell.SetHighlight(highlight, isValid);
            }
        }

        public void ClearAllHighlights()
        {
            foreach (var cell in _cells.Values)
            {
                cell.SetHighlight(false);
            }
        }
    }
}

