using UnityEngine;

namespace TripleDots
{
    /// <summary>
    /// Конфиг визуальных настроек - цвета hex, элементов мира и UI
    /// </summary>
    [CreateAssetMenu(fileName = "VisualConfig", menuName = "TripleDots/VisualConfig")]
    public class VisualConfig : ScriptableObject
    {
        [Header("Hex Colors")]
        [Tooltip("Цвета шестиугольников (порядок соответствует HexColor enum)")]
        public Color[] HexColors = new Color[5]
        {
            new(0.9f, 0.3f, 0.3f, 1f),  // Red
            new(0.3f, 0.5f, 0.9f, 1f),  // Blue
            new(0.3f, 0.8f, 0.4f, 1f),  // Green
            new(0.95f, 0.85f, 0.3f, 1f), // Yellow
            new(0.7f, 0.4f, 0.9f, 1f)   // Purple
        };

        [Header("World Colors")]
        [Tooltip("Цвет пола/фона")]
        public Color GroundColor = new(0.15f, 0.15f, 0.2f, 1f);
        
        [Tooltip("Цвет фона ячейки сетки")]
        public Color CellBackgroundColor = new(0.25f, 0.25f, 0.3f, 1f);
        
        [Tooltip("Цвет подсветки валидной ячейки для drop")]
        public Color CellHighlightValidColor = new(0.4f, 0.8f, 0.4f, 0.5f);
        
        [Tooltip("Цвет подсветки невалидной ячейки для drop")]
        public Color CellHighlightInvalidColor = new(0.8f, 0.3f, 0.3f, 0.5f);

        [Header("UI Colors")]
        [Tooltip("Цвет фона packshot")]
        public Color PackshotBackgroundColor = new(0.1f, 0.1f, 0.15f, 0.95f);
        
        [Tooltip("Цвет кнопки Play")]
        public Color ButtonColor = new(0.2f, 0.7f, 0.3f, 1f);
        
        [Tooltip("Цвет текста кнопки")]
        public Color ButtonTextColor = Color.white;

        [Header("Effects")]
        [Tooltip("Цвет подсветки при слиянии")]
        public Color MergeHighlightColor = new(1f, 1f, 0.5f, 0.8f);

        /// <summary>
        /// Получить цвет по enum HexColor
        /// </summary>
        public Color GetHexColor(HexColor hexColor)
        {
            int index = (int)hexColor;
            if (index >= 0 && index < HexColors.Length)
            {
                return HexColors[index];
            }
            return Color.white;
        }

        /// <summary>
        /// Проверить валидность конфига
        /// </summary>
        private void OnValidate()
        {
            if (HexColors == null || HexColors.Length != 5)
            {
                HexColors = new Color[5];
            }
        }
    }
}