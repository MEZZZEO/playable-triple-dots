using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TripleDots
{
    /// <summary>
    /// Реализация универсального пула объектов.
    /// Создаёт PoolContainer на сцене при инициализации.
    /// </summary>
    public class PoolService : IPoolService, IDisposable
    {
        private readonly Dictionary<Type, Queue<Component>> _pools = new();
        private readonly Dictionary<Type, Transform> _containers = new();
        private readonly Dictionary<Component, Type> _activeObjects = new();
        
        private Transform _rootContainer;
        private bool _isInitialized;

        public void Initialize()
        {
            if (_isInitialized) return;

            // Создаём корневой контейнер для всех пулов
            var rootGo = new GameObject("[Pool]");
            _rootContainer = rootGo.transform;
            Object.DontDestroyOnLoad(rootGo);
            
            _isInitialized = true;
        }

        public T Get<T>(GameObject prefab) where T : Component
        {
            EnsureInitialized();
            
            var type = typeof(T);
            
            // Проверяем есть ли объект в пуле
            if (_pools.TryGetValue(type, out var pool) && pool.Count > 0)
            {
                var pooledItem = pool.Dequeue() as T;
                if (pooledItem != null)
                {
                    pooledItem.gameObject.SetActive(true);
                    _activeObjects[pooledItem] = type;
                    return pooledItem;
                }
            }

            // Создаём новый объект
            var newItem = CreateInstance<T>(prefab);
            _activeObjects[newItem] = type;
            return newItem;
        }

        public void Return<T>(T item) where T : Component
        {
            if (item == null) return;
            
            EnsureInitialized();
            
            var type = typeof(T);
            
            // Удаляем из активных
            _activeObjects.Remove(item);
            
            // Деактивируем и возвращаем в пул
            item.gameObject.SetActive(false);
            item.transform.SetParent(GetOrCreateContainer(type));
            
            // Добавляем в пул
            if (!_pools.TryGetValue(type, out var pool))
            {
                pool = new Queue<Component>();
                _pools[type] = pool;
            }
            
            pool.Enqueue(item);
        }

        public void Prewarm<T>(GameObject prefab, int count) where T : Component
        {
            EnsureInitialized();
            
            var type = typeof(T);
            
            if (!_pools.TryGetValue(type, out var pool))
            {
                pool = new Queue<Component>();
                _pools[type] = pool;
            }

            for (int i = 0; i < count; i++)
            {
                var item = CreateInstance<T>(prefab);
                item.gameObject.SetActive(false);
                item.transform.SetParent(GetOrCreateContainer(type));
                pool.Enqueue(item);
            }
        }

        public void Clear()
        {
            foreach (var pool in _pools.Values)
            {
                while (pool.Count > 0)
                {
                    var item = pool.Dequeue();
                    if (item != null)
                    {
                        Object.Destroy(item.gameObject);
                    }
                }
            }
            
            _pools.Clear();
            _activeObjects.Clear();
            _containers.Clear();
        }

        public void Dispose()
        {
            Clear();
            
            if (_rootContainer != null)
            {
                Object.Destroy(_rootContainer.gameObject);
                _rootContainer = null;
            }
            
            _isInitialized = false;
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
        }

        private T CreateInstance<T>(GameObject prefab) where T : Component
        {
            var go = Object.Instantiate(prefab);
            var component = go.GetComponent<T>();
            
            if (component == null)
            {
                component = go.AddComponent<T>();
            }
            
            return component;
        }

        private Transform GetOrCreateContainer(Type type)
        {
            if (_containers.TryGetValue(type, out var container))
            {
                return container;
            }

            var containerGo = new GameObject($"[{type.Name}]");
            containerGo.transform.SetParent(_rootContainer);
            _containers[type] = containerGo.transform;
            
            return containerGo.transform;
        }
    }
}