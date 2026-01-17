using UnityEngine;

namespace TripleDots
{
    /// <summary>
    /// View компонент для одного шестиугольника.
    /// Использует MaterialPropertyBlock для per-instance цвета.
    /// </summary>
    public class HexPieceView : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _renderer;
        
        private IColorService _colorService;
        private Material _sharedMaterial;
        private HexColor _currentColor;
        private bool _isInitialized;

        /// <summary>
        /// Текущий цвет hex
        /// </summary>
        public HexColor CurrentColor => _currentColor;

        /// <summary>
        /// Renderer компонент
        /// </summary>
        public MeshRenderer Renderer => _renderer;

        /// <summary>
        /// Инициализация view сервисами
        /// </summary>
        public void Initialize(IColorService colorService, Material sharedMaterial)
        {
            _colorService = colorService;
            _sharedMaterial = sharedMaterial;
            
            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<MeshRenderer>();
            }

            // Устанавливаем shared материал
            if (_renderer != null && _sharedMaterial != null)
            {
                _renderer.sharedMaterial = _sharedMaterial;
            }
            
            _isInitialized = true;
        }

        /// <summary>
        /// Установить цвет hex
        /// </summary>
        public void SetColor(HexColor color)
        {
            _currentColor = color;
            
            if (_isInitialized && _colorService != null)
            {
                _colorService.SetHexColor(_renderer, color);
            }
        }

        /// <summary>
        /// Установить позицию в мировых координатах
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        /// <summary>
        /// Установить локальную позицию
        /// </summary>
        public void SetLocalPosition(Vector3 localPosition)
        {
            transform.localPosition = localPosition;
        }

        /// <summary>
        /// Установить родителя
        /// </summary>
        public void SetParent(Transform parent, bool worldPositionStays = false)
        {
            transform.SetParent(parent, worldPositionStays);
        }

        /// <summary>
        /// Сбросить состояние перед возвратом в пул
        /// </summary>
        public void ResetState()
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            
            // КРИТИЧНО: Сбрасываем цвет чтобы избежать визуальных артефактов
            _currentColor = default;
        }

        private void Awake()
        {
            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<MeshRenderer>();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<MeshRenderer>();
            }
        }
#endif
    }
}

