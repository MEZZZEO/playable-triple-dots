using UnityEngine;

namespace TripleDots
{
    /// <summary>
    /// Ссылки на префабы и материалы.
    /// Используется для создания объектов через пул без ссылок на сцену.
    /// </summary>
    [CreateAssetMenu(fileName = "PrefabRefs", menuName = "TripleDots/PrefabRefs")]
    public class PrefabRefs : ScriptableObject
    {
        [Header("Hex")]
        [Tooltip("Префаб шестиугольника")]
        public GameObject HexPiecePrefab;
        
        [Tooltip("Единственный shared материал для всех hex (используется с MaterialPropertyBlock)")]
        public Material HexSharedMaterial;

        [Header("Grid")]
        [Tooltip("Префаб фона ячейки сетки")]
        public GameObject CellBackgroundPrefab;

        [Header("UI")]
        [Tooltip("Префаб руки туториала")]
        public GameObject TutorialHandPrefab;
        
        [Tooltip("Префаб Packshot UI")]
        public GameObject PackshotPrefab;

        [Header("Effects")]
        [Tooltip("Префаб эффекта исчезновения (опционально)")]
        public GameObject DisappearEffectPrefab;

        /// <summary>
        /// Проверить что все обязательные ссылки назначены
        /// </summary>
        public bool Validate(out string error)
        {
            if (HexPiecePrefab == null)
            {
                error = "HexPiecePrefab is not assigned";
                return false;
            }
            
            if (HexSharedMaterial == null)
            {
                error = "HexSharedMaterial is not assigned";
                return false;
            }
            
            error = null;
            return true;
        }

        private void OnValidate()
        {
            if (HexPiecePrefab == null)
            {
                Debug.LogWarning($"[{name}] HexPiecePrefab is not assigned!");
            }
            
            if (HexSharedMaterial == null)
            {
                Debug.LogWarning($"[{name}] HexSharedMaterial is not assigned!");
            }
        }
    }
}