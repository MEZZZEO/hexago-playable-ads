using UnityEngine;
using Zenject;

namespace Game.Utilities.Addressables
{
    public class PooledAddressablesInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            var poolContainer = new GameObject("Pool");
            DontDestroyOnLoad(poolContainer);

            PooledAddressables.Initialize(new DefaultAddressablesPool(PoolFactory));
            return;

            ILocalPool PoolFactory(ComponentReference reference)
            {
                var localPool = Container.Instantiate<LocalPool>(new object[] {reference, poolContainer.transform});
                return localPool;
            }
        }
    }
}
