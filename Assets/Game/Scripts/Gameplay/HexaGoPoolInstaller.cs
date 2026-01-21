using Game.Gameplay.Configs;
using Game.Utilities.Addressables;
using Zenject;

namespace Game.Gameplay
{
    public class HexaGoPoolInstaller : Installer<HexaGoPoolInstaller>
    {
        public override void InstallBindings()
        {
            // Пул уже зарегистрирован глобально через PooledAddressablesInstaller
            // Здесь можно добавить дополнительную настройку при необходимости
        }
    }
}

