using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TripleDots
{
    public class GridView : MonoBehaviour
    {
        [SerializeField] private Transform _cellContainer;
        [SerializeField] private float _flipAnimationDuration = 0.3f;
        [SerializeField] private float _disappearAnimationDuration = 0.5f;

        private readonly Dictionary<HexCoord, HexCellView> _cellViews = new();
        private IGridService _gridService;
        private IColorService _colorService;
        private IHexViewPool _hexViewPool;
        private IChainReactionService _chainReactionService;
        private GameConfig _gameConfig;
        private PrefabRefs _prefabRefs;
        private MonoBehaviour _coroutineRunner;

        public IReadOnlyDictionary<HexCoord, HexCellView> CellViews => _cellViews;

        public void Initialize(
            IGridService gridService,
            IColorService colorService,
            IHexViewPool hexViewPool,
            IChainReactionService chainReactionService,
            GameConfig gameConfig,
            PrefabRefs prefabRefs)
        {
            _gridService = gridService;
            _colorService = colorService;
            _hexViewPool = hexViewPool;
            _chainReactionService = chainReactionService;
            _gameConfig = gameConfig;
            _prefabRefs = prefabRefs;
            _coroutineRunner = this; // Используем сам GridView для запуска корутин

            if (_cellContainer == null)
            {
                _cellContainer = transform;
            }

            // Подписываемся на события цепной реакции
            _chainReactionService.OnChainStep += OnChainStep;
            
            // Подписываемся на событие синхронизации
            if (_chainReactionService is ChainReactionService service)
            {
                service.OnSyncRequired += SyncVisualizationWithData;
            }

            CreateCellViews();
        }

        public void LoadLevel(GeneratedLevel level)
        {
            ClearAllStacks();

            foreach (var kvp in level.GridStacks)
            {
                var coord = kvp.Key;
                var stackData = kvp.Value;

                if (_cellViews.TryGetValue(coord, out var cellView))
                {
                    var stackView = CreateStackView();
                    stackView.BuildFromData(stackData);
                    cellView.SetStackView(stackView);
                }
            }
        }

        public HexCellView GetCellView(HexCoord coord)
        {
            return _cellViews.GetValueOrDefault(coord);
        }

        public bool TryGetCellViewAtWorldPosition(Vector3 worldPos, out HexCellView cellView)
        {
            cellView = null;

            if (_gridService.TryWorldPositionToCoord(worldPos, out var coord))
            {
                return _cellViews.TryGetValue(coord, out cellView);
            }

            return false;
        }

        public void HighlightCell(HexCoord coord, CellHighlightState state)
        {
            if (_cellViews.TryGetValue(coord, out var cellView))
            {
                cellView.SetHighlight(state);
            }
        }

        public void ClearAllHighlights()
        {
            foreach (var cellView in _cellViews.Values)
            {
                cellView.SetHighlight(CellHighlightState.None);
            }
        }

        /// <summary>
        /// Синхронизирует визуализацию с данными после изменений.
        /// Обновляет все стопки согласно текущему состоянию GridData.
        /// </summary>
        public void SyncVisualizationWithData()
        {
            var gridData = _gridService.GridData;
            
            foreach (var cellView in _cellViews.Values)
            {
                var cellData = gridData.GetCell(cellView.Coord);
                
                if (cellData == null)
                    continue;
                
                // Если ячейка пустая - очищаем визуализацию
                if (cellData.IsEmpty)
                {
                    if (cellView.StackView != null)
                    {
                        cellView.ClearStack();
                    }
                }
                else
                {
                    // Если ячейка заполнена - обновляем стопку
                    // Проверяем совпадает ли визуализация с данными
                    if (cellView.StackView == null)
                    {
                        var stackView = CreateStackView();
                        stackView.BuildFromData(cellData.Stack);
                        cellView.SetStackView(stackView);
                    }
                    else
                    {
                        // Проверяем соответствие количества элементов
                        if (cellView.StackView.HexViews.Count != cellData.Stack.Count)
                        {
                            // Пересоздаём стопку если не совпадает
                            cellView.ClearStack();
                            var stackView = CreateStackView();
                            stackView.BuildFromData(cellData.Stack);
                            cellView.SetStackView(stackView);
                        }
                    }
                }
            }
        }

        public HexStackView CreateStackView()
        {
            var go = new GameObject("StackView");
            var stackView = go.AddComponent<HexStackView>();
            stackView.Initialize(_hexViewPool, _colorService, _gameConfig.HexStackHeight);
            return stackView;
        }

        private void CreateCellViews()
        {
            var gridData = _gridService.GridData;

            foreach (var cellData in gridData.GetAllCells())
            {
                var worldPos = _gridService.CoordToWorldPosition(cellData.Coord);
                var cellView = CreateCellView(cellData.Coord, worldPos);
                _cellViews[cellData.Coord] = cellView;
            }
        }

        private HexCellView CreateCellView(HexCoord coord, Vector3 position)
        {
            GameObject cellGo;

            if (_prefabRefs != null && _prefabRefs.CellBackgroundPrefab != null)
            {
                cellGo = Instantiate(_prefabRefs.CellBackgroundPrefab, _cellContainer);
            }
            else
            {
                cellGo = new GameObject($"Cell_{coord.Col}_{coord.Row}");
                cellGo.transform.SetParent(_cellContainer);
            }

            cellGo.transform.position = position;

            var cellView = cellGo.GetComponent<HexCellView>();
            if (cellView == null)
            {
                cellView = cellGo.AddComponent<HexCellView>();
            }

            cellView.Initialize(coord, _colorService);
            return cellView;
        }

        private void ClearAllStacks()
        {
            foreach (var cellView in _cellViews.Values)
            {
                cellView.ClearStack();
            }
        }

        private void OnChainStep(ChainStep step)
        {
            // Запускаем анимированную обработку шага цепной реакции
            _coroutineRunner.StartCoroutine(AnimateChainStepAndNotify(step));
        }

        /// <summary>
        /// Анимирует шаг и уведомляет о завершении.
        /// </summary>
        private IEnumerator AnimateChainStepAndNotify(ChainStep step)
        {
            yield return _coroutineRunner.StartCoroutine(AnimateChainStep(step));
            
            // ВАЖНО: Уведомляем ChainReactionService о завершении анимации
            (_chainReactionService as ChainReactionService)?.NotifyAnimationCompleted();
        }

        private IEnumerator AnimateChainStep(ChainStep step)
        {
            // Анимируем слияния поэтапно, одно за другим с задержками
            foreach (var operation in step.Operations)
            {
                var sourceCell = _cellViews.GetValueOrDefault(operation.SourceCoord);
                var targetCell = _cellViews.GetValueOrDefault(operation.TargetCoord);

                if (sourceCell?.StackView != null && targetCell?.StackView != null)
                {
                    // Запускаем перелёты элементов параллельно с небольшой задержкой между стартами
                    var moveCoroutines = new List<Coroutine>();

                    for (int i = 0; i < operation.PieceCount; i++)
                    {
                        // Запускаем перелёт без ожидания (StartCoroutine вместо yield return)
                        var coroutine = _coroutineRunner.StartCoroutine(
                            AnimatePieceMove(sourceCell, targetCell, operation.Color));
                        moveCoroutines.Add(coroutine);

                        // Небольшая задержка перед запуском следующего перелёта
                        // (чтобы они не все стартовали одновременно, а красиво друг за другом)
                        if (i < operation.PieceCount - 1)
                        {
                            yield return new WaitForSeconds(_gameConfig.PieceFlyCascadeDelay);
                        }
                    }

                    // Ждём пока ВСЕ перелёты завершатся
                    foreach (var coroutine in moveCoroutines)
                    {
                        yield return coroutine;
                    }

                    // Задержка перед следующей операцией
                    yield return new WaitForSeconds(0.2f);
                }
            }

            // Затем анимируем исчезновение стопок (если они есть)
            foreach (var coord in step.DisappearingStacks)
            {
                var cellView = _cellViews.GetValueOrDefault(coord);
                if (cellView?.StackView != null)
                {
                    // Находим операцию для этой ячейки чтобы узнать цвет
                    var operation = step.Operations.Find(op => op.TargetCoord == coord);
                    if (operation.PieceCount > 0) // struct - проверяем валидность по PieceCount
                    {
                        yield return _coroutineRunner.StartCoroutine(
                            AnimateStackDisappear(cellView, operation.Color));
                    }
                }
            }
        }

        private IEnumerator AnimatePieceMove(HexCellView sourceCell, HexCellView targetCell, HexColor color)
        {
            // ВАЖНО: Визуализация не меняет данные, только отображает
            // Данные будут изменены позже в ApplyStep

            // Получаем верхний элемент ИЗ стопки для анимации
            var hexViews = sourceCell.StackView.HexViews;
            if (hexViews.Count == 0) yield break;

            HexPieceView hexView = null;
            int hexViewIndex = -1;

            // КРИТИЧНО: Ищем первый элемент нужного цвета СВЕРХУ
            for (int i = hexViews.Count - 1; i >= 0; i--)
            {
                if (hexViews[i].CurrentColor == color)
                {
                    hexView = hexViews[i];
                    hexViewIndex = i;
                    break;
                }
            }

            // Если не нашли элемент нужного цвета - ошибка, выходим
            if (hexView == null || hexViewIndex < 0)
            {
                Debug.LogWarning($"AnimatePieceMove: Не найден элемент цвета {color} в стопке {sourceCell.Coord}");
                yield break;
            }

            // Проверяем что это ВЕРХНИЙ элемент (должен быть последним в списке)
            if (hexViewIndex != hexViews.Count - 1)
            {
                Debug.LogWarning($"AnimatePieceMove: Элемент цвета {color} не является верхним в стопке {sourceCell.Coord}");
                yield break;
            }

            // Сохраняем начальную позицию элемента в мировых координатах ДО удаления
            Vector3 startWorldPos = hexView.transform.position;

            // Удаляем ВЕРХНИЙ элемент из визуальной стопки (который мы нашли)
            var removedHex = sourceCell.StackView.RemoveTopHex();

            // Проверяем что удалили правильный элемент
            if (removedHex != hexView)
            {
                Debug.LogError($"AnimatePieceMove: Удалён неправильный элемент! Ожидался {color}, удалён {removedHex?.CurrentColor}");
                // Возвращаем обратно если ошибка
                if (removedHex != null)
                {
                    _hexViewPool.Return(removedHex);
                }
                yield break;
            }

            // Восстанавливаем мировую позицию
            hexView.transform.position = startWorldPos;

            // Вычисляем конечную позицию
            Vector3 endWorldPos = targetCell.StackView.GetTopPosition();

            // Вычисляем высоту полёта
            float targetStackHeight = targetCell.StackView.transform.position.y + (targetCell.StackView.HexViews.Count * _gameConfig.HexStackHeight);
            float flyHeight = Mathf.Max(1.5f, targetStackHeight + 1f);

            // Анимация перемещения
            var animator = new HexPieceAnimator(hexView, _flipAnimationDuration, flyHeight);
            yield return _coroutineRunner.StartCoroutine(animator.AnimatePieceMove(startWorldPos, endWorldPos, targetCell.StackView.transform));

            // После анимации возвращаем в пул
            _hexViewPool.Return(hexView);

            // И создаём НОВЫЙ элемент нужного цвета в целевой стопке
            var newHex = targetCell.StackView.AddHex(color);

            // ПРОВЕРКА: убеждаемся что новый элемент имеет правильный цвет
            if (newHex.CurrentColor != color)
            {
                Debug.LogError($"AnimatePieceMove: Новый элемент имеет неправильный цвет! Ожидался {color}, получен {newHex.CurrentColor}");
                // Принудительно устанавливаем цвет
                newHex.SetColor(color);
            }
        }

        private IEnumerator AnimateStackDisappear(HexCellView cellView, HexColor colorToRemove)
        {
            if (cellView?.StackView == null) yield break;

            var stackView = cellView.StackView;
            
            if (stackView.HexViews.Count == 0) yield break;
            
            var disappearAnimator = new StackDisappearAnimator(stackView, _disappearAnimationDuration);

            yield return _coroutineRunner.StartCoroutine(disappearAnimator.AnimateDisappear());

            // После анимации удаляем только верхние элементы УКАЗАННОГО цвета
            while (stackView.HexViews.Count > 0 && stackView.HexViews[stackView.HexViews.Count - 1].CurrentColor == colorToRemove)
            {
                var hexView = stackView.RemoveTopHex();
                _hexViewPool.Return(hexView);
            }
            
            // Задержка перед следующим слиянием
            yield return new WaitForSeconds(0.2f);
        }

        private void OnDestroy()
        {
            ClearAllStacks();
            
            // Отписываемся от событий
            if (_chainReactionService != null)
            {
                _chainReactionService.OnChainStep -= OnChainStep;
            }
            
            _cellViews.Clear();
        }
    }
}

