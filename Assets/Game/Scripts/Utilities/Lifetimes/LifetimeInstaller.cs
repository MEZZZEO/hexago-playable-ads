using Game.Utilities.Lifetimes.Extensions;
using JetBrains.Lifetimes;
using Zenject;

namespace Game.Utilities.Lifetimes
{
    public class LifetimeInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container
                .Bind<Lifetime>()
                .FromMethod(GetLifetime)
                .AsSingle()
                .CopyIntoAllSubContainers();
            
            Container.BindInterfacesTo<LifetimeStarter>()
                .FromNewComponentOnRoot()
                .AsSingle()
                .CopyIntoAllSubContainers()
                .NonLazy();

            Container.Bind<LifetimeInitializer>().AsSingle().CopyIntoAllSubContainers();
        }

        private static Lifetime GetLifetime(InjectContext context)
        {
            return context.Container.Resolve<Context>().gameObject.GetLifetime();
        }
    }
}
