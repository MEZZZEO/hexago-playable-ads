using Cysharp.Threading.Tasks;
using JetBrains.Lifetimes;

namespace Game.Utilities.Addressables
{
    public static class PooledAddressables
    {
        private static IPool _pool;

        public static void Initialize(IPool pool)
        {
            _pool = pool;
        }

        public static UniTask<T> RentLocal<T>(this ComponentReference<T> reference, Lifetime lifetime) where T : Poolable
        {
            return _pool.Rent(lifetime, reference);
        }
        
        public static UniTask Warmup(this ComponentReference reference, Lifetime lifetime, int count, int limit)
        {
            return _pool.Warmup(lifetime, reference, count, limit);
        }
    }
}
