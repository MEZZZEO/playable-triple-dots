﻿using UnityEngine;

namespace TripleDots
{
    /// <summary>
    /// Основной конфиг игры - настройки сетки, геймплея и анимаций
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "TripleDots/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Grid Settings")]
        [Tooltip("Ширина сетки в ячейках")]
        public int GridWidth = 8;
        
        [Tooltip("Высота сетки в ячейках")]
        public int GridHeight = 10;
        
        [Tooltip("Размер одного шестиугольника")]
        public float HexSize = 1f;
        
        [Tooltip("Ориентация шестиугольников")]
        public HexOrientation Orientation = HexOrientation.FlatTop;

        [Header("Hex Stack")]
        [Tooltip("Высота одного hex в стопке")]
        public float HexStackHeight = 0.15f;

        [Header("Gameplay")]
        [Tooltip("Максимальный размер стопки (исчезает при достижении)")]
        public int MaxStackSize = 10;
        
        [Tooltip("Количество стопок у игрока")]
        public int PlayerStackCount = 3;
        
        [Tooltip("Увеличение скорости после каждой установленной стопки (0.3 = +30%)")]
        [Range(0f, 1f)]
        public float SpeedIncreasePercent = 0.3f;

        [Header("Animation Durations (базовые, в секундах)")]
        [Tooltip("Длительность перелёта hex")]
        public float HexFlyDuration = 0.25f;
        
        [Tooltip("Задержка между перелётами элементов в каскаде (для красивого эффекта)")]
        public float PieceFlyCascadeDelay = 0.05f;
        
        [Tooltip("Длительность исчезновения стопки")]
        public float DisappearDuration = 0.2f;
        
        [Tooltip("Длительность установки стопки на поле")]
        public float StackPlaceDuration = 0.15f;
        
        [Tooltip("Задержка между шагами цепной реакции")]
        public float ChainReactionDelay = 0.1f;

        [Header("Drag & Drop")]
        [Tooltip("Высота подъёма стопки при перетаскивании")]
        public float DragHeight = 1.5f;
        
        [Tooltip("Расстояние для привязки к ячейке")]
        public float SnapDistance = 0.5f;

        [Header("Tutorial")]
        [Tooltip("Время бездействия до повторного показа туториала (секунды)")]
        public float InactivityTimeout = 2.5f;
        
        [Tooltip("Длительность анимации руки туториала")]
        public float TutorialHandDuration = 1.2f;

        [Header("Camera")]
        [Tooltip("Смещение камеры относительно центра сетки")]
        public Vector3 CameraOffset = new(0f, 15f, -8f);
        
        [Tooltip("Угол наклона камеры")]
        public Vector3 CameraRotation = new(60f, 0f, 0f);
    }
}