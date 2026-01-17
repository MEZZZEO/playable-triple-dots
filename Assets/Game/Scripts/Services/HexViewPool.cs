using UnityEngine;

namespace TripleDots
{
    /// <summary>
    /// Реализация специализированного пула для HexPieceView.
    /// Использует общий PoolService и настраивает объекты при получении.
    /// </summary>
    public class HexViewPool : IHexViewPool
    {
        private readonly IPoolService _poolService;
        private readonly IColorService _colorService;
        private readonly PrefabRefs _prefabRefs;

        public HexViewPool(
            IPoolService poolService,
            IColorService colorService,
            PrefabRefs prefabRefs)
        {
            _poolService = poolService;
            _colorService = colorService;
            _prefabRefs = prefabRefs;
        }

        public void Initialize()
        {
            // Прогреваем пул начальным количеством объектов
            Prewarm(50);
        }

        public HexPieceView Get()
        {
            var view = _poolService.Get<HexPieceView>(_prefabRefs.HexPiecePrefab);
            
            // Инициализируем view сервисами если нужно
            view.Initialize(_colorService, _prefabRefs.HexSharedMaterial);
            
            return view;
        }

        public void Return(HexPieceView view)
        {
            if (view == null) return;
            
            view.ResetState();
            _poolService.Return(view);
        }

        public void Prewarm(int count)
        {
            _poolService.Prewarm<HexPieceView>(_prefabRefs.HexPiecePrefab, count);
        }
    }
}
