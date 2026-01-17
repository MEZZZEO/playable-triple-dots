using System.Collections.Generic;

namespace TripleDots
{
    /// <summary>
    /// Результат генерации уровня
    /// </summary>
    public struct GeneratedLevel
    {
        /// <summary>
        /// Стопки на игровом поле (координата -> стопка)
        /// </summary>
        public Dictionary<HexCoord, HexStackData> GridStacks;

        /// <summary>
        /// Стопки для игрока (3 штуки внизу экрана)
        /// </summary>
        public List<HexStackData> PlayerStacks;

        /// <summary>
        /// Рекомендуемая координата для первого хода (для туториала)
        /// </summary>
        public HexCoord SuggestedFirstMoveTarget;

        /// <summary>
        /// Индекс рекомендуемой стопки игрока для первого хода (0-2)
        /// </summary>
        public int SuggestedFirstMoveStackIndex;

        /// <summary>
        /// Сид генератора (для воспроизводимости)
        /// </summary>
        public int Seed;

        /// <summary>
        /// Проверена ли проходимость уровня
        /// </summary>
        public bool IsValidated;
    }
}