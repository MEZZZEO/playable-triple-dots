using System.Collections.Generic;

namespace TripleDots
{
    /// <summary>
    /// Результат цепной реакции
    /// </summary>
    public struct ChainReactionResult
    {
        /// <summary>
        /// Общее количество слияний
        /// </summary>
        public int TotalMerges;

        /// <summary>
        /// Общее количество исчезнувших pieces
        /// </summary>
        public int TotalDisappearedPieces;

        /// <summary>
        /// Количество исчезнувших стопок
        /// </summary>
        public int TotalDisappearedStacks;

        /// <summary>
        /// Все шаги цепной реакции
        /// </summary>
        public List<ChainStep> Steps;

        /// <summary>
        /// Была ли хоть одна реакция
        /// </summary>
        public bool HadReaction => TotalMerges > 0;

        public static ChainReactionResult Empty => new()
        {
            TotalMerges = 0,
            TotalDisappearedPieces = 0,
            TotalDisappearedStacks = 0,
            Steps = new List<ChainStep>()
        };
    }

    /// <summary>
    /// Один шаг цепной реакции (может содержать несколько параллельных операций)
    /// </summary>
    public struct ChainStep
    {
        /// <summary>
        /// Индекс шага (для последовательной анимации)
        /// </summary>
        public int StepIndex;

        /// <summary>
        /// Операции слияния в этом шаге (могут выполняться параллельно)
        /// </summary>
        public List<MergeOperation> Operations;

        /// <summary>
        /// Координаты стопок, которые исчезнут после этого шага
        /// </summary>
        public List<HexCoord> DisappearingStacks;

        public ChainStep(int stepIndex)
        {
            StepIndex = stepIndex;
            Operations = new List<MergeOperation>();
            DisappearingStacks = new List<HexCoord>();
        }

        public override string ToString() => 
            $"Step {StepIndex}: {Operations.Count} operations, {DisappearingStacks.Count} disappearing";
    }
}