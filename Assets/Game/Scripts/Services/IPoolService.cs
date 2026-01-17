using UnityEngine;

namespace TripleDots
{
    public interface IPoolService
    {
        void Initialize();
        T Get<T>(GameObject prefab) where T : Component;
        void Return<T>(T item) where T : Component;
        void Prewarm<T>(GameObject prefab, int count) where T : Component;
        void Clear();
    }
}