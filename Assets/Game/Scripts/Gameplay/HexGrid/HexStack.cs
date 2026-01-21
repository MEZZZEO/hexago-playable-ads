using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Gameplay.Configs;
using Game.Utilities.Addressables;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Game.Gameplay.HexGrid
{
    public class HexStack : Poolable
    {
        [SerializeField] private Transform _piecesContainer;
        [SerializeField] private Collider _collider;
        
        private readonly List<HexPiece> _pieces = new();
        private HexCoord _gridPosition;
        private bool _isOnGrid;
        private bool _isPlayerStack;
        private float _stackOffset = 0.15f;
        private float _baseOffset = 0.1f;
        private LifetimeDefinition _stackLifetimeDefinition;
        private bool _isDestroyed;

        public IReadOnlyList<HexPiece> Pieces => _pieces;
        public int Count => _pieces.Count;
        public HexCoord GridPosition => _gridPosition;
        public bool IsOnGrid => _isOnGrid;
        public bool IsPlayerStack => _isPlayerStack;
        public bool IsEmpty => _pieces.Count == 0;
        public bool IsDestroyed => _isDestroyed;
            
        public HexPiece TopPiece => _pieces.Count > 0 ? _pieces[^1] : null;
        public HexPiece BottomPiece => _pieces.Count > 0 ? _pieces[0] : null;
        public HexColorType? TopColorType => TopPiece?.ColorType;

        private void Awake()
        {
            if (_piecesContainer == null)
                _piecesContainer = transform;
            
            if (_collider == null)
                _collider = GetComponent<Collider>();
        }

        public override UniTask OnRent(Lifetime lifetime)
        {
            _pieces.Clear();
            _isOnGrid = false;
            _isPlayerStack = false;
            _isDestroyed = false;
            _stackLifetimeDefinition = null;
            
            return UniTask.CompletedTask;
        }

        public override UniTask OnReturn()
        {
            _pieces.Clear();
            _stackLifetimeDefinition = null;
            _isDestroyed = true;
            
            return UniTask.CompletedTask;
        }
        
        public void SetLifetimeDefinition(LifetimeDefinition lifetimeDefinition)
        {
            _stackLifetimeDefinition = lifetimeDefinition;
        }
        
        public void Initialize(float stackOffset, float baseOffset, bool isPlayerStack = false)
        {
            _stackOffset = stackOffset;
            _baseOffset = baseOffset;
            _isPlayerStack = isPlayerStack;
        }
        
        public void AddPiece(HexPiece piece)
        {
            piece.transform.SetParent(_piecesContainer);
            piece.SetStackPosition(_pieces.Count, _stackOffset, _baseOffset);
            _pieces.Add(piece);
            UpdateCollider();
        }

        public List<HexPiece> RemoveTopPiecesOfColor(HexColorType colorType)
        {
            var removed = new List<HexPiece>();
            
            while (_pieces.Count > 0 && TopPiece.ColorType == colorType)
            {
                removed.Add(RemoveTopPiece());
            }
            
            return removed;
        }

        public int GetTopColorCount()
        {
            if (_pieces.Count == 0) return 0;
            
            var topColor = TopPiece.ColorType;
            int count = 0;
            
            for (int i = _pieces.Count - 1; i >= 0; i--)
            {
                if (_pieces[i].ColorType == topColor)
                    count++;
                else
                    break;
            }
            
            return count;
        }

        public void SetGridPosition(HexCoord coord, Vector3 worldPosition)
        {
            _gridPosition = coord;
            _isOnGrid = true;
            transform.position = worldPosition;
        }

        public void RemoveFromGrid()
        {
            _isOnGrid = false;
        }

        public void SetInteractable(bool interactable)
        {
            _collider.enabled = interactable;
        }

        private void UpdateCollider()
        {
            if (_collider is BoxCollider boxCollider)
            {
                float height = Mathf.Max(0.5f, _pieces.Count * _stackOffset);
                boxCollider.center = new Vector3(0, height / 2f, 0);
                boxCollider.size = new Vector3(1f, height, 1f);
            }
        }

        public void RefreshPiecePositions()
        {
            for (int i = 0; i < _pieces.Count; i++)
            {
                _pieces[i].SetStackPosition(i, _stackOffset, _baseOffset);
            }
        }

        public float GetBaseOffset() => _baseOffset;

        public float GetStackOffset() => _stackOffset;

        public void Destroy()
        {
            if (_isDestroyed) return;
            
            _isDestroyed = true;
            
            _stackLifetimeDefinition?.Terminate();
        }

        private HexPiece RemoveTopPiece()
        {
            if (_pieces.Count == 0) return null;
            
            var piece = _pieces[^1];
            _pieces.RemoveAt(_pieces.Count - 1);
            piece.transform.SetParent(null);
            UpdateCollider();
            return piece;
        }
    }
}