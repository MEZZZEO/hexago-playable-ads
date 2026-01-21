using Cysharp.Threading.Tasks;
using Game.Utilities.Addressables;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Game.Gameplay.HexGrid
{
    public class CellBackground : Poolable
    {
        public override UniTask OnRent(Lifetime lifetime)
        {
            transform.localScale = Vector3.one;
            return UniTask.CompletedTask;
        }

        public override UniTask OnReturn()
        {
            return UniTask.CompletedTask;
        }
        
        public void SetScale(float scale)
        {
            transform.localScale = Vector3.one * scale;
        }
    }
}