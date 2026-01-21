using UnityEngine;

namespace Game.Gameplay.Configs
{
    public enum Difficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2
    }
    
    [CreateAssetMenu(fileName = "GameplayConfig", menuName = "Game/Configs/GameplayConfig")]
    public class GameplayConfig : ScriptableObject
    {
        [Header("Stack Settings")]
        [Tooltip("Минимальный размер стопки")]
        [SerializeField, Range(1, 5)] private int _minStackSize = 2;
        
        [Tooltip("Максимальный размер стопки")]
        [SerializeField, Range(3, 12)] private int _maxStackSize = 8;
        
        [Tooltip("Максимальное количество цветов в одной стопке")]
        [SerializeField, Range(1, 5)] private int _maxColorsPerStack = 4;
        
        [Header("Player Settings")]
        [Tooltip("Количество стопок доступных игроку")]
        [SerializeField, Range(1, 5)] private int _playerStacksCount = 3;
        
        [Header("Merge Settings")]
        [Tooltip("Порог для удаления (10 гексов одного цвета)")]
        [SerializeField, Range(5, 15)] private int _mergeThreshold = 10;
        
        [Tooltip("Базовая скорость слияния")]
        [SerializeField, Range(0.5f, 3f)] private float _baseMergeSpeed = 1f;
        
        [Tooltip("Множитель увеличения скорости с каждой стопкой")]
        [SerializeField, Range(1f, 2f)] private float _speedMultiplier = 1.3f;
        
        [Header("Animation Settings")]
        [Tooltip("Длительность перелёта гекса")]
        [SerializeField, Range(0.1f, 1f)] private float _hexFlyDuration = 0.3f;
        
        [Tooltip("Высота дуги перелёта гекса")]
        [SerializeField, Range(0.1f, 10f)] private float _hexArcHeight = 0.5f;
        
        [Tooltip("Вертикальный оффсет между гексами в стопке")]
        [SerializeField, Range(0.05f, 0.5f)] private float _hexStackOffset = 0.15f;
        
        [Tooltip("Базовый отступ первого гекса от земли")]
        [SerializeField, Range(0f, 0.5f)] private float _hexBaseOffset = 0.1f;
        
        [Tooltip("Размер фона ячейки (scale)")]
        [SerializeField, Range(0.5f, 3f)] private float _cellBackgroundScale = 1.2f;
        
        [Header("Drag Settings")]
        [Tooltip("Высота подъёма стопки при перетаскивании")]
        [SerializeField, Range(0.5f, 5f)] private float _dragHeight = 2f;
        
        [Tooltip("Расстояние привязки к ячейке")]
        [SerializeField, Range(0.1f, 1f)] private float _snapDistance = 0.5f;
        
        [Header("Level Generation")]
        [Tooltip("Начальное количество стопок на поле")]
        [SerializeField, Range(3, 15)] private int _initialStacksOnField = 5;
        
        [Tooltip("Минимальный размер начальных стопок")]
        [SerializeField, Range(1, 3)] private int _minInitialStackSize = 2;
        
        [Tooltip("Максимальный размер начальных стопок")]
        [SerializeField, Range(3, 8)] private int _maxInitialStackSize = 5;
        
        [Tooltip("Доля заполненности сетки при старте (0..1)")] 
        [SerializeField, Range(0f, 1f)] private float _initialFillRatio = 0.35f;
        
        [Tooltip("Целевое число установок стопок для прохождения уровня (примерно)")] 
        [SerializeField, Range(1, 5)] private int _targetMovesToSolve = 2;
        
        [Header("Difficulty Settings")]
        [Tooltip("Сложность уровня. Влияет на генерацию: меньше стопок/цветов на Easy, больше на Hard.")]
        [SerializeField] private Difficulty _difficulty = Difficulty.Easy;
        
        [Tooltip("Вероятность создавать пары верхних цветов при генерации поля (Easy/Normal/Hard)")]
        [SerializeField, Range(0f, 1f)] private float _pairingProbabilityEasy = 0.9f;
        [SerializeField, Range(0f, 1f)] private float _pairingProbabilityNormal = 0.7f;
        [SerializeField, Range(0f, 1f)] private float _pairingProbabilityHard = 0.4f;
        
        [Tooltip("Балансировать сегменты цветов в стопках (избегать 1 vs 6 и т.п.)")] 
        [SerializeField] private bool _balanceColorSegments = true;
        
        [Header("Prefab References")]
        [SerializeField] private HexPieceReference _hexPiecePrefab;
        [SerializeField] private HexStackReference _hexStackPrefab;
        [SerializeField] private GridCellReference _gridCellPrefab;
        [SerializeField] private CellBackgroundReference _cellBackgroundPrefab;
        
        [Header("Packshot Settings")]
        [Tooltip("Задержка до показа пэкшота после завершения обучения (сек)")]
        [SerializeField, Range(1f, 120f)] private float _packshotDelayAfterTutorialSec = 20f;

        public int MinStackSize => _minStackSize;
        public int MaxStackSize => _maxStackSize;
        public int MaxColorsPerStack => _maxColorsPerStack;

        public int PlayerStacksCount => _playerStacksCount;

        public int MergeThreshold => _mergeThreshold;
        public float BaseMergeSpeed => _baseMergeSpeed;
        public float SpeedMultiplier => _speedMultiplier;

        public float HexFlyDuration => _hexFlyDuration;
        public float HexArcHeight => _hexArcHeight;
        public float HexStackOffset => _hexStackOffset;
        public float HexBaseOffset => _hexBaseOffset;
        public float CellBackgroundScale => _cellBackgroundScale;

        public float DragHeight => _dragHeight;
        public float SnapDistance => _snapDistance;

        public int InitialStacksOnField => _initialStacksOnField;
        public int MinInitialStackSize => _minInitialStackSize;
        public int MaxInitialStackSize => _maxInitialStackSize;
        public float InitialFillRatio => _initialFillRatio;
        public int TargetMovesToSolve => _targetMovesToSolve;

        public Difficulty Difficulty => _difficulty;
        public bool BalanceColorSegments => _balanceColorSegments;
        public float GetPairingProbability()
        {
            return _difficulty switch
            {
                Difficulty.Easy => _pairingProbabilityEasy,
                Difficulty.Hard => _pairingProbabilityHard,
                _ => _pairingProbabilityNormal
            };
        }

        public HexPieceReference HexPiecePrefab => _hexPiecePrefab;
        public HexStackReference HexStackPrefab => _hexStackPrefab;
        public GridCellReference GridCellPrefab => _gridCellPrefab;
        public CellBackgroundReference CellBackgroundPrefab => _cellBackgroundPrefab;

        public float PackshotDelayAfterTutorialSec => _packshotDelayAfterTutorialSec;
    }
}
