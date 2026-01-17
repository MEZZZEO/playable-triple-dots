using System;
using System.Collections.Generic;
using System.Linq;

namespace TripleDots
{
    /// <summary>
    /// Данные всей игровой сетки.
    /// Хранит все ячейки и предоставляет методы для работы с ними.
    /// </summary>
    [Serializable]
    public class GridData
    {
        private readonly Dictionary<HexCoord, HexCellData> _cells = new();

        public int Width { get; }
        public int Height { get; }
        public int CellCount => _cells.Count;

        public GridData(int width, int height)
        {
            Width = width;
            Height = height;
            InitializeCells();
        }

        private void InitializeCells()
        {
            for (int row = 0; row < Height; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    var coord = new HexCoord(col, row);
                    _cells[coord] = new HexCellData(coord);
                }
            }
        }

        #region Cell Access

        /// <summary>
        /// Получить ячейку по координатам
        /// </summary>
        public HexCellData GetCell(HexCoord coord)
        {
            return _cells.GetValueOrDefault(coord);
        }

        /// <summary>
        /// Попытаться получить ячейку по координатам
        /// </summary>
        public bool TryGetCell(HexCoord coord, out HexCellData cell)
        {
            return _cells.TryGetValue(coord, out cell);
        }

        /// <summary>
        /// Проверить, существует ли ячейка с указанными координатами
        /// </summary>
        public bool HasCell(HexCoord coord)
        {
            return _cells.ContainsKey(coord);
        }

        /// <summary>
        /// Проверить, находятся ли координаты в пределах сетки
        /// </summary>
        public bool IsInBounds(HexCoord coord)
        {
            return coord.Col >= 0 && coord.Col < Width &&
                   coord.Row >= 0 && coord.Row < Height;
        }

        #endregion

        #region Cell Queries

        /// <summary>
        /// Получить все ячейки
        /// </summary>
        public IEnumerable<HexCellData> GetAllCells()
        {
            return _cells.Values;
        }

        /// <summary>
        /// Получить все непустые ячейки
        /// </summary>
        public IEnumerable<HexCellData> GetOccupiedCells()
        {
            return _cells.Values.Where(c => !c.IsEmpty);
        }

        /// <summary>
        /// Получить все пустые ячейки (доступные для размещения)
        /// </summary>
        public IEnumerable<HexCellData> GetEmptyCells()
        {
            return _cells.Values.Where(c => c.CanPlaceStack);
        }

        /// <summary>
        /// Получить соседние ячейки
        /// </summary>
        public IEnumerable<HexCellData> GetNeighbors(HexCoord coord)
        {
            var neighborCoords = coord.GetNeighbors();
            foreach (var neighborCoord in neighborCoords)
            {
                if (_cells.TryGetValue(neighborCoord, out var cell))
                {
                    yield return cell;
                }
            }
        }

        /// <summary>
        /// Получить соседние ячейки с непустыми стопками
        /// </summary>
        public IEnumerable<HexCellData> GetOccupiedNeighbors(HexCoord coord)
        {
            return GetNeighbors(coord).Where(c => !c.IsEmpty);
        }

        /// <summary>
        /// Получить соседние ячейки с указанным цветом верхнего элемента
        /// </summary>
        public IEnumerable<HexCellData> GetNeighborsWithTopColor(HexCoord coord, HexColor color)
        {
            return GetNeighbors(coord).Where(c => c.TopColor == color);
        }

        /// <summary>
        /// Получить все ячейки с указанным цветом верхнего элемента
        /// </summary>
        public IEnumerable<HexCellData> GetCellsWithTopColor(HexColor color)
        {
            return _cells.Values.Where(c => c.TopColor == color);
        }

        #endregion

        #region Validation

        /// <summary>
        /// Можно ли разместить стопку в указанную ячейку
        /// </summary>
        public bool CanPlaceStack(HexCoord coord)
        {
            return TryGetCell(coord, out var cell) && cell.CanPlaceStack;
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Подсчитать общее количество pieces на сетке
        /// </summary>
        public int GetTotalPieceCount()
        {
            return _cells.Values.Sum(c => c.Stack?.Count ?? 0);
        }

        /// <summary>
        /// Подсчитать количество pieces указанного цвета на сетке
        /// </summary>
        public int GetPieceCountOfColor(HexColor color)
        {
            return _cells.Values.Sum(c => c.Stack?.CountPiecesOfColor(color) ?? 0);
        }

        /// <summary>
        /// Получить статистику по цветам
        /// </summary>
        public Dictionary<HexColor, int> GetColorStatistics()
        {
            var stats = new Dictionary<HexColor, int>();
            foreach (HexColor color in Enum.GetValues(typeof(HexColor)))
            {
                stats[color] = GetPieceCountOfColor(color);
            }
            return stats;
        }

        #endregion

        /// <summary>
        /// Очистить все ячейки
        /// </summary>
        public void Clear()
        {
            foreach (var cell in _cells.Values)
            {
                cell.Clear();
            }
        }

        public override string ToString() => $"Grid[{Width}x{Height}], {GetTotalPieceCount()} pieces";
    }
}