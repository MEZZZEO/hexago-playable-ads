using Cysharp.Threading.Tasks;
using Game.Utilities.Addressables;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Game.Gameplay.HexGrid
{
    public class GridCell : Poolable
    {
        [SerializeField] private MeshRenderer _highlightRenderer;
        [SerializeField] private Collider _collider;
        [SerializeField] private CellBackground _cellBackground;
        
        private HexCoord _coord;
        private HexStack _stack;
        private Color _normalColor;
        private Color _highlightColor;
        private Color _invalidColor;
        
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");
        private MaterialPropertyBlock _propertyBlock;

        public HexCoord Coord => _coord;
        public HexStack Stack => _stack;
        public bool IsOccupied => _stack != null && !_stack.IsDestroyed;
        public bool IsEmpty => _stack == null || _stack.IsDestroyed;
        
        public bool CanPlaceStack => _stack == null || _stack.IsEmpty || _stack.IsDestroyed;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            
            if (_highlightRenderer == null)
                _highlightRenderer = GetComponentInChildren<MeshRenderer>();
            
            if (_collider == null)
                _collider = GetComponent<Collider>();
            
            if (_cellBackground == null)
                _cellBackground = GetComponentInChildren<CellBackground>();
        }

        public override UniTask OnRent(Lifetime lifetime)
        {
            _stack = null;
            SetHighlight(false, true);
            return UniTask.CompletedTask;
        }

        public override UniTask OnReturn()
        {
            _stack = null;
            return UniTask.CompletedTask;
        }

        public void Initialize(HexCoord coord, Vector3 worldPosition, Color normalColor, Color highlightColor, Color invalidColor)
        {
            _coord = coord;
            _normalColor = normalColor;
            _highlightColor = highlightColor;
            _invalidColor = invalidColor;
            
            transform.position = worldPosition;
            SetColor(_normalColor);
        }

        public void PlaceStack(HexStack stack)
        {
            _stack = stack;
            if (stack != null)
            {
                stack.SetGridPosition(_coord, transform.position);
            }
        }

        public HexStack RemoveStack()
        {
            var stack = _stack;
            _stack = null;
            stack?.RemoveFromGrid();
            return stack;
        }
        
        public void SetHighlight(bool highlighted, bool isValid = true)
        {
            if (_highlightRenderer != null)
            {
                Color targetColor;
                if (highlighted)
                {
                    targetColor = isValid ? _highlightColor : _invalidColor;
                }
                else
                {
                    targetColor = _normalColor;
                }
                
                SetColor(targetColor);
            }
        }

        private void SetColor(Color color)
        {
            if (_highlightRenderer != null)
            {
                _highlightRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(ColorProperty, color);
                _highlightRenderer.SetPropertyBlock(_propertyBlock);
            }
        }

        public void SetBackgroundScale(float scale)
        {
            if (_cellBackground != null)
            {
                _cellBackground.SetScale(scale);
            }
        }
    }
}