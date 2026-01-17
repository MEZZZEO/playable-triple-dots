using System.Collections.Generic;
using UnityEngine;

namespace TripleDots
{
    public class PlayerStacksView : MonoBehaviour
    {
        [SerializeField] private Transform[] _stackPositions;
        [SerializeField] private float _stackSpacing = 2.5f;

        private readonly List<HexStackView> _stacks = new();
        private IHexViewPool _hexViewPool;
        private IColorService _colorService;
        private GameConfig _gameConfig;

        public IReadOnlyList<HexStackView> Stacks => _stacks;
        public int Count => _stacks.Count;

        public void Initialize(
            IHexViewPool hexViewPool,
            IColorService colorService,
            GameConfig gameConfig)
        {
            _hexViewPool = hexViewPool;
            _colorService = colorService;
            _gameConfig = gameConfig;
        }

        public void LoadPlayerStacks(List<HexStackData> stacksData)
        {
            Clear();

            for (int i = 0; i < stacksData.Count; i++)
            {
                var stackView = CreateStackView();
                stackView.BuildFromData(stacksData[i]);
                
                var position = GetStackPosition(i, stacksData.Count);
                stackView.transform.position = position;
                
                _stacks.Add(stackView);
            }
        }

        public HexStackView GetStackAt(int index)
        {
            if (index < 0 || index >= _stacks.Count) return null;
            return _stacks[index];
        }

        public bool TryGetStackAtPosition(Vector3 worldPos, float radius, out int index, out HexStackView stack)
        {
            index = -1;
            stack = null;

            for (int i = 0; i < _stacks.Count; i++)
            {
                var stackPos = _stacks[i].transform.position;
                var distance = Vector3.Distance(
                    new Vector3(worldPos.x, 0, worldPos.z),
                    new Vector3(stackPos.x, 0, stackPos.z));

                if (distance < radius)
                {
                    index = i;
                    stack = _stacks[i];
                    return true;
                }
            }

            return false;
        }

        public void RemoveStack(int index)
        {
            if (index < 0 || index >= _stacks.Count) return;

            var stack = _stacks[index];
            _stacks.RemoveAt(index);
            
            stack.Clear();
            Destroy(stack.gameObject);
        }

        /// <summary>
        /// Удалить стопку из списка игрока БЕЗ уничтожения GameObject
        /// (используется при перемещении стопки в ячейку сетки)
        /// </summary>
        public HexStackView RemoveStackWithoutDestroy(int index)
        {
            if (index < 0 || index >= _stacks.Count) return null;

            var stack = _stacks[index];
            _stacks.RemoveAt(index);
            
            return stack;
        }

        public void Clear()
        {
            foreach (var stack in _stacks)
            {
                stack.Clear();
                Destroy(stack.gameObject);
            }
            _stacks.Clear();
        }

        private HexStackView CreateStackView()
        {
            var go = new GameObject("PlayerStack");
            go.transform.SetParent(transform);
            
            var stackView = go.AddComponent<HexStackView>();
            stackView.Initialize(_hexViewPool, _colorService, _gameConfig.HexStackHeight);
            
            return stackView;
        }

        private Vector3 GetStackPosition(int index, int totalCount)
        {
            if (_stackPositions != null && index < _stackPositions.Length && _stackPositions[index] != null)
            {
                return _stackPositions[index].position;
            }

            // Позиционируем стопки по центру
            float totalWidth = (totalCount - 1) * _stackSpacing;
            float startX = -totalWidth / 2f;
            float x = startX + index * _stackSpacing;

            return transform.position + new Vector3(x, 0f, 0f);
        }

        private void OnDestroy()
        {
            Clear();
        }
    }
}

