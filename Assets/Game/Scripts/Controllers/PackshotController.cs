using PrimeTween;

namespace TripleDots
{
    public class PackshotController
    {
        private readonly GameFlowController _gameFlowController;
        private readonly PackshotView _packshotView;
        private readonly VisualConfig _visualConfig;

        public PackshotController(
            GameFlowController gameFlowController,
            PackshotView packshotView,
            VisualConfig visualConfig)
        {
            _gameFlowController = gameFlowController;
            _packshotView = packshotView;
            _visualConfig = visualConfig;

            _gameFlowController.OnStateChanged += OnGameStateChanged;

            if (_packshotView != null)
            {
                _packshotView.Initialize(_visualConfig);
                _packshotView.OnPlayClicked += OnPlayClicked;
                _packshotView.OnAnyClick += OnAnyClick;
            }
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Packshot)
            {
                Show();
            }
        }

        public void Show()
        {
            if (_packshotView == null) return;

            // Анимация появления
            _packshotView.SetAlpha(0f);
            _packshotView.Show();

            Tween.Custom(0f, 1f, 0.5f, (value) =>
            {
                _packshotView.SetAlpha(value);
            });

            // Вызываем Luna API - игра завершена
#if LUNA_PLATFORM
            Luna.Unity.LifeCycle.GameEnded();
#endif
        }

        private void OnPlayClicked()
        {
            InstallGame();
        }

        private void OnAnyClick()
        {
            InstallGame();
        }

        private void InstallGame()
        {
#if LUNA_PLATFORM
            Luna.Unity.Playable.InstallFullGame();
#endif
        }

        public void Dispose()
        {
            _gameFlowController.OnStateChanged -= OnGameStateChanged;
            
            if (_packshotView != null)
            {
                _packshotView.OnPlayClicked -= OnPlayClicked;
                _packshotView.OnAnyClick -= OnAnyClick;
            }
        }
    }
}

