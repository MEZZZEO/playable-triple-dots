using System;
using UnityEngine;
using UnityEngine.UI;

namespace TripleDots
{
    public class PackshotView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _playButton;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _logoImage;

        public CanvasGroup CanvasGroup => _canvasGroup;
        public Button PlayButton => _playButton;

        public event Action OnPlayClicked;
        public event Action OnAnyClick;

        public void Initialize(VisualConfig visualConfig)
        {
            if (_backgroundImage != null && visualConfig != null)
            {
                _backgroundImage.color = visualConfig.PackshotBackgroundColor;
            }

            if (_playButton != null)
            {
                _playButton.onClick.AddListener(HandlePlayClick);
                
                if (visualConfig != null)
                {
                    var colors = _playButton.colors;
                    colors.normalColor = visualConfig.ButtonColor;
                    _playButton.colors = colors;
                }
            }

            Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }
        }

        public void Hide()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
            
            gameObject.SetActive(false);
        }

        public void SetAlpha(float alpha)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = alpha;
            }
        }

        private void HandlePlayClick()
        {
            OnPlayClicked?.Invoke();
        }

        private void Update()
        {
            if (!gameObject.activeSelf) return;

            // Любой клик/тап
            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                OnAnyClick?.Invoke();
            }
        }

        private void OnDestroy()
        {
            if (_playButton != null)
            {
                _playButton.onClick.RemoveListener(HandlePlayClick);
            }
        }
    }
}

