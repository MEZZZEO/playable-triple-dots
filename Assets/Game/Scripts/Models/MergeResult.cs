using System.Collections.Generic;

namespace TripleDots
{
    /// <summary>
    /// Результат операции слияния стопок
    /// </summary>
    public struct MergeResult
    {
        /// <summary>
        /// Есть ли слияние
        /// </summary>
        public bool HasMerge;

        /// <summary>
        /// Список операций слияния
        /// </summary>
        public List<MergeOperation> Operations;

        public static MergeResult Empty => new()
        {
            HasMerge = false,
            Operations = new List<MergeOperation>()
        };

        public static MergeResult FromOperations(List<MergeOperation> operations)
        {
            return new MergeResult
            {
                HasMerge = operations.Count > 0,
                Operations = operations
            };
        }
    }

    /// <summary>
    /// Одна операция слияния (перемещение pieces с одной ячейки на другую)
    /// </summary>
    public struct MergeOperation
    {
        /// <summary>
        /// Исходная ячейка (откуда перемещаются pieces)
        /// </summary>
        public HexCoord SourceCoord;

        /// <summary>
        /// Целевая ячейка (куда перемещаются pieces)
        /// </summary>
        public HexCoord TargetCoord;

        /// <summary>
        /// Цвет перемещаемых pieces
        /// </summary>
        public HexColor Color;

        /// <summary>
        /// Количество перемещаемых pieces
        /// </summary>
        public int PieceCount;

        /// <summary>
        /// Стопка исчезнет после этой операции (достигла MaxStackSize)
        /// </summary>
        public bool WillDisappear;

        /// <summary>
        /// Количество pieces в целевой стопке после слияния (если не исчезнет)
        /// </summary>
        public int ResultingStackSize;

        public override string ToString() =>
            $"Merge {PieceCount}x{Color} from {SourceCoord} to {TargetCoord}" +
            (WillDisappear ? " [DISAPPEAR]" : "");
    }
}
