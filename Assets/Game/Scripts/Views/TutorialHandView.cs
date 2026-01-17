using UnityEngine;

namespace TripleDots
{
    public class TutorialHandView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _handSprite;
        [SerializeField] private Transform _handTransform;

        private bool _isVisible;

        public bool IsVisible => _isVisible;
        public Transform HandTransform => _handTransform != null ? _handTransform : transform;

        public void Show()
        {
            _isVisible = true;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            _isVisible = false;
            gameObject.SetActive(false);
        }

        public void SetPosition(Vector3 worldPosition)
        {
            HandTransform.position = worldPosition;
        }

        public void SetAlpha(float alpha)
        {
            if (_handSprite != null)
            {
                var color = _handSprite.color;
                color.a = alpha;
                _handSprite.color = color;
            }
        }

        private void Awake()
        {
            if (_handTransform == null)
            {
                _handTransform = transform;
            }

            if (_handSprite == null)
            {
                _handSprite = GetComponentInChildren<SpriteRenderer>();
            }
        }
    }
}
