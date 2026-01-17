using UnityEngine;

namespace TripleDots
{
    public enum CellHighlightState
    {
        None,
        Valid,
        Invalid
    }

    public class HexCellView : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _backgroundRenderer;

        private IColorService _colorService;
        private HexStackView _stackView;

        public HexCoord Coord { get; private set; }
        public HexStackView StackView => _stackView;
        public bool HasStack => _stackView != null && _stackView.Count > 0;

        public void Initialize(HexCoord coord, IColorService colorService)
        {
            Coord = coord;
            _colorService = colorService;

            if (_backgroundRenderer != null)
            {
                _colorService.SetWorldElementColor(_backgroundRenderer, WorldElementType.CellBackground);
            }
        }

        public void SetStackView(HexStackView stackView)
        {
            _stackView = stackView;
            if (_stackView != null)
            {
                _stackView.transform.SetParent(transform);
                _stackView.transform.localPosition = Vector3.zero;
            }
        }

        public HexStackView TakeStackView()
        {
            var stack = _stackView;
            _stackView = null;
            return stack;
        }

        public void SetHighlight(CellHighlightState state)
        {
            if (_backgroundRenderer == null || _colorService == null) return;

            switch (state)
            {
                case CellHighlightState.Valid:
                    _colorService.SetWorldElementColor(_backgroundRenderer, WorldElementType.CellHighlightValid);
                    break;
                case CellHighlightState.Invalid:
                    _colorService.SetWorldElementColor(_backgroundRenderer, WorldElementType.CellHighlightInvalid);
                    break;
                case CellHighlightState.None:
                default:
                    _colorService.SetWorldElementColor(_backgroundRenderer, WorldElementType.CellBackground);
                    break;
            }
        }

        public void ClearStack()
        {
            if (_stackView != null)
            {
                _stackView.Clear();
                Destroy(_stackView.gameObject);
                _stackView = null;
            }
        }

        private void Awake()
        {
            if (_backgroundRenderer == null)
            {
                _backgroundRenderer = GetComponentInChildren<MeshRenderer>();
            }
        }
    }
}

