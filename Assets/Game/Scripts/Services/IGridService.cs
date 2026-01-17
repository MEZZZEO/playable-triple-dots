using System.Collections.Generic;
using UnityEngine;

namespace TripleDots
{
    /// <summary>
    /// Сервис управления игровой сеткой
    /// </summary>
    public interface IGridService
    {
        GridData GridData { get; }
        void Initialize();
        Vector3 CoordToWorldPosition(HexCoord coord);
        bool TryWorldPositionToCoord(Vector3 worldPos, out HexCoord coord);
        IReadOnlyList<HexCellData> GetNeighbors(HexCoord coord);
        bool IsValidDropPosition(HexCoord coord);
        bool TryGetNearestValidCell(Vector3 worldPos, float maxDistance, out HexCoord coord);
        void LoadLevel(GeneratedLevel level);
        bool PlaceStack(HexCoord coord, HexStackData stack);
        Vector3 GetGridCenter();
    }
}
