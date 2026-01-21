using Cysharp.Threading.Tasks;
using JetBrains.Lifetimes;

namespace Game.Utilities.Addressables
{
    public interface ILocalPool
    {
        ComponentReference Reference { get; }
        UniTask Warmup(Lifetime lifetime, int count, int limit);
        UniTask<Poolable> Rent(Lifetime lifetime);
    }
}
