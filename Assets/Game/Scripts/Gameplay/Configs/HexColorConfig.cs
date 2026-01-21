using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Gameplay.Configs
{
    public enum HexColorType
    {
        Green = 0,
        Yellow = 1,
        Red = 2,
        Cyan = 3,
        White = 4,
        Pink = 5,
        Black = 6,
        Blue = 7
    }

    [Serializable]
    public class HexColorData
    {
        [SerializeField] private HexColorType _colorType;
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private bool _isEnabled = true;

        public HexColorType ColorType => _colorType;
        public Color Color => _color;
        public bool IsEnabled => _isEnabled;

        public HexColorData(HexColorType colorType, Color color, bool isEnabled = true)
        {
            _colorType = colorType;
            _color = color;
            _isEnabled = isEnabled;
        }
    }

    [CreateAssetMenu(fileName = "HexColorConfig", menuName = "Game/Configs/HexColorConfig")]
    public class HexColorConfig : ScriptableObject
    {
        [SerializeField] private List<HexColorData> _colors = new();

        public IReadOnlyList<HexColorData> Colors => _colors;

        public IEnumerable<HexColorData> GetEnabledColors()
        {
            return _colors.Where(c => c.IsEnabled);
        }

        public Color GetColor(HexColorType colorType)
        {
            var colorData = _colors.FirstOrDefault(c => c.ColorType == colorType);
            return colorData?.Color ?? Color.white;
        }

        public HexColorType GetRandomEnabledColorType()
        {
            var enabledColors = GetEnabledColors().ToList();
            if (enabledColors.Count == 0)
                return HexColorType.Green;
            
            return enabledColors[UnityEngine.Random.Range(0, enabledColors.Count)].ColorType;
        }

        public List<HexColorType> GetEnabledColorTypes()
        {
            return GetEnabledColors().Select(c => c.ColorType).ToList();
        }
    }
}

