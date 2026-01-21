using Cysharp.Threading.Tasks;
using Game.Gameplay.Configs;
using Game.Utilities.Addressables;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Game.Gameplay.HexGrid
{
    public class HexPiece : Poolable
    {
        [SerializeField] private MeshRenderer _meshRenderer;

        private static readonly int ColorProperty = Shader.PropertyToID("_Color");

        private MaterialPropertyBlock _propertyBlock;
        private HexColorType _colorType;
        private Color _color;
        
        private LifetimeDefinition _lifetimeDefinition;
        private bool _isDestroyed;

        public HexColorType ColorType => _colorType;
        public bool IsDestroyed => _isDestroyed;

        private void Awake()
        {
            if (_meshRenderer == null)
                _meshRenderer = GetComponentInChildren<MeshRenderer>();
            
            _propertyBlock = new MaterialPropertyBlock();
        }

        public override UniTask OnRent(Lifetime lifetime)
        {
            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;
            _isDestroyed = false;
            return UniTask.CompletedTask;
        }

        public override UniTask OnReturn()
        {
            _lifetimeDefinition = null;
            _isDestroyed = true;
            return UniTask.CompletedTask;
        }

        public void SetLifetimeDefinition(LifetimeDefinition lifetimeDefinition)
        {
            _lifetimeDefinition = lifetimeDefinition;
        }

        public void Destroy()
        {
            if (_isDestroyed) return;
            _isDestroyed = true;
            
            _lifetimeDefinition?.Terminate();
        }

        public void SetColor(HexColorType colorType, Color color)
        {
            _colorType = colorType;
            _color = color;
            
            if (_meshRenderer != null)
            {
                _meshRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(ColorProperty, color);
                _meshRenderer.SetPropertyBlock(_propertyBlock);
            }
        }

        public void SetStackPosition(int index, float stackOffset, float baseOffset = 0f)
        {
            transform.localPosition = new Vector3(0, baseOffset + index * stackOffset, 0);
        }
    }
}