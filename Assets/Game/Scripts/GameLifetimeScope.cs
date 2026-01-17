using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TripleDots
{
    public class GameLifetimeScope : LifetimeScope
    {
        [Header("Configs")]
        [SerializeField] private GameConfig _gameConfig;
        [SerializeField] private VisualConfig _visualConfig;
        [SerializeField] private GenerationConfig _generationConfig;
        [SerializeField] private PrefabRefs _prefabRefs;

        [Header("Scene References")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private GridView _gridView;
        [SerializeField] private PlayerStacksView _playerStacksView;
        [SerializeField] private TutorialHandView _tutorialHandView;
        [SerializeField] private PackshotView _packshotView;

        protected override void Configure(IContainerBuilder builder)
        {
            // Configs
            builder.RegisterInstance(_gameConfig);
            builder.RegisterInstance(_visualConfig);
            builder.RegisterInstance(_generationConfig);
            builder.RegisterInstance(_prefabRefs);

            // Scene references
            builder.RegisterInstance(_mainCamera);

            // Services
            builder.Register<IColorService, ColorService>(Lifetime.Singleton);
            builder.Register<IPoolService, PoolService>(Lifetime.Singleton);
            builder.Register<IHexViewPool, HexViewPool>(Lifetime.Singleton);
            builder.Register<IGridService, GridService>(Lifetime.Singleton);
            builder.Register<ILevelGeneratorService, LevelGeneratorService>(Lifetime.Singleton);
            builder.Register<IMergeService, MergeService>(Lifetime.Singleton);
            builder.Register<IChainReactionService, ChainReactionService>(Lifetime.Singleton);

            // Views
            builder.RegisterComponent(_gridView);
            builder.RegisterComponent(_playerStacksView);
            //builder.RegisterComponent(_tutorialHandView);
            //builder.RegisterComponent(_packshotView);

            // Controllers
            builder.Register<GameFlowController>(Lifetime.Singleton)
                .As<GameFlowController>()
                .As<IStartable>()
                .As<ITickable>();
            builder.Register<DragDropController>(Lifetime.Singleton).As<ITickable>();
            //builder.Register<TutorialController>(Lifetime.Singleton);
            //builder.Register<PackshotController>(Lifetime.Singleton);

            // Entry point
            builder.RegisterEntryPoint<GameBootstrapper>();
        }
    }
}