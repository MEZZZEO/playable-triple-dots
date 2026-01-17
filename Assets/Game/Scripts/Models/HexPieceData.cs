using System;

namespace TripleDots
{
    /// <summary>
    /// Данные одного шестиугольника (piece).
    /// Используется для хранения логики, отделённой от визуализации.
    /// </summary>
    [Serializable]
    public class HexPieceData
    {
        public HexColor Color { get; }
        public int IndexInStack { get; set; }

        public HexPieceData(HexColor color, int indexInStack = 0)
        {
            Color = color;
            IndexInStack = indexInStack;
        }

        public HexPieceData Clone()
        {
            return new HexPieceData(Color, IndexInStack);
        }

        public override string ToString() => $"Piece({Color}, idx:{IndexInStack})";
    }
}