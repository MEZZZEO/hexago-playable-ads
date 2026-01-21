using Game.Gameplay.Services;
using Game.View.Gameplay.Packshot;
using Game.View.Gameplay.Tutorial;
using UnityEngine;
using Zenject;

namespace Game.Gameplay
{
    public class HexaGoServicesInstaller : Installer<Transform, Transform, HexaGoServicesInstaller>
    {
        private readonly Transform _gridContainer;
        private readonly Transform _playerStacksContainer;

        public HexaGoServicesInstaller(Transform gridContainer, Transform playerStacksContainer)
        {
            _gridContainer = gridContainer;
            _playerStacksContainer = playerStacksContainer;
        }

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<GridService>().AsSingle().WithArguments(_gridContainer);
            
            Container.BindInterfacesTo<StackFactory>().AsSingle();
            
            Container.BindInterfacesTo<MergeService>().AsSingle();
            Container.BindInterfacesTo<PlayerStacksService>().AsSingle().WithArguments(_playerStacksContainer);
            Container.BindInterfacesTo<DragDropService>().AsSingle();
            Container.BindInterfacesTo<LevelGenerator>().AsSingle();
            Container.BindInterfacesTo<GameStateService>().AsSingle();
            Container.BindInterfacesTo<TutorialService>().AsSingle();
            
            Container.BindInterfacesTo<HexaGoBootstrap>().AsSingle();
            
            Container.BindInterfacesTo<HexaTutorialUIInteractor>().AsSingle();
            Container.BindInterfacesTo<HexaPackshotInteractor>().AsSingle();
        }
    }
}
