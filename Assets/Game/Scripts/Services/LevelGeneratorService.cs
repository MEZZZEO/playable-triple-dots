using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TripleDots
{
    public class LevelGeneratorService : ILevelGeneratorService
    {
        private readonly GameConfig _gameConfig;
        private readonly GenerationConfig _genConfig;
        
        private System.Random _random;
        private HexColor[] _activeColors;

        public LevelGeneratorService(GameConfig gameConfig, GenerationConfig generationConfig)
        {
            _gameConfig = gameConfig;
            _genConfig = generationConfig;
        }

        public GeneratedLevel Generate()
        {
            return Generate(_genConfig.GetSeed());
        }

        public GeneratedLevel Generate(int seed)
        {
            _random = new System.Random(seed);
            _activeColors = SelectActiveColors();

            var level = new GeneratedLevel
            {
                Seed = seed,
                GridStacks = new Dictionary<HexCoord, HexStackData>(),
                PlayerStacks = new List<HexStackData>(),
                IsValidated = false
            };

            // 1. Генерируем базовое заполнение сетки
            GenerateGridStacks(level);

            // 2. Генерируем стопки игрока с гарантией победы
            GeneratePlayerStacks(level);

            // 3. Находим лучший первый ход для туториала
            FindSuggestedFirstMove(level);

            // 4. Валидируем уровень
            if (_genConfig.EnsureWinnable)
            {
                level.IsValidated = ValidateLevel(level);
            }

            return level;
        }

        private HexColor[] SelectActiveColors()
        {
            var allColors = (HexColor[])Enum.GetValues(typeof(HexColor));
            var shuffled = allColors.OrderBy(_ => _random.Next()).ToArray();
            return shuffled.Take(_genConfig.ActiveColorsCount).ToArray();
        }

        private void GenerateGridStacks(GeneratedLevel level)
        {
            int totalCells = _gameConfig.GridWidth * _gameConfig.GridHeight;
            int targetFillCount = Mathf.RoundToInt(totalCells * _genConfig.FillPercent);

            // Собираем все доступные координаты
            var availableCoords = new List<HexCoord>();
            for (int row = 0; row < _gameConfig.GridHeight; row++)
            {
                for (int col = 0; col < _gameConfig.GridWidth; col++)
                {
                    availableCoords.Add(new HexCoord(col, row));
                }
            }

            // Перемешиваем
            availableCoords = availableCoords.OrderBy(_ => _random.Next()).ToList();

            int filledCount = 0;
            int coordIndex = 0;

            while (filledCount < targetFillCount && coordIndex < availableCoords.Count)
            {
                var coord = availableCoords[coordIndex];
                coordIndex++;

                // Создаём стопку
                var stack = GenerateStack(_genConfig.MinStackSize, _genConfig.MaxStackSize);
                level.GridStacks[coord] = stack;
                filledCount++;

                // Пытаемся создать кластер того же цвета
                if (_genConfig.CreateColorClusters && stack.TopColor.HasValue)
                {
                    var clusterColor = stack.TopColor.Value;
                    var neighbors = coord.GetNeighbors();

                    foreach (var neighborCoord in neighbors)
                    {
                        if (filledCount >= targetFillCount) break;
                        if (!IsValidGridCoord(neighborCoord)) continue;
                        if (level.GridStacks.ContainsKey(neighborCoord)) continue;

                        // С определённой вероятностью создаём соседа того же цвета
                        if (_random.NextDouble() < _genConfig.ClusterProbability)
                        {
                            var clusterStack = GenerateStackWithTopColor(clusterColor);
                            level.GridStacks[neighborCoord] = clusterStack;
                            filledCount++;

                            // Удаляем из доступных
                            availableCoords.Remove(neighborCoord);
                        }
                    }
                }
            }
        }

        private void GeneratePlayerStacks(GeneratedLevel level)
        {
            // Анализируем сетку чтобы создать эффективные стопки
            var colorCounts = CountColorsOnGrid(level);

            for (int i = 0; i < _gameConfig.PlayerStackCount; i++)
            {
                HexStackData stack;

                if (i < _genConfig.GuaranteedMerges && colorCounts.Count > 0)
                {
                    // Для гарантированных слияний выбираем цвет с наибольшим количеством
                    var bestColor = colorCounts.OrderByDescending(kvp => kvp.Value).First().Key;
                    stack = GenerateStackWithTopColor(bestColor);

                    // Уменьшаем счётчик для разнообразия
                    colorCounts[bestColor] = Math.Max(0, colorCounts[bestColor] - stack.Count);
                }
                else
                {
                    // Случайная стопка
                    stack = GenerateStack(_genConfig.PlayerStackMinSize, _genConfig.PlayerStackMaxSize);
                }

                level.PlayerStacks.Add(stack);
            }
        }

        private void FindSuggestedFirstMove(GeneratedLevel level)
        {
            if (level.PlayerStacks.Count == 0)
            {
                level.SuggestedFirstMoveStackIndex = 0;
                level.SuggestedFirstMoveTarget = HexCoord.Invalid;
                return;
            }

            int bestStackIndex = 0;
            HexCoord bestTarget = HexCoord.Invalid;
            int bestScore = -1;

            // Для каждой стопки игрока ищем лучшую позицию
            for (int stackIndex = 0; stackIndex < level.PlayerStacks.Count; stackIndex++)
            {
                var playerStack = level.PlayerStacks[stackIndex];
                if (!playerStack.TopColor.HasValue) continue;

                var targetColor = playerStack.TopColor.Value;

                // Ищем пустые ячейки рядом с стопками того же цвета
                for (int row = 0; row < _gameConfig.GridHeight; row++)
                {
                    for (int col = 0; col < _gameConfig.GridWidth; col++)
                    {
                        var coord = new HexCoord(col, row);

                        // Должна быть пустая
                        if (level.GridStacks.ContainsKey(coord)) continue;

                        // Считаем соседей с тем же цветом
                        int score = CountNeighborsWithColor(coord, targetColor, level);

                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestStackIndex = stackIndex;
                            bestTarget = coord;
                        }
                    }
                }
            }

            level.SuggestedFirstMoveStackIndex = bestStackIndex;
            level.SuggestedFirstMoveTarget = bestTarget.IsValid ? bestTarget : FindAnyEmptyCell(level);
        }

        private HexStackData GenerateStack(int minSize, int maxSize)
        {
            int size = _random.Next(minSize, maxSize + 1);
            var stack = new HexStackData();

            // Выбираем 2-3 цвета для этой стопки, чтобы она была более однородной
            int colorsInStack = _random.Next(1, 3); // 1 или 2 цвета
            var stackColors = new List<HexColor>();
            
            // Выбираем случайные цвета для этой стопки
            for (int c = 0; c < colorsInStack; c++)
            {
                stackColors.Add(_activeColors[_random.Next(_activeColors.Length)]);
            }

            // Генерируем элементы стопки, используя только выбранные цвета
            for (int i = 0; i < size; i++)
            {
                var color = stackColors[_random.Next(stackColors.Count)];
                stack.AddPiece(new HexPieceData(color));
            }

            return stack;
        }

        private HexStackData GenerateStackWithTopColor(HexColor topColor)
        {
            int size = _random.Next(_genConfig.PlayerStackMinSize, _genConfig.PlayerStackMaxSize + 1);
            var stack = new HexStackData();

            // Выбираем максимум 2 цвета для стопки (topColor плюс один альтернативный)
            var stackColors = new List<HexColor> { topColor };
            
            // С 70% вероятностью добавляем второй цвет для разнообразия
            if (_random.NextDouble() < 0.7 && _activeColors.Length > 1)
            {
                // Выбираем второй цвет (но не topColor)
                var otherColor = _activeColors.FirstOrDefault(c => c != topColor);
                if (otherColor != default)
                {
                    stackColors.Add(otherColor);
                }
            }

            // Генерируем элементы: большая часть будет topColor, некоторые - альтернативный цвет
            for (int i = 0; i < size - 1; i++)
            {
                // 80% topColor, 20% альтернативный цвет (если есть)
                HexColor color;
                if (stackColors.Count > 1 && _random.NextDouble() < 0.2)
                {
                    color = stackColors[1];
                }
                else
                {
                    color = topColor;
                }
                stack.AddPiece(new HexPieceData(color));
            }

            // Верхний элемент - нужного цвета
            stack.AddPiece(new HexPieceData(topColor));

            return stack;
        }

        private Dictionary<HexColor, int> CountColorsOnGrid(GeneratedLevel level)
        {
            var counts = new Dictionary<HexColor, int>();

            foreach (var kvp in level.GridStacks)
            {
                var stack = kvp.Value;
                if (stack.TopColor.HasValue)
                {
                    var color = stack.TopColor.Value;
                    counts.TryGetValue(color, out int current);
                    counts[color] = current + stack.CountTopPiecesOfColor(color);
                }
            }

            return counts;
        }

        private int CountNeighborsWithColor(HexCoord coord, HexColor color, GeneratedLevel level)
        {
            int count = 0;
            var neighbors = coord.GetNeighbors();

            foreach (var neighbor in neighbors)
            {
                if (level.GridStacks.TryGetValue(neighbor, out var stack))
                {
                    if (stack.TopColor == color)
                    {
                        count += stack.CountTopPiecesOfColor(color);
                    }
                }
            }

            return count;
        }

        private HexCoord FindAnyEmptyCell(GeneratedLevel level)
        {
            for (int row = 0; row < _gameConfig.GridHeight; row++)
            {
                for (int col = 0; col < _gameConfig.GridWidth; col++)
                {
                    var coord = new HexCoord(col, row);
                    if (!level.GridStacks.ContainsKey(coord))
                    {
                        return coord;
                    }
                }
            }

            return HexCoord.Invalid;
        }

        private bool IsValidGridCoord(HexCoord coord)
        {
            return coord.Col >= 0 && coord.Col < _gameConfig.GridWidth &&
                   coord.Row >= 0 && coord.Row < _gameConfig.GridHeight;
        }

        private bool ValidateLevel(GeneratedLevel level)
        {
            // Простая валидация: проверяем что есть хотя бы одно возможное слияние
            foreach (var playerStack in level.PlayerStacks)
            {
                if (!playerStack.TopColor.HasValue) continue;

                var color = playerStack.TopColor.Value;

                // Ищем пустую ячейку рядом с стопкой того же цвета
                for (int row = 0; row < _gameConfig.GridHeight; row++)
                {
                    for (int col = 0; col < _gameConfig.GridWidth; col++)
                    {
                        var coord = new HexCoord(col, row);
                        if (level.GridStacks.ContainsKey(coord)) continue;

                        if (CountNeighborsWithColor(coord, color, level) > 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public List<HexStackData> GenerateNewPlayerStacks(GridData gridData, int count)
        {
            if (_random == null)
            {
                _random = new System.Random();
            }

            if (_activeColors == null || _activeColors.Length == 0)
            {
                _activeColors = SelectActiveColors();
            }

            var newStacks = new List<HexStackData>();

            // Анализируем текущее состояние сетки
            var colorCounts = CountColorsOnGrid(gridData);

            for (int i = 0; i < count; i++)
            {
                HexStackData stack;

                if (i < _genConfig.GuaranteedMerges && colorCounts.Count > 0)
                {
                    // Для гарантированных слияний выбираем цвет с наибольшим количеством
                    var bestColor = colorCounts.OrderByDescending(kvp => kvp.Value).First().Key;
                    stack = GenerateStackWithTopColor(bestColor);

                    // Уменьшаем счётчик для разнообразия
                    colorCounts[bestColor] = Math.Max(0, colorCounts[bestColor] - stack.Count);
                }
                else
                {
                    // Случайная стопка
                    stack = GenerateStack(_genConfig.PlayerStackMinSize, _genConfig.PlayerStackMaxSize);
                }

                newStacks.Add(stack);
            }

            return newStacks;
        }

        private Dictionary<HexColor, int> CountColorsOnGrid(GridData gridData)
        {
            var counts = new Dictionary<HexColor, int>();

            foreach (var cell in gridData.GetAllCells())
            {
                if (cell.Stack != null && cell.Stack.TopColor.HasValue)
                {
                    var color = cell.Stack.TopColor.Value;
                    counts.TryGetValue(color, out int current);
                    counts[color] = current + cell.Stack.CountTopPiecesOfColor(color);
                }
            }

            return counts;
        }
    }
}


