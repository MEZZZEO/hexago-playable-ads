using Game.Scripts.View.Core;
using Zenject;

namespace Game.View.Core
{
    public class ViewCoreInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<ProtocolDispatcher>().AsSingle().CopyIntoAllSubContainers();
        }
    }
}
