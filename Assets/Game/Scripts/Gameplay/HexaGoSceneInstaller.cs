using Game.Gameplay.Configs;
using UnityEngine;
using Zenject;

namespace Game.Gameplay
{
    public class HexaGoSceneInstaller : MonoInstaller
    {
        [Header("Configs")]
        [SerializeField] private GameplayConfig _gameplayConfig;
        [SerializeField] private GridConfig _gridConfig;
        [SerializeField] private HexColorConfig _hexColorConfig;
        
        [Header("Scene References")]
        [SerializeField] private Transform _gridContainer;
        [SerializeField] private Transform _playerStacksContainer;
        [SerializeField] private Camera _mainCamera;

        public override void InstallBindings()
        {
            Container.Bind<GameplayConfig>().FromInstance(_gameplayConfig).AsSingle();
            Container.Bind<GridConfig>().FromInstance(_gridConfig).AsSingle();
            Container.Bind<HexColorConfig>().FromInstance(_hexColorConfig).AsSingle();
            Container.Bind<Camera>().FromInstance(_mainCamera != null ? _mainCamera : Camera.main).AsSingle();
            
            HexaGoServicesInstaller.Install(Container, _gridContainer, _playerStacksContainer);
        }
    }
}
