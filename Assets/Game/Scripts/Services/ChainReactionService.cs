using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TripleDots
{
    /// <summary>
    /// Новая реализация сервиса цепной реакции для классической match-3 логики.
    /// 
    /// Процесс:
    /// 1. Игрок размещает стопку в ячейку
    /// 2. Проверяем эту ячейку на возможные слияния
    /// 3. Если есть слияния - выполняем все в один шаг
    /// 4. После каждого выполнения проверяем все затронутые ячейки
    /// 5. Повторяем до тех пор, пока есть возможные слияния
    /// </summary>
    public class ChainReactionService : IChainReactionService
    {
        private readonly IGridService _gridService;
        private readonly IMergeService _mergeService;
        private readonly GameConfig _gameConfig;

        public event Action<ChainStep> OnChainStep;
        public event Action OnChainComplete;
        public event Action OnSyncRequired; // Новое событие для принудительной синхронизации
        
        // Новое событие для синхронизации
        private TaskCompletionSource<bool> _animationCompleted;

        public ChainReactionService(
            IGridService gridService,
            IMergeService mergeService,
            GameConfig gameConfig)
        {
            _gridService = gridService;
            _mergeService = mergeService;
            _gameConfig = gameConfig;
        }

        /// <summary>
        /// Устанавливает сигнал о завершении анимации.
        /// Вызывается GridView после завершения всех анимаций шага.
        /// </summary>
        public void NotifyAnimationCompleted()
        {
            _animationCompleted?.TrySetResult(true);
        }

        /// <summary>
        /// Выполняет цепную реакцию асинхронно с полной синхронизацией.
        /// </summary>
        public async Task<ChainReactionResult> ExecuteAsync(HexCoord startCoord)
        {
            var result = CalculateChainReaction(startCoord);

            foreach (var step in result.Steps)
            {
                // Создаём сигнал ожидания
                _animationCompleted = new TaskCompletionSource<bool>();
                
                // Запускаем анимацию
                OnChainStep?.Invoke(step);
                
                // ЖДЁМ пока GridView не вызовет NotifyAnimationCompleted()
                await _animationCompleted.Task;
                
                // Только ПОСЛЕ завершения анимации применяем к данным
                ApplyStep(step);
                
                // КРИТИЧНО: Требуем принудительной синхронизации визуализации
                OnSyncRequired?.Invoke();
                
                // Небольшая задержка перед следующим шагом
                await Task.Delay((int)(_gameConfig.ChainReactionDelay * 1000));
            }

            OnChainComplete?.Invoke();
            return result;
        }


        /// <summary>
        /// Вычисляет всю цепную реакцию, начиная с размещённой стопки.
        /// </summary>
        public ChainReactionResult CalculateChainReaction(HexCoord startCoord)
        {
            var result = new ChainReactionResult
            {
                Steps = new List<ChainStep>(),
                TotalMerges = 0,
                TotalDisappearedPieces = 0,
                TotalDisappearedStacks = 0
            };

            // Создаём копию сетки для симуляции
            var simulatedGrid = CloneGridData();

            // Начинаем с размещённой стопки
            var cellsToCheck = new HashSet<HexCoord> { startCoord };
            int stepIndex = 0;

            // Повторяем пока есть ячейки для проверки
            while (cellsToCheck.Count > 0)
            {
                var currentCellsToCheck = cellsToCheck;
                cellsToCheck = new HashSet<HexCoord>();

                // Проверяем каждую ячейку из текущего набора
                foreach (var coord in currentCellsToCheck)
                {
                    var cell = simulatedGrid.GetCell(coord);
                    if (cell == null || cell.IsEmpty)
                        continue;

                    // Ищем слияния для этой ячейки
                    var mergeResult = _mergeService.CalculateMerge(cell, simulatedGrid, _gameConfig.MaxStackSize);

                    if (mergeResult.HasMerge)
                    {
                        var currentStep = new ChainStep(stepIndex);

                        // Выполняем ВСЕ слияния в один шаг
                        foreach (var operation in mergeResult.Operations)
                        {
                            currentStep.Operations.Add(operation);
                            result.TotalMerges++;

                            // Применяем к симуляции
                            ApplyOperationToSimulation(operation, simulatedGrid);

                            // Добавляем затронутые ячейки для следующей проверки
                            cellsToCheck.Add(operation.SourceCoord);
                            cellsToCheck.Add(operation.TargetCoord);

                            // Если стопка исчезает, добавляем в список
                            if (operation.WillDisappear)
                            {
                                result.TotalDisappearedPieces += operation.PieceCount;
                                if (!currentStep.DisappearingStacks.Contains(operation.TargetCoord))
                                {
                                    currentStep.DisappearingStacks.Add(operation.TargetCoord);
                                }
                                result.TotalDisappearedStacks++;
                            }
                        }

                        result.Steps.Add(currentStep);
                        stepIndex++;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Применяет шаг цепной реакции к реальной сетке.
        /// </summary>
        private void ApplyStep(ChainStep step)
        {
            var gridData = _gridService.GridData;

            // Сначала выполняем все слияния
            foreach (var operation in step.Operations)
            {
                var sourceCell = gridData.GetCell(operation.SourceCoord);
                var targetCell = gridData.GetCell(operation.TargetCoord);

                if (sourceCell?.Stack == null || targetCell?.Stack == null)
                    continue;

                // Переносим верхние элементы нужного цвета
                var piecesToMove = sourceCell.Stack.RemoveTopPiecesOfColor(operation.Color);
                if (piecesToMove != null)
                {
                    targetCell.Stack.AddPieces(piecesToMove);
                }
            }

            // Потом удаляем исчезающие стопки
            foreach (var coord in step.DisappearingStacks)
            {
                var cell = gridData.GetCell(coord);
                if (cell?.Stack == null)
                    continue;

                // Находим операцию для этой ячейки чтобы узнать цвет
                var operation = step.Operations.FirstOrDefault(op => op.TargetCoord == coord);
                if (operation.PieceCount > 0) // Проверяем валидность struct'а
                {
                    cell.Stack.RemoveTopPiecesOfColor(operation.Color);
                }
            }
        }

        /// <summary>
        /// Применяет операцию слияния к симулированной сетке.
        /// </summary>
        private void ApplyOperationToSimulation(MergeOperation operation, GridData simulatedGrid)
        {
            var sourceCell = simulatedGrid.GetCell(operation.SourceCoord);
            var targetCell = simulatedGrid.GetCell(operation.TargetCoord);

            if (sourceCell?.Stack == null || targetCell?.Stack == null)
                return;

            // Переносим элементы
            var piecesToMove = sourceCell.Stack.RemoveTopPiecesOfColor(operation.Color);
            targetCell.Stack.AddPieces(piecesToMove);

            // Если стопка должна исчезнуть, удаляем её верхние элементы
            if (operation.WillDisappear)
            {
                targetCell.Stack.RemoveTopPiecesOfColor(operation.Color);
            }
        }

        /// <summary>
        /// Создаёт копию сетки для симуляции.
        /// </summary>
        private GridData CloneGridData()
        {
            var original = _gridService.GridData;
            var clone = new GridData(original.Width, original.Height);

            foreach (var cell in original.GetAllCells())
            {
                if (!cell.IsEmpty)
                {
                    // Клонируем стопку через конструктор, передавая pieces
                    var clonedStack = new HexStackData(cell.Stack.Pieces);
                    clone.GetCell(cell.Coord).SetStack(clonedStack);
                }
            }

            return clone;
        }
    }
}
