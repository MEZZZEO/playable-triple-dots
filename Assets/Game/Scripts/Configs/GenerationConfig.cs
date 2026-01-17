using UnityEngine;

namespace TripleDots
{
    /// <summary>
    /// Конфиг "умной" генерации уровня
    /// </summary>
    [CreateAssetMenu(fileName = "GenerationConfig", menuName = "TripleDots/GenerationConfig")]
    public class GenerationConfig : ScriptableObject
    {
        [Header("Grid Fill")]
        [Tooltip("Процент заполнения сетки стопками")]
        [Range(0.2f, 0.8f)]
        public float FillPercent = 0.5f;

        [Header("Stack Settings")]
        [Tooltip("Минимальный размер стопки на поле")]
        [Range(1, 5)]
        public int MinStackSize = 2;
        
        [Tooltip("Максимальный размер стопки на поле")]
        [Range(3, 8)]
        public int MaxStackSize = 6;

        [Header("Color Distribution")]
        [Tooltip("Количество активных цветов (2-5)")]
        [Range(2, 5)]
        public int ActiveColorsCount = 4;

        [Header("Smart Generation")]
        [Tooltip("Гарантировать возможность победы")]
        public bool EnsureWinnable = true;
        
        [Tooltip("Минимальная длина цепочки слияний")]
        [Range(1, 5)]
        public int MinChainLength = 2;
        
        [Tooltip("Минимальное количество гарантированных слияний")]
        [Range(1, 5)]
        public int GuaranteedMerges = 2;
        
        [Tooltip("Пытаться создать кластеры одного цвета рядом")]
        public bool CreateColorClusters = true;
        
        [Tooltip("Вероятность добавить соседа того же цвета при создании кластера")]
        [Range(0f, 1f)]
        public float ClusterProbability = 0.4f;

        [Header("Player Stacks")]
        [Tooltip("Минимальный размер стопки игрока")]
        [Range(1, 4)]
        public int PlayerStackMinSize = 2;
        
        [Tooltip("Максимальный размер стопки игрока")]
        [Range(2, 6)]
        public int PlayerStackMaxSize = 4;

        [Header("Seed")]
        [Tooltip("Использовать случайный seed")]
        public bool UseRandomSeed = true;
        
        [Tooltip("Фиксированный seed для тестирования (если UseRandomSeed = false)")]
        public int FixedSeed = 12345;

        [Header("Blocked Cells")]
        [Tooltip("Процент заблокированных ячеек (недоступных для размещения)")]
        [Range(0f, 0.3f)]
        public float BlockedCellsPercent = 0f;

        /// <summary>
        /// Получить seed для генерации
        /// </summary>
        public int GetSeed()
        {
            return UseRandomSeed ? System.Environment.TickCount : FixedSeed;
        }

        /// <summary>
        /// Валидация конфига
        /// </summary>
        private void OnValidate()
        {
            if (MinStackSize > MaxStackSize)
            {
                MinStackSize = MaxStackSize;
            }
            
            if (PlayerStackMinSize > PlayerStackMaxSize)
            {
                PlayerStackMinSize = PlayerStackMaxSize;
            }
            
            if (ActiveColorsCount < 2)
            {
                ActiveColorsCount = 2;
            }
        }
    }
}