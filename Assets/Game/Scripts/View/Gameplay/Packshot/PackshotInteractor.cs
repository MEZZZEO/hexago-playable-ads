using Game.Gameplay.Services;
using Game.Scripts.View.Core;
using JetBrains.Lifetimes;

namespace Game.View.Gameplay.Packshot
{
    public class HexaPackshotInteractor : IInteractor<HexaPackshotProtocol>
    {
        private readonly IGameStateService _gameStateService;

        public HexaPackshotInteractor(IGameStateService gameStateService)
        {
            _gameStateService = gameStateService;
        }

        public HexaPackshotProtocol Get(Lifetime lifetime)
        {
            var protocol = new HexaPackshotProtocol();

            _gameStateService.CurrentState.Advise(lifetime, state =>
            {
                protocol.IsVisible.Value = state == GameState.Packshot;
            });

            protocol.PlayButtonCommand.Execute.Advise(lifetime, _ =>
            {
                OnPlayClicked();
            });

            protocol.AnyClickCommand.Execute.Advise(lifetime, _ =>
            {
                OnAnyClick();
            });

            return protocol;
        }

        private static void OnPlayClicked()
        {
#if LUNA_PLAYABLE
            Luna.Unity.LifeCycle.GameEnded();
            Luna.Unity.Playable.InstallFullGame();
#endif
        }

        private static void OnAnyClick()
        {
#if LUNA_PLAYABLE
            Luna.Unity.Playable.InstallFullGame();
#endif
        }
    }
}