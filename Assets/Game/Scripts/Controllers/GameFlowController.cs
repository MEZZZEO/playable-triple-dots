using System;
using System.Linq;
using VContainer.Unity;

namespace TripleDots
{
    public class GameFlowController : IStartable, ITickable
    {
        private readonly IGridService _gridService;
        private readonly ILevelGeneratorService _levelGenerator;
        private readonly IChainReactionService _chainReactionService;
        private readonly GridView _gridView;
        private readonly PlayerStacksView _playerStacksView;
        private readonly GameConfig _gameConfig;

        private GameState _currentState = GameState.Initializing;
        private int _placedStacksCount;
        private float _currentSpeedMultiplier = 1f;

        public GameState CurrentState => _currentState;
        public float SpeedMultiplier => _currentSpeedMultiplier;

        public event Action<GameState> OnStateChanged;
        public event Action OnGameReady;

        public GameFlowController(
            IGridService gridService,
            ILevelGeneratorService levelGenerator,
            IChainReactionService chainReactionService,
            GridView gridView,
            PlayerStacksView playerStacksView,
            GameConfig gameConfig)
        {
            _gridService = gridService;
            _levelGenerator = levelGenerator;
            _chainReactionService = chainReactionService;
            _gridView = gridView;
            _playerStacksView = playerStacksView;
            _gameConfig = gameConfig;
        }

        public void Start()
        {
            // Инициализируем игровое состояние - переходим в Gameplay
            SetState(GameState.Gameplay);
            OnGameReady?.Invoke();
        }

        public void Tick() { }

        public void StartGameplay()
        {
            SetState(GameState.Gameplay);
        }

        public void OnStackPlaced(HexCoord targetCoord)
        {
            _placedStacksCount++;
            _currentSpeedMultiplier = 1f + (_placedStacksCount * _gameConfig.SpeedIncreasePercent);
            SetState(GameState.ChainReaction);
            StartChainReaction(targetCoord);
        }

        private async void StartChainReaction(HexCoord startCoord)
        {
            await _chainReactionService.ExecuteAsync(startCoord);
            OnChainReactionComplete();
        }

        private void OnChainReactionComplete()
        {
            if (_playerStacksView.Count == 0)
            {
                // Проверяем есть ли свободные ячейки для продолжения игры
                var emptyCellsCount = _gridService.GridData.GetEmptyCells().Count();
                
                if (emptyCellsCount > 0)
                {
                    // Есть свободные ячейки - генерируем новые стопки игрока
                    GenerateNewPlayerStacks();
                    SetState(GameState.Gameplay);
                }
                else
                {
                    // Нет свободных ячеек - игра окончена
                    SetState(GameState.Packshot);
                }
            }
            else
            {
                SetState(GameState.Gameplay);
            }
        }

        private void GenerateNewPlayerStacks()
        {
            // Генерируем новые стопки на основе текущего состояния сетки
            var newStacks = _levelGenerator.GenerateNewPlayerStacks(
                _gridService.GridData, 
                _gameConfig.PlayerStackCount);
            
            // Загружаем их в PlayerStacksView
            _playerStacksView.LoadPlayerStacks(newStacks);
        }

        public void ShowPackshot()
        {
            SetState(GameState.Packshot);
        }

        private void SetState(GameState newState)
        {
            if (_currentState == newState) return;
            _currentState = newState;
            OnStateChanged?.Invoke(_currentState);
        }
    }
}
