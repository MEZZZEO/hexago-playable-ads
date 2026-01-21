using Game.Gameplay.Services;
using Game.Scripts.View.Core;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Game.View.Gameplay.Tutorial
{
    public class HexaTutorialUIInteractor : IInteractor<HexaTutorialUIProtocol>
    {
        private readonly ITutorialService _tutorialService;

        public HexaTutorialUIInteractor(ITutorialService tutorialService)
        {
            _tutorialService = tutorialService;
        }

        public HexaTutorialUIProtocol Get(Lifetime lifetime)
        {
            var protocol = new HexaTutorialUIProtocol();

            _tutorialService.IsActive.WhenTrue(lifetime, tutorialLifetime =>
            {
                protocol.IsActive.Value = true;
                
                _tutorialService.CurrentState.Advise(tutorialLifetime, state =>
                {
                    protocol.ShowHand.Value = state == TutorialState.ShowingHand;
                });
                
                _tutorialService.HandTargetPosition.Advise(tutorialLifetime, pos =>
                {
                    protocol.HandStartPosition.Value = pos;
                });

                _tutorialService.HandDestinationPosition.Advise(tutorialLifetime, pos =>
                {
                    protocol.HandEndPosition.Value = pos;
                });
                
                tutorialLifetime.OnTermination(() =>
                {
                    protocol.IsActive.Value = false;
                    protocol.ShowHand.Value = false;
                });
            });

            return protocol;
        }
    }
}