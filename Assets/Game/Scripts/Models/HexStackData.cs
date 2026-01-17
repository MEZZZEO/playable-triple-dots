using System;
using System.Collections.Generic;

namespace TripleDots
{
    /// <summary>
    /// Стопка шестиугольников.
    /// Хранит список pieces снизу вверх (index 0 = дно стопки).
    /// </summary>
    [Serializable]
    public class HexStackData
    {
        private readonly List<HexPieceData> _pieces = new();

        public IReadOnlyList<HexPieceData> Pieces => _pieces;
        public int Count => _pieces.Count;
        public bool IsEmpty => Count == 0;

        /// <summary>
        /// Цвет верхнего элемента стопки
        /// </summary>
        public HexColor? TopColor => Count > 0 ? _pieces[^1].Color : null;

        /// <summary>
        /// Цвет нижнего элемента стопки
        /// </summary>
        public HexColor? BottomColor => Count > 0 ? _pieces[0].Color : null;

        public HexStackData()
        {
        }

        public HexStackData(IEnumerable<HexPieceData> pieces)
        {
            foreach (var piece in pieces)
            {
                AddPiece(piece);
            }
        }

        /// <summary>
        /// Создать стопку из массива цветов (снизу вверх)
        /// </summary>
        public static HexStackData FromColors(params HexColor[] colors)
        {
            var stack = new HexStackData();
            foreach (var color in colors)
            {
                stack.AddPiece(new HexPieceData(color));
            }
            return stack;
        }

        /// <summary>
        /// Добавить piece на верх стопки
        /// </summary>
        public void AddPiece(HexPieceData piece)
        {
            piece.IndexInStack = _pieces.Count;
            _pieces.Add(piece);
        }

        /// <summary>
        /// Добавить несколько pieces на верх стопки
        /// </summary>
        public void AddPieces(IEnumerable<HexPieceData> pieces)
        {
            foreach (var piece in pieces)
            {
                AddPiece(piece);
            }
        }

        /// <summary>
        /// Получить piece по индексу
        /// </summary>
        public HexPieceData GetPiece(int index)
        {
            if (index < 0 || index >= _pieces.Count)
                return null;
            return _pieces[index];
        }

        /// <summary>
        /// Получить верхний piece
        /// </summary>
        public HexPieceData PeekTop()
        {
            return Count > 0 ? _pieces[^1] : null;
        }

        /// <summary>
        /// Удалить и вернуть верхний piece
        /// </summary>
        public HexPieceData PopTop()
        {
            if (Count == 0) return null;
            
            var piece = _pieces[^1];
            _pieces.RemoveAt(_pieces.Count - 1);
            return piece;
        }

        /// <summary>
        /// Удалить верхние pieces указанного цвета и вернуть их
        /// </summary>
        public List<HexPieceData> RemoveTopPiecesOfColor(HexColor color)
        {
            var removed = new List<HexPieceData>();
            
            while (Count > 0 && _pieces[^1].Color == color)
            {
                removed.Add(PopTop());
            }
            
            return removed;
        }

        /// <summary>
        /// Посчитать количество pieces указанного цвета сверху стопки
        /// </summary>
        public int CountTopPiecesOfColor(HexColor color)
        {
            int count = 0;
            for (int i = _pieces.Count - 1; i >= 0; i--)
            {
                if (_pieces[i].Color == color)
                    count++;
                else
                    break;
            }
            return count;
        }

        /// <summary>
        /// Посчитать общее количество pieces указанного цвета
        /// </summary>
        public int CountPiecesOfColor(HexColor color)
        {
            int count = 0;
            foreach (var piece in _pieces)
            {
                if (piece.Color == color)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Очистить стопку
        /// </summary>
        public void Clear()
        {
            _pieces.Clear();
        }

        /// <summary>
        /// Обновить индексы всех pieces
        /// </summary>
        public void RefreshIndices()
        {
            for (int i = 0; i < _pieces.Count; i++)
            {
                _pieces[i].IndexInStack = i;
            }
        }

        /// <summary>
        /// Создать глубокую копию стопки
        /// </summary>
        public HexStackData Clone()
        {
            var clone = new HexStackData();
            foreach (var piece in _pieces)
            {
                clone.AddPiece(piece.Clone());
            }
            return clone;
        }

        public override string ToString() => $"Stack[{Count}]: {string.Join(", ", _pieces)}";
    }
}