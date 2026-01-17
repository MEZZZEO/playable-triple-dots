using System;
using UnityEngine;

namespace TripleDots
{
    /// <summary>
    /// Координаты шестиугольника в сетке.
    /// Поддерживает Offset (для хранения) и Cube (для расчётов) системы координат.
    /// </summary>
    [Serializable]
    public readonly struct HexCoord : IEquatable<HexCoord>
    {
        // Offset координаты (odd-q для FlatTop, odd-r для PointyTop)
        public readonly int Col;
        public readonly int Row;

        // Направления соседей в Cube координатах
        private static readonly Vector3Int[] CubeDirections =
        {
            new(1, -1, 0),  // Right
            new(1, 0, -1),  // Upper Right
            new(0, 1, -1),  // Upper Left
            new(-1, 1, 0),  // Left
            new(-1, 0, 1),  // Lower Left
            new(0, -1, 1)   // Lower Right
        };

        public HexCoord(int col, int row)
        {
            Col = col;
            Row = row;
        }

        #region Cube Coordinates (для расчётов соседей и расстояний)

        /// <summary>
        /// Cube Q координата (для FlatTop offset odd-q)
        /// </summary>
        public int Q => Col;

        /// <summary>
        /// Cube R координата (для FlatTop offset odd-q)
        /// </summary>
        public int R => Row - (Col - (Col & 1)) / 2;

        /// <summary>
        /// Cube S координата (Q + R + S = 0)
        /// </summary>
        public int S => -Q - R;

        /// <summary>
        /// Cube координаты как Vector3Int
        /// </summary>
        public Vector3Int ToCube() => new(Q, R, S);

        /// <summary>
        /// Создать HexCoord из Cube координат (для FlatTop)
        /// </summary>
        public static HexCoord FromCube(int q, int r)
        {
            int col = q;
            int row = r + (q - (q & 1)) / 2;
            return new HexCoord(col, row);
        }

        public static HexCoord FromCube(Vector3Int cube) => FromCube(cube.x, cube.y);

        #endregion

        #region Neighbors

        /// <summary>
        /// Получить координаты всех 6 соседей
        /// </summary>
        public HexCoord[] GetNeighbors()
        {
            var neighbors = new HexCoord[6];
            var cube = ToCube();

            for (int i = 0; i < 6; i++)
            {
                var neighborCube = cube + CubeDirections[i];
                neighbors[i] = FromCube(neighborCube);
            }

            return neighbors;
        }

        /// <summary>
        /// Получить соседа в указанном направлении (0-5)
        /// </summary>
        public HexCoord GetNeighbor(int direction)
        {
            var cube = ToCube();
            var neighborCube = cube + CubeDirections[direction % 6];
            return FromCube(neighborCube);
        }

        #endregion

        #region Distance

        /// <summary>
        /// Расстояние до другого hex в количестве шагов
        /// </summary>
        public int DistanceTo(HexCoord other)
        {
            var a = ToCube();
            var b = other.ToCube();
            return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z)) / 2;
        }

        #endregion

        #region Equality

        public bool Equals(HexCoord other) => Col == other.Col && Row == other.Row;

        public override bool Equals(object obj) => obj is HexCoord other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Col, Row);

        public static bool operator ==(HexCoord left, HexCoord right) => left.Equals(right);

        public static bool operator !=(HexCoord left, HexCoord right) => !left.Equals(right);

        #endregion

        public override string ToString() => $"HexCoord({Col}, {Row})";

        /// <summary>
        /// Невалидная координата для обозначения "нет координаты"
        /// </summary>
        public static HexCoord Invalid => new(-1, -1);

        public bool IsValid => Col >= 0 && Row >= 0;
    }
}