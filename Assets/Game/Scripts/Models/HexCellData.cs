using System;

namespace TripleDots
{
    /// <summary>
    /// Данные ячейки сетки.
    /// Содержит координаты и ссылку на стопку (если есть).
    /// </summary>
    [Serializable]
    public class HexCellData
    {
        public HexCoord Coord { get; }
        public HexStackData Stack { get; private set; }
        
        /// <summary>
        /// Заблокированная ячейка (нельзя размещать стопки)
        /// </summary>
        public bool IsBlocked { get; set; }

        /// <summary>
        /// Ячейка пуста (нет стопки или стопка пуста)
        /// </summary>
        public bool IsEmpty => Stack == null || Stack.IsEmpty;

        /// <summary>
        /// Можно ли разместить стопку в эту ячейку
        /// </summary>
        public bool CanPlaceStack => !IsBlocked && IsEmpty;

        /// <summary>
        /// Цвет верхнего элемента стопки (если есть)
        /// </summary>
        public HexColor? TopColor => Stack?.TopColor;

        public HexCellData(HexCoord coord)
        {
            Coord = coord;
            Stack = null;
            IsBlocked = false;
        }

        /// <summary>
        /// Установить стопку в ячейку
        /// </summary>
        public void SetStack(HexStackData stack)
        {
            Stack = stack;
        }

        /// <summary>
        /// Забрать стопку из ячейки (ячейка станет пустой)
        /// </summary>
        public HexStackData TakeStack()
        {
            var stack = Stack;
            Stack = null;
            return stack;
        }

        /// <summary>
        /// Очистить ячейку
        /// </summary>
        public void Clear()
        {
            Stack?.Clear();
            Stack = null;
        }

        public override string ToString() => 
            $"Cell{Coord} [{(IsBlocked ? "BLOCKED" : IsEmpty ? "empty" : $"{Stack.Count} pieces")}]";
    }
}