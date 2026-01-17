using UnityEngine;
using PrimeTween;

namespace TripleDots
{
    public class TutorialController
    {
        private readonly GameFlowController _gameFlowController;
        private readonly DragDropController _dragDropController;
        private readonly PlayerStacksView _playerStacksView;
        private readonly IGridService _gridService;
        private readonly GameConfig _gameConfig;
        private readonly TutorialHandView _tutorialHandView;

        private float _inactivityTimer;
        private bool _isShowingHint;
        private Tween _handTween;
        private HexCoord _suggestedTarget;
        private int _suggestedStackIndex;

        public TutorialController(
            GameFlowController gameFlowController,
            DragDropController dragDropController,
            PlayerStacksView playerStacksView,
            IGridService gridService,
            GameConfig gameConfig,
            TutorialHandView tutorialHandView)
        {
            _gameFlowController = gameFlowController;
            _dragDropController = dragDropController;
            _playerStacksView = playerStacksView;
            _gridService = gridService;
            _gameConfig = gameConfig;
            _tutorialHandView = tutorialHandView;

            _dragDropController.OnDragStarted += OnDragStarted;
            _dragDropController.OnDragEnded += OnDragEnded;
            _gameFlowController.OnStateChanged += OnGameStateChanged;
        }

        public void SetSuggestedMove(int stackIndex, HexCoord targetCoord)
        {
            _suggestedStackIndex = stackIndex;
            _suggestedTarget = targetCoord;
        }

        public void Update(float deltaTime)
        {
            if (_gameFlowController.CurrentState != GameState.Tutorial &&
                _gameFlowController.CurrentState != GameState.Gameplay)
            {
                return;
            }

            if (!_dragDropController.IsDragging && !_isShowingHint)
            {
                _inactivityTimer += deltaTime;

                if (_inactivityTimer >= _gameConfig.InactivityTimeout)
                {
                    ShowHint();
                }
            }
        }

        private void ShowHint()
        {
            if (_tutorialHandView == null) return;
            if (_playerStacksView.Count == 0) return;

            _isShowingHint = true;
            _tutorialHandView.Show();

            // Получаем позиции
            var stackIndex = Mathf.Clamp(_suggestedStackIndex, 0, _playerStacksView.Count - 1);
            var stack = _playerStacksView.GetStackAt(stackIndex);
            if (stack == null) return;

            var startPos = stack.GetTopPosition();
            var endPos = _suggestedTarget.IsValid
                ? _gridService.CoordToWorldPosition(_suggestedTarget)
                : _gridService.GetGridCenter();

            // Анимируем руку
            AnimateHand(startPos, endPos);
        }

        private void HideHint()
        {
            _isShowingHint = false;
            _handTween.Stop();
            _tutorialHandView?.Hide();
        }

        private void AnimateHand(Vector3 from, Vector3 to)
        {
            _tutorialHandView.SetPosition(from);

            _handTween = Tween.Position(
                _tutorialHandView.HandTransform,
                to,
                _gameConfig.TutorialHandDuration,
                Ease.InOutQuad,
                cycles: -1,
                cycleMode: CycleMode.Restart);
        }

        private void OnDragStarted()
        {
            _inactivityTimer = 0;
            HideHint();

            // Переключаемся на gameplay если были в туториале
            if (_gameFlowController.CurrentState == GameState.Tutorial)
            {
                _gameFlowController.StartGameplay();
            }
        }

        private void OnDragEnded()
        {
            _inactivityTimer = 0;
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Packshot || state == GameState.ChainReaction)
            {
                HideHint();
            }
        }

        public void Dispose()
        {
            _dragDropController.OnDragStarted -= OnDragStarted;
            _dragDropController.OnDragEnded -= OnDragEnded;
            _gameFlowController.OnStateChanged -= OnGameStateChanged;
            _handTween.Stop();
        }
    }
}

