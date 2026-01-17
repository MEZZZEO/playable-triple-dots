using System.Collections;
using UnityEngine;

namespace TripleDots
{
    /// <summary>
    /// Отвечает за анимацию перемещения HexPieceView между стопками
    /// Перемещает элемент из исходной стопки в целевую с полётом на высоте и поворотом
    /// </summary>
    public class HexPieceAnimator
    {
        private readonly HexPieceView _hexView;
        private readonly float _animationDuration;
        private readonly float _flyHeight; // Высота полёта

        public HexPieceAnimator(HexPieceView hexView, float animationDuration, float flyHeight = 1.5f)
        {
            _hexView = hexView;
            _animationDuration = animationDuration;
            _flyHeight = flyHeight;
        }

        /// <summary>
        /// Анимирует перемещение элемента из исходной позиции в целевую с полётом на высоте
        /// Элемент поворачивается в сторону целевой стопки
        /// </summary>
        public IEnumerator AnimatePieceMove(Vector3 startPos, Vector3 endPos, Transform targetStackTransform = null)
        {
            float elapsed = 0f;
            Vector3 originalRotation = _hexView.transform.eulerAngles;
            
            // Вычисляем направление от start к end для поворота
            Vector3 direction = (endPos - startPos).normalized;
            
            // Вычисляем нужный угол поворота вокруг Y для поворота в сторону целевой стопки
            float targetRotationY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            
            // Если передана целевая стопка, используем её позицию для более точного поворота
            if (targetStackTransform != null)
            {
                Vector3 toTarget = (targetStackTransform.position - _hexView.transform.position).normalized;
                targetRotationY = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
            }

            while (elapsed < _animationDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / _animationDuration;
                
                // Парабола для полёта: поднимаемся в начале, опускаемся в конце
                float arcHeight = Mathf.Sin(progress * Mathf.PI) * _flyHeight;
                
                // Плавное перемещение из start в end с аркой вверх
                Vector3 horizontalPos = Vector3.Lerp(startPos, endPos, progress);
                Vector3 currentPos = horizontalPos + Vector3.up * arcHeight;
                _hexView.transform.position = currentPos;

                // Поворот: элемент смотрит в сторону целевой стопки
                // Одновременно поворачивается на 180° вокруг X (переворот)
                float rotationX = Mathf.Lerp(0f, 180f, progress);
                float rotationY = Mathf.Lerp(originalRotation.y, targetRotationY, progress);
                _hexView.transform.eulerAngles = new Vector3(rotationX, rotationY, originalRotation.z);

                yield return null;
            }

            // Финальное состояние
            _hexView.transform.position = endPos;
            _hexView.transform.eulerAngles = originalRotation;
        }
    }

    /// <summary>
    /// Отвечает за анимацию исчезновения стопки с эффектом "утопания"
    /// Каждый элемент начинает с разного масштаба и "утопает" одновременно
    /// </summary>
    public class StackDisappearAnimator
    {
        private readonly HexStackView _stackView;
        private readonly float _animationDuration;
        private readonly float _sinkDistance;

        public StackDisappearAnimator(HexStackView stackView, float animationDuration, float sinkDistance = 2f)
        {
            _stackView = stackView;
            _animationDuration = animationDuration;
            _sinkDistance = sinkDistance;
        }

        /// <summary>
        /// Анимирует исчезновение стопки: масштабирование + спускание вниз
        /// Каждый элемент начинает с разного масштаба: 0.2, 0.3, 0.4, ..., N*0.1
        /// Все элементы "утопают" одновременно
        /// </summary>
        public IEnumerator AnimateDisappear()
        {
            var hexViews = _stackView.HexViews;
            if (hexViews.Count == 0) yield break;

            float elapsed = 0f;

            // Сохраняем начальные значения
            float[] initialScales = new float[hexViews.Count];
            Vector3[] initialPositions = new Vector3[hexViews.Count];

            for (int i = 0; i < hexViews.Count; i++)
            {
                // Начальный масштаб: 0.2, 0.3, 0.4, ..., N*0.1
                initialScales[i] = 0.2f + (i * 0.1f);
                initialPositions[i] = hexViews[i].transform.localPosition;
            }

            while (elapsed < _animationDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / _animationDuration;

                for (int i = 0; i < hexViews.Count; i++)
                {
                    // Масштаб от начального значения к 0
                    float targetScale = Mathf.Lerp(initialScales[i], 0f, progress);
                    hexViews[i].transform.localScale = Vector3.one * targetScale;

                    // Позиция спускается вниз (Y координата уменьшается)
                    // Используется InQuad easing для ускорения при "падении"
                    float easeProgress = progress * progress; // InQuad
                    float sinkDistance = easeProgress * _sinkDistance;
                    
                    Vector3 newPos = initialPositions[i];
                    newPos.y -= sinkDistance;
                    hexViews[i].transform.localPosition = newPos;
                }

                yield return null;
            }

            // Финальное состояние
            for (int i = 0; i < hexViews.Count; i++)
            {
                hexViews[i].transform.localScale = Vector3.zero;
            }
        }
    }
}
