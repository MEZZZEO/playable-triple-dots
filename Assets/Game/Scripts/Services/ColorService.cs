using UnityEngine;

namespace TripleDots
{
    /// <summary>
    /// Реализация сервиса цветов с использованием MaterialPropertyBlock.
    /// Позволяет использовать единственный shared материал для всех объектов.
    /// </summary>
    public class ColorService : IColorService
    {
        private readonly VisualConfig _visualConfig;
        private readonly MaterialPropertyBlock _propertyBlock;
        
        // Кэшируем ID свойства для производительности
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

        public ColorService(VisualConfig visualConfig)
        {
            _visualConfig = visualConfig;
            _propertyBlock = new MaterialPropertyBlock();
        }

        public void Initialize()
        {
            // Пока не требуется дополнительная инициализация
            // В будущем здесь можно добавить прогрев MaterialPropertyBlock
        }

        public void SetHexColor(Renderer renderer, HexColor color)
        {
            if (renderer == null) return;
            
            Color unityColor = _visualConfig.GetHexColor(color);
            ApplyColor(renderer, unityColor);
        }

        public void SetColor(Renderer renderer, Color color)
        {
            if (renderer == null) return;
            
            ApplyColor(renderer, color);
        }

        public void SetWorldElementColor(Renderer renderer, WorldElementType elementType)
        {
            if (renderer == null) return;
            
            Color color = GetWorldElementColor(elementType);
            ApplyColor(renderer, color);
        }

        public Color GetHexColor(HexColor color)
        {
            return _visualConfig.GetHexColor(color);
        }

        public Color GetWorldElementColor(WorldElementType elementType)
        {
            return elementType switch
            {
                WorldElementType.Ground => _visualConfig.GroundColor,
                WorldElementType.CellBackground => _visualConfig.CellBackgroundColor,
                WorldElementType.CellHighlightValid => _visualConfig.CellHighlightValidColor,
                WorldElementType.CellHighlightInvalid => _visualConfig.CellHighlightInvalidColor,
                _ => Color.white
            };
        }

        private void ApplyColor(Renderer renderer, Color color)
        {
            // Получаем текущий PropertyBlock (если есть)
            renderer.GetPropertyBlock(_propertyBlock);
            
            // Устанавливаем цвет
            _propertyBlock.SetColor(ColorPropertyId, color);
            
            // Применяем PropertyBlock к renderer
            renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}

