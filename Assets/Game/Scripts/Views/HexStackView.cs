using System.Collections.Generic;
using UnityEngine;

namespace TripleDots
{
    public class HexStackView : MonoBehaviour
    {
        private readonly List<HexPieceView> _hexViews = new();
        private IHexViewPool _pool;
        private IColorService _colorService;
        private float _hexStackHeight;

        public IReadOnlyList<HexPieceView> HexViews => _hexViews;
        public int Count => _hexViews.Count;

        public void Initialize(IHexViewPool pool, IColorService colorService, float hexStackHeight)
        {
            _pool = pool;
            _colorService = colorService;
            _hexStackHeight = hexStackHeight;
        }

        public void BuildFromData(HexStackData data)
        {
            Clear();
            if (data == null) return;

            foreach (var pieceData in data.Pieces)
            {
                AddHex(pieceData.Color);
            }
        }

        public HexPieceView AddHex(HexColor color)
        {
            var hexView = _pool.Get();
            hexView.SetColor(color);
            hexView.SetParent(transform);
            hexView.SetLocalPosition(new Vector3(0f, _hexViews.Count * _hexStackHeight, 0f));
            _hexViews.Add(hexView);
            return hexView;
        }

        public HexPieceView RemoveTopHex()
        {
            if (_hexViews.Count == 0) return null;
            var topHex = _hexViews[^1];
            _hexViews.RemoveAt(_hexViews.Count - 1);
            topHex.SetParent(null);
            return topHex;
        }

        public List<HexPieceView> RemoveTopHexesOfColor(HexColor color)
        {
            var removed = new List<HexPieceView>();
            while (_hexViews.Count > 0 && _hexViews[^1].CurrentColor == color)
            {
                removed.Add(RemoveTopHex());
            }
            return removed;
        }

        public void Clear()
        {
            foreach (var hexView in _hexViews)
            {
                _pool?.Return(hexView);
            }
            _hexViews.Clear();
        }

        public void RefreshPositions()
        {
            for (int i = 0; i < _hexViews.Count; i++)
            {
                _hexViews[i].SetLocalPosition(new Vector3(0f, i * _hexStackHeight, 0f));
            }
        }

        public Vector3 GetTopPosition()
        {
            float height = _hexViews.Count * _hexStackHeight;
            return transform.position + new Vector3(0f, height, 0f);
        }

        private void OnDestroy()
        {
            Clear();
        }
    }
}
