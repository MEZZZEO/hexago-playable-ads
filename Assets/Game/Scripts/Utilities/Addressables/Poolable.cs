using Cysharp.Threading.Tasks;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Game.Utilities.Addressables
{
    public abstract class Poolable : MonoBehaviour
    {
        public abstract UniTask OnRent(Lifetime lifetime);
        public abstract UniTask OnReturn();
    }
}
