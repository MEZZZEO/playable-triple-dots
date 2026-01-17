using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TripleDots
{
    /// <summary>
    /// Реализация сервиса сетки.
    /// Использует HexMetrics для расчётов позиций.
    /// </summary>
    public class GridService : IGridService
    {
        private readonly GameConfig _gameConfig;
        
        private GridData _gridData;
        private Vector3 _gridOffset;

        public GridData GridData => _gridData;

        public GridService(GameConfig gameConfig)
        {
            _gameConfig = gameConfig;
        }

        public void Initialize()
        {
            _gridData = new GridData(_gameConfig.GridWidth, _gameConfig.GridHeight);
            
            // Вычисляем смещение чтобы центрировать сетку
            CalculateGridOffset();
        }

        public Vector3 CoordToWorldPosition(HexCoord coord)
        {
            var localPos = HexMetrics.Center(
                _gameConfig.HexSize,
                coord.Col,
                coord.Row,
                _gameConfig.Orientation);
            
            return localPos + _gridOffset;
        }

        public bool TryWorldPositionToCoord(Vector3 worldPos, out HexCoord coord)
        {
            // Убираем смещение сетки
            var localPos = worldPos - _gridOffset;
            
            // Конвертируем в координаты hex
            coord = WorldToHex(localPos);
            
            // Проверяем что координаты в пределах сетки
            return _gridData.IsInBounds(coord);
        }

        public IReadOnlyList<HexCellData> GetNeighbors(HexCoord coord)
        {
            var neighbors = new List<HexCellData>();
            
            foreach (var cell in _gridData.GetNeighbors(coord))
            {
                neighbors.Add(cell);
            }
            
            return neighbors;
        }

        public bool IsValidDropPosition(HexCoord coord)
        {
            return _gridData.CanPlaceStack(coord);
        }

        public bool TryGetNearestValidCell(Vector3 worldPos, float maxDistance, out HexCoord coord)
        {
            coord = HexCoord.Invalid;
            float minDistance = float.MaxValue;

            foreach (var cell in _gridData.GetEmptyCells())
            {
                var cellWorldPos = CoordToWorldPosition(cell.Coord);
                var distance = Vector3.Distance(
                    new Vector3(worldPos.x, 0, worldPos.z),
                    new Vector3(cellWorldPos.x, 0, cellWorldPos.z));

                if (distance < maxDistance && distance < minDistance)
                {
                    minDistance = distance;
                    coord = cell.Coord;
                }
            }

            return coord.IsValid;
        }

        public void LoadLevel(GeneratedLevel level)
        {
            // Очищаем текущие данные
            _gridData.Clear();

            // Загружаем стопки на поле
            foreach (var kvp in level.GridStacks)
            {
                var cell = _gridData.GetCell(kvp.Key);
                if (cell != null)
                {
                    cell.SetStack(kvp.Value);
                }
            }
        }

        public bool PlaceStack(HexCoord coord, HexStackData stack)
        {
            if (!IsValidDropPosition(coord))
                return false;

            var cell = _gridData.GetCell(coord);
            if (cell == null)
                return false;

            cell.SetStack(stack);
            return true;
        }

        public Vector3 GetGridCenter()
        {
            // Центр сетки
            float centerX = (_gameConfig.GridWidth - 1) / 2f;
            float centerZ = (_gameConfig.GridHeight - 1) / 2f;
            
            var centerCoord = new HexCoord(
                Mathf.RoundToInt(centerX),
                Mathf.RoundToInt(centerZ));
            
            return CoordToWorldPosition(centerCoord);
        }

        private void CalculateGridOffset()
        {
            // Вычисляем центр сетки в локальных координатах
            var minPos = HexMetrics.Center(_gameConfig.HexSize, 0, 0, _gameConfig.Orientation);
            var maxPos = HexMetrics.Center(
                _gameConfig.HexSize,
                _gameConfig.GridWidth - 1,
                _gameConfig.GridHeight - 1,
                _gameConfig.Orientation);

            var center = (minPos + maxPos) / 2f;
            
            // Смещение чтобы центр сетки был в (0, 0, 0)
            _gridOffset = -center;
        }

        private HexCoord WorldToHex(Vector3 localPos)
        {
            if (_gameConfig.Orientation == HexOrientation.FlatTop)
            {
                return WorldToHexFlatTop(localPos);
            }
            else
            {
                return WorldToHexPointyTop(localPos);
            }
        }

        private HexCoord WorldToHexFlatTop(Vector3 localPos)
        {
            float size = _gameConfig.HexSize;
            
            // Конвертация в axial координаты для FlatTop
            float q = (2f / 3f * localPos.x) / size;
            float r = (-1f / 3f * localPos.x + Mathf.Sqrt(3f) / 3f * localPos.z) / size;
            
            return AxialToOffset(q, r);
        }

        private HexCoord WorldToHexPointyTop(Vector3 localPos)
        {
            float size = _gameConfig.HexSize;
            
            // Конвертация в axial координаты для PointyTop
            float q = (Mathf.Sqrt(3f) / 3f * localPos.x - 1f / 3f * localPos.z) / size;
            float r = (2f / 3f * localPos.z) / size;
            
            return AxialToOffset(q, r);
        }

        private HexCoord AxialToOffset(float q, float r)
        {
            // Округляем axial координаты до ближайшего hex
            var (roundedQ, roundedR) = AxialRound(q, r);
            
            // Конвертируем в offset координаты
            int col = roundedQ;
            int row = roundedR + (roundedQ - (roundedQ & 1)) / 2;
            
            return new HexCoord(col, row);
        }

        private (int q, int r) AxialRound(float q, float r)
        {
            float s = -q - r;
            
            int roundQ = Mathf.RoundToInt(q);
            int roundR = Mathf.RoundToInt(r);
            int roundS = Mathf.RoundToInt(s);

            float qDiff = Mathf.Abs(roundQ - q);
            float rDiff = Mathf.Abs(roundR - r);
            float sDiff = Mathf.Abs(roundS - s);

            if (qDiff > rDiff && qDiff > sDiff)
            {
                roundQ = -roundR - roundS;
            }
            else if (rDiff > sDiff)
            {
                roundR = -roundQ - roundS;
            }

            return (roundQ, roundR);
        }
    }
}

