using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JetBrains.Lifetimes;

namespace Game.Utilities.Addressables
{
    public class DefaultAddressablesPool : IPool
    {
        private readonly Func<ComponentReference, ILocalPool> _poolFactory;
        private readonly Dictionary<string, ILocalPool> _pools = new();

        public DefaultAddressablesPool(Func<ComponentReference, ILocalPool> poolFactory)
        {
            _poolFactory = poolFactory;
        }

        public UniTask Warmup(Lifetime lifetime, ComponentReference reference, int count, int limit)
        {
            return GetOrCreatePool(reference).Warmup(lifetime, count, limit);
        }

        public async UniTask<T> Rent<T>(Lifetime lifetime, ComponentReference<T> reference) where T : Poolable
        {
            var poolable = await GetOrCreatePool(reference).Rent(lifetime);
            
            if (poolable is T typedComponent)
            {
                return typedComponent;
            }

            var component = poolable.gameObject.GetComponent<T>();
            if (component != null)
            {
                return component;
            }

            throw new InvalidOperationException(
                $"Failed to get component of type {typeof(T).Name} from poolable {poolable.GetType().Name}. " +
                $"Make sure the prefab contains the required component.");
        }

        public UniTask<Poolable> Rent(Lifetime lifetime, ComponentReference reference)
        {
            return GetOrCreatePool(reference).Rent(lifetime);
        }

        private ILocalPool GetOrCreatePool(ComponentReference reference)
        {
            if (!_pools.TryGetValue(reference.AssetGUID, out var pool))
            {
                pool = _poolFactory.Invoke(reference);
                _pools.Add(reference.AssetGUID, pool);
            }

            return pool;
        }
    }
}
