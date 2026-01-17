﻿using VContainer.Unity;

namespace TripleDots
{
    public class GameBootstrapper : IInitializable
    {
        private readonly IPoolService _poolService;
        private readonly IColorService _colorService;
        private readonly IHexViewPool _hexViewPool;
        private readonly IGridService _gridService;
        private readonly ILevelGeneratorService _levelGenerator;
        private readonly IChainReactionService _chainReactionService;
        private readonly GridView _gridView;
        private readonly PlayerStacksView _playerStacksView;
        //private readonly TutorialController _tutorialController;
        private readonly GameConfig _gameConfig;
        private readonly PrefabRefs _prefabRefs;

        public GameBootstrapper(
            IPoolService poolService,
            IColorService colorService,
            IHexViewPool hexViewPool,
            IGridService gridService,
            ILevelGeneratorService levelGenerator,
            IChainReactionService chainReactionService,
            GridView gridView,
            PlayerStacksView playerStacksView,
            //TutorialController tutorialController,
            GameConfig gameConfig,
            PrefabRefs prefabRefs)
        {
            _poolService = poolService;
            _colorService = colorService;
            _hexViewPool = hexViewPool;
            _gridService = gridService;
            _levelGenerator = levelGenerator;
            _chainReactionService = chainReactionService;
            _gridView = gridView;
            _playerStacksView = playerStacksView;
            //_tutorialController = tutorialController;
            _gameConfig = gameConfig;
            _prefabRefs = prefabRefs;
        }

        public void Initialize()
        {
            // 1. Инициализируем пул (создаёт [Pool] контейнер на сцене)
            _poolService.Initialize();

            // 2. Инициализируем сервис цветов
            _colorService.Initialize();

            // 3. Инициализируем пул hex'ов
            _hexViewPool.Initialize();

            // 4. Инициализируем сетку
            _gridService.Initialize();

            // 5. Генерируем уровень
            var level = _levelGenerator.Generate();

            // 6. Инициализируем Views
            _gridView.Initialize(
                _gridService,
                _colorService,
                _hexViewPool,
                _chainReactionService,
                _gameConfig,
                _prefabRefs);

            _playerStacksView.Initialize(
                _hexViewPool,
                _colorService,
                _gameConfig);

            // 7. Загружаем уровень
            _gridService.LoadLevel(level); // ← Загружаем данные в GridData
            _gridView.LoadLevel(level);     // ← Загружаем визуальное представление
            _playerStacksView.LoadPlayerStacks(level.PlayerStacks);
            // 8. Настраиваем туториал
            //_tutorialController.SetSuggestedMove(
            //    level.SuggestedFirstMoveStackIndex,
            //    level.SuggestedFirstMoveTarget);
        }
    }
}

