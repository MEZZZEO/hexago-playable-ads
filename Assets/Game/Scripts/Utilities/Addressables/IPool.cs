using Cysharp.Threading.Tasks;
using JetBrains.Lifetimes;

namespace Game.Utilities.Addressables
{
    public interface IPool
    {
        UniTask Warmup(Lifetime lifetime, ComponentReference reference, int count, int limit);
        UniTask<T> Rent<T>(Lifetime lifetime, ComponentReference<T> reference) where T : Poolable;
        UniTask<Poolable> Rent(Lifetime lifetime, ComponentReference reference);
    }
}
