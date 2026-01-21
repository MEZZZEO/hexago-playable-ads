using Cysharp.Threading.Tasks;
using Game.Gameplay.Services;
using Game.Utilities.Lifetimes;
using JetBrains.Lifetimes;

namespace Game.Gameplay
{
    public class HexaGoBootstrap : ILifetimeInitializable
    {
        private readonly IGridService _gridService;
        private readonly ILevelGenerator _levelGenerator;
        private readonly IPlayerStacksService _playerStacksService;
        private readonly IGameStateService _gameStateService;
        private readonly ITutorialService _tutorialService;

        public HexaGoBootstrap(
            IGridService gridService,
            ILevelGenerator levelGenerator,
            IPlayerStacksService playerStacksService,
            IGameStateService gameStateService,
            ITutorialService tutorialService)
        {
            _gridService = gridService;
            _levelGenerator = levelGenerator;
            _playerStacksService = playerStacksService;
            _gameStateService = gameStateService;
            _tutorialService = tutorialService;
        }

        public void Initialize(Lifetime lifetime)
        {
            InitializeGame(lifetime).Forget();
        }

        private async UniTask InitializeGame(Lifetime lifetime)
        {
            _gameStateService.SetState(GameState.Loading);

            while (!_gridService.IsInitialized.Value && lifetime.IsAlive)
            {
                await UniTask.Yield();
            }

            if (!lifetime.IsAlive) return;

            await _levelGenerator.GenerateLevel(lifetime);

            if (!lifetime.IsAlive) return;
            
            await _playerStacksService.SpawnNewStacks(lifetime);

            if (!lifetime.IsAlive) return;

            _gameStateService.SetState(GameState.Tutorial);
            _tutorialService.StartTutorial();

            _tutorialService.OnTutorialComplete.Advise(lifetime, _ =>
            {
                _gameStateService.SetState(GameState.Playing);
            });
        }
    }
}
