namespace TripleDots
{
    /// <summary>
    /// Состояния игрового процесса
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// Инициализация (загрузка, генерация уровня)
        /// </summary>
        Initializing = 0,

        /// <summary>
        /// Туториал (показ подсказки)
        /// </summary>
        Tutorial = 1,

        /// <summary>
        /// Игровой процесс (игрок может перетаскивать стопки)
        /// </summary>
        Gameplay = 2,

        /// <summary>
        /// Выполняется цепная реакция (игрок не может взаимодействовать)
        /// </summary>
        ChainReaction = 3,

        /// <summary>
        /// Показ финального экрана
        /// </summary>
        Packshot = 4
    }
}