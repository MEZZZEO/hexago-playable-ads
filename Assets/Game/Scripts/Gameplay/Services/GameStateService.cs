using Cysharp.Threading.Tasks;
using Game.Utilities.Lifetimes;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using JetBrains.Lifetimes;

namespace Game.Gameplay.Services
{
    public enum GameState
    {
        Loading,
        Tutorial,
        Playing,
        Merging,
        Packshot
    }

    public interface IGameStateService
    {
        IReadonlyProperty<GameState> CurrentState { get; }
        IReadonlyProperty<int> PlacedStacksCount { get; }
        ISource<Unit> OnGameComplete { get; }

        void SetState(GameState state);
        void OnStackPlaced();
        void CompleteGame();
    }
    
    public class GameStateService : IGameStateService, ILifetimeInitializable
    {
        private readonly IMergeService _mergeService;
        private readonly IPlayerStacksService _playerStacksService;
        private readonly IGridService _gridService;
        private readonly Configs.GameplayConfig _gameplayConfig;

        private readonly ViewableProperty<GameState> _currentState = new(GameState.Loading);
        private readonly ViewableProperty<int> _placedStacksCount = new(0);
        private readonly Signal<Unit> _onGameComplete = new();

        public IReadonlyProperty<GameState> CurrentState => _currentState;
        public IReadonlyProperty<int> PlacedStacksCount => _placedStacksCount;
        public ISource<Unit> OnGameComplete => _onGameComplete;

        public GameStateService(
            IMergeService mergeService,
            IPlayerStacksService playerStacksService,
            IGridService gridService,
            Configs.GameplayConfig gameplayConfig)
        {
            _mergeService = mergeService;
            _playerStacksService = playerStacksService;
            _gridService = gridService;
            _gameplayConfig = gameplayConfig;
        }

        public void Initialize(Lifetime lifetime)
        {
            _mergeService.IsMerging.Advise(lifetime, isMerging =>
            {
                if (_currentState.Value is GameState.Playing or GameState.Merging)
                {
                    _currentState.Value = isMerging ? GameState.Merging : GameState.Playing;
                }
            });

            var previous = _currentState.Value;
            _currentState.Advise(lifetime, state =>
            {
                if (previous == GameState.Tutorial && state == GameState.Playing)
                {
                    StartPackshotDelayTimer(lifetime).Forget();
                }

                previous = state;
            });

            _playerStacksService.OnAllStacksPlaced.Advise(lifetime, _ =>
                {
                    RegeneratePlayerStacks(lifetime).Forget();
                });
        }

        public void SetState(GameState state)
        {
            _currentState.Value = state;
        }

        public void OnStackPlaced()
        {
            _placedStacksCount.Value++;
        }

        private async UniTask RegeneratePlayerStacks(Lifetime lifetime)
        {
            while (_mergeService.IsMerging.Value && lifetime.IsAlive)
            {
                await UniTask.Yield();
            }

            if (!lifetime.IsAlive) return;

            var occupiedCells = _gridService.GetOccupiedCells();
            if (occupiedCells.Count == 0)
            {
                CompleteGame();
                return;
            }

            await _playerStacksService.SpawnNewStacks(lifetime);
        }

        private async UniTask StartPackshotDelayTimer(Lifetime lifetime)
        {
            var delaySec = _gameplayConfig.PackshotDelayAfterTutorialSec;
            await UniTask.Delay(System.TimeSpan.FromSeconds(delaySec), cancellationToken: lifetime);
            if (!lifetime.IsAlive) return;

            if (_currentState.Value != GameState.Packshot)
            {
                CompleteGame();
            }
        }

        public void CompleteGame()
        {
            _currentState.Value = GameState.Packshot;
            _onGameComplete.Fire(Unit.Instance);
        }
    }
}
