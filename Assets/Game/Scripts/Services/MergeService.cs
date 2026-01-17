using System.Collections.Generic;
using System.Linq;

namespace TripleDots
{
    /// <summary>
    /// Новая реализация сервиса слияния для классической match-3 логики.
    /// Основной принцип: находим все возможные слияния для стопки,
    /// проверяя соседей с тем же верхним цветом.
    /// </summary>
    public class MergeService : IMergeService
    {
        /// <summary>
        /// Находит все возможные слияния для целевой ячейки.
        /// Для каждого соседа с тем же верхним цветом создаём операцию.
        /// </summary>
        public MergeResult CalculateMerge(HexCellData targetCell, GridData gridData, int maxStackSize)
        {
            var operations = new List<MergeOperation>();

            if (targetCell == null || targetCell.IsEmpty)
                return MergeResult.Empty;

            if (!targetCell.Stack.TopColor.HasValue)
                return MergeResult.Empty;

            var targetColor = targetCell.Stack.TopColor.Value;
            var targetTopCount = targetCell.Stack.CountTopPiecesOfColor(targetColor);

            // Ищем всех соседей
            var neighbors = gridData.GetNeighbors(targetCell.Coord);
            
            foreach (var neighbor in neighbors)
            {
                // Пропускаем пустые соседей
                if (neighbor.IsEmpty)
                    continue;

                // Пропускаем соседей с другим верхним цветом
                if (neighbor.TopColor != targetColor)
                    continue;

                var neighborTopCount = neighbor.Stack.CountTopPiecesOfColor(targetColor);
                
                if (neighborTopCount <= 0)
                    continue;

                // Классическая логика: переносим из меньшей стопки в большую
                var (sourceCell, destCell) = DetermineDirection(
                    targetCell, neighbor,
                    targetTopCount, neighborTopCount);

                if (sourceCell == null || destCell == null)
                    continue;

                var sourceTopCount = sourceCell.Stack.CountTopPiecesOfColor(targetColor);
                var destTopCount = destCell.Stack.CountTopPiecesOfColor(targetColor);
                
                // ИТОГОВОЕ количество верхних элементов ПОСЛЕ переноса
                var totalTopCount = sourceTopCount + destTopCount;
                var willDisappear = totalTopCount >= maxStackSize;

                // ВАЖНО: ResultingStackSize - это размер ПОСЛЕ всех операций
                // Если исчезнет - останется destCell.Stack.Count минус все верхние элементы цвета
                // Если не исчезнет - будет destCell.Stack.Count плюс перенесённые элементы
                var operation = new MergeOperation
                {
                    SourceCoord = sourceCell.Coord,
                    TargetCoord = destCell.Coord,
                    Color = targetColor,
                    PieceCount = sourceTopCount,
                    WillDisappear = willDisappear,
                    ResultingStackSize = willDisappear 
                        ? destCell.Stack.Count - destTopCount  // Останется только нижняя часть без верхних элементов
                        : destCell.Stack.Count + sourceTopCount // Все элементы вместе
                };

                operations.Add(operation);
            }

            // Сортируем по количеству элементов (меньше элементов = выше приоритет)
            operations = operations.OrderBy(op => op.PieceCount).ToList();

            return MergeResult.FromOperations(operations);
        }

        /// <summary>
        /// Определяет направление переноса: из меньшей стопки в большую.
        /// </summary>
        private (HexCellData source, HexCellData dest) DetermineDirection(
            HexCellData cell1, HexCellData cell2,
            int count1, int count2)
        {
            // Если counts равны, переносим из cell2 в cell1 (тот, что был первым при проверке)
            if (count1 >= count2)
                return (cell2, cell1);
            else
                return (cell1, cell2);
        }

        public bool CanMerge(HexStackData source, HexStackData target)
        {
            if (source == null || target == null) return false;
            if (source.IsEmpty || target.IsEmpty) return false;

            return source.TopColor == target.TopColor;
        }
    }
}
