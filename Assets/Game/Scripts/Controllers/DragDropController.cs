using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace TripleDots
{
    public class DragDropController : ITickable
    {
        private readonly Camera _camera;
        private readonly IGridService _gridService;
        private readonly GridView _gridView;
        private readonly PlayerStacksView _playerStacksView;
        private readonly GameConfig _gameConfig;
        private readonly GameFlowController _gameFlowController;

        private bool _isEnabled = true;
        private bool _isDragging;
        private int _draggedStackIndex = -1;
        private HexStackView _draggedStack;
        private Vector3 _dragOffset;
        private Vector3 _originalStackPosition; // Исходная позиция стопки для возврата
        private HexCoord _lastHighlightedCoord;

        public bool IsDragging => _isDragging;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public event Action OnDragStarted;
        public event Action OnDragEnded;
        public event Action<HexCoord> OnStackDropped;

        public DragDropController(
            Camera camera,
            IGridService gridService,
            GridView gridView,
            PlayerStacksView playerStacksView,
            GameConfig gameConfig,
            GameFlowController gameFlowController)
        {
            _camera = camera;
            _gridService = gridService;
            _gridView = gridView;
            _playerStacksView = playerStacksView;
            _gameConfig = gameConfig;
            _gameFlowController = gameFlowController;
        }

        public void Tick()
        {
            if (!_isEnabled) return;
            if (_gameFlowController.CurrentState != GameState.Gameplay &&
                _gameFlowController.CurrentState != GameState.Tutorial)
            {
                return;
            }

            HandleInput();
        }

        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                TryStartDrag();
            }
            else if (Input.GetMouseButton(0) && _isDragging)
            {
                UpdateDrag();
            }
            else if (Input.GetMouseButtonUp(0) && _isDragging)
            {
                EndDrag();
            }
        }

        private void TryStartDrag()
        {
            var worldPos = GetWorldPosition();
            if (!worldPos.HasValue) return;

            if (_playerStacksView.TryGetStackAtPosition(worldPos.Value, _gameConfig.HexSize, out int index, out var stack))
            {
                _isDragging = true;
                _draggedStackIndex = index;
                _draggedStack = stack;
                _originalStackPosition = stack.transform.position; // Сохраняем исходную позицию
                _dragOffset = stack.transform.position - worldPos.Value;
                _dragOffset.y = 0;

                // Поднимаем стопку
                var pos = stack.transform.position;
                pos.y = _gameConfig.DragHeight;
                stack.transform.position = pos;

                OnDragStarted?.Invoke();
            }
        }

        private void UpdateDrag()
        {
            if (_draggedStack == null) return;

            var worldPos = GetWorldPosition();
            if (!worldPos.HasValue) return;

            // Двигаем стопку
            var newPos = worldPos.Value + _dragOffset;
            newPos.y = _gameConfig.DragHeight;
            _draggedStack.transform.position = newPos;

            // Подсветка ячейки
            UpdateHighlight(worldPos.Value);
        }

        private void EndDrag()
        {
            if (_draggedStack == null)
            {
                _isDragging = false;
                return;
            }

            var worldPos = GetWorldPosition();
            bool placed = false;

            if (worldPos.HasValue)
            {
                // Пытаемся найти ближайшую валидную ячейку
                if (_gridService.TryGetNearestValidCell(worldPos.Value, _gameConfig.SnapDistance * 2, out var coord))
                {
                    // Пытаемся разместить стопку (PlaceStack проверит что ячейка свободна)
                    placed = TryPlaceStack(coord);
                }
            }

            if (!placed)
            {
                // Возвращаем стопку на место
                ReturnStackToOriginalPosition();
            }

            // Очищаем подсветку
            _gridView.ClearAllHighlights();
            _lastHighlightedCoord = HexCoord.Invalid;

            _isDragging = false;
            _draggedStack = null;
            _draggedStackIndex = -1;

            OnDragEnded?.Invoke();
        }

        private bool TryPlaceStack(HexCoord coord)
        {
            // Получаем данные ячейки
            var gridData = _gridService.GridData;
            var cellData = gridData.GetCell(coord);
            
            // ПРОВЕРКА: Можем ли разместить стопку в эту ячейку (она должна быть пуста)
            if (cellData == null || !cellData.CanPlaceStack)
            {
                // Ячейка занята или заблокирована - не можем разместить
                return false;
            }

            // Создаём данные стопки из view
            var stackData = new HexStackData();
            foreach (var hexView in _draggedStack.HexViews)
            {
                stackData.AddPiece(new HexPieceData(hexView.CurrentColor));
            }

            // Размещаем в данных
            cellData.SetStack(stackData);

            // Размещаем визуально
            var cellView = _gridView.GetCellView(coord);
            if (cellView != null)
            {
                var worldPos = _gridService.CoordToWorldPosition(coord);
                _draggedStack.transform.position = worldPos;
                
                // ВАЖНО: Сначала удаляем из PlayerStacksView БЕЗ Destroy
                _playerStacksView.RemoveStackWithoutDestroy(_draggedStackIndex);
                
                // ПОТОМ добавляем в cellView (теперь стопка не будет удалена)
                cellView.SetStackView(_draggedStack);
            }

            OnStackDropped?.Invoke(coord);
            _gameFlowController.OnStackPlaced(coord);
            
            return true;
        }

        private void ReturnStackToOriginalPosition()
        {
            // Возвращаем стопку на её исходную позицию
            if (_draggedStack != null)
            {
                _draggedStack.transform.position = _originalStackPosition;
            }
        }

        private void UpdateHighlight(Vector3 worldPos)
        {
            // Убираем старую подсветку
            if (_lastHighlightedCoord.IsValid)
            {
                _gridView.HighlightCell(_lastHighlightedCoord, CellHighlightState.None);
            }

            // Находим новую ячейку
            if (_gridService.TryWorldPositionToCoord(worldPos, out var coord))
            {
                bool isValid = _gridService.IsValidDropPosition(coord);
                _gridView.HighlightCell(coord, isValid ? CellHighlightState.Valid : CellHighlightState.Invalid);
                _lastHighlightedCoord = coord;
            }
            else
            {
                _lastHighlightedCoord = HexCoord.Invalid;
            }
        }

        private Vector3? GetWorldPosition()
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            var plane = new Plane(Vector3.up, Vector3.zero);

            if (plane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return null;
        }
    }
}

