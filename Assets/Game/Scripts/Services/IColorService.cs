using UnityEngine;

namespace TripleDots
{
    /// <summary>
    /// Тип элемента мира для окрашивания
    /// </summary>
    public enum WorldElementType
    {
        Ground,
        CellBackground,
        CellHighlightValid,
        CellHighlightInvalid
    }

    /// <summary>
    /// Сервис управления цветами через MaterialPropertyBlock.
    /// Обеспечивает единственный материал на всю игру с per-instance цветами.
    /// </summary>
    public interface IColorService
    {
        void Initialize();
        void SetHexColor(Renderer renderer, HexColor color);
        void SetColor(Renderer renderer, Color color);
        void SetWorldElementColor(Renderer renderer, WorldElementType elementType);
        Color GetHexColor(HexColor color);
        Color GetWorldElementColor(WorldElementType elementType);
    }
}