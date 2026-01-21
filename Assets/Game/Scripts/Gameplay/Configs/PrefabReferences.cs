using Game.Gameplay.HexGrid;
using Game.Utilities.Addressables;
using UnityEngine;

namespace Game.Gameplay.Configs
{
    
    [System.Serializable]
    public class HexPieceReference : ComponentReference<HexPiece>
    {
        public HexPieceReference(GameObject prefab, string guid = "") : base(prefab, guid) { }
        public HexPieceReference() : base() { }
    }
    
    [System.Serializable]
    public class HexStackReference : ComponentReference<HexStack>
    {
        public HexStackReference(GameObject prefab, string guid = "") : base(prefab, guid) { }
        public HexStackReference() : base() { }
    }
    
    [System.Serializable]
    public class GridCellReference : ComponentReference<GridCell>
    {
        public GridCellReference(GameObject prefab, string guid = "") : base(prefab, guid) { }
        public GridCellReference() : base() { }
    }
    
    [System.Serializable]
    public class CellBackgroundReference : ComponentReference<CellBackground>
    {
        public CellBackgroundReference(GameObject prefab, string guid = "") : base(prefab, guid) { }
        public CellBackgroundReference() : base() { }
    }
}

