using Cysharp.Threading.Tasks;
using JetBrains.Lifetimes;

namespace Game.Utilities.Addressables
{
    public class PoolableBehaviour : Poolable
    {
        public override UniTask OnRent(Lifetime lifetime)
        {
            return UniTask.CompletedTask;
        }

        public override UniTask OnReturn()
        {
            return UniTask.CompletedTask;
        }
    }
}
