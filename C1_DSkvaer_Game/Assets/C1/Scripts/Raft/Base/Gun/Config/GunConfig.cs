using UnityEngine;

namespace Ship {
    [CreateAssetMenu(fileName = "GunConfig", menuName = "ShipConfigs/Gun/GunConfig")]
    public sealed class GunConfig : ScriptableObject {
        [Header("Стрельба")]
        [InspectorLabel("Скорострельность")]
        [Tooltip("Количество выстрелов в секунду. Чем выше значение, тем быстрее орудие готовится к следующему выстрелу.")]
        [SerializeField, Min(0.01f)] private float fireRate = 2f;
        [InspectorLabel("Задержка очереди")]
        [Tooltip("Пауза между последовательными выстрелами нескольких пушек на одном корабле.")]
        [SerializeField, Min(0f)] private float sequentialFireDelay = 0.12f;

        [Header("Старый разброс")]
        [InspectorLabel("Точность")]
        [Tooltip("Старый параметр точности. Оставлен для совместимости с существующими настройками.")]
        [SerializeField, Range(0f, 100f)] private float accuracy = 80f;
        [InspectorLabel("Максимальный разброс")]
        [Tooltip("Старый максимальный угол разброса выстрела.")]
        [SerializeField, Min(0f)] private float spreadAngle = 30f;
        [InspectorLabel("Минимальный угол разброса")]
        [Tooltip("Старый нижний предел разброса. Обычно отрицательное значение от максимального разброса.")]
        [SerializeField] private float minSpreadAngle = -30f;

        [Header("Прицеливание")]
        [InspectorLabel("Разброс без прицеливания")]
        [Tooltip("Ширина конуса выстрела при быстром нажатии без удержания кнопки.")]
        [SerializeField, Min(0f)] private float maxAimSpreadAngle = 30f;
        [InspectorLabel("Минимальный разброс")]
        [Tooltip("Самый узкий конус при полном наведении. Для арбалета можно поставить 0.")]
        [SerializeField, Min(0f)] private float minAimSpreadAngle = 0f;
        [InspectorLabel("Время сведения")]
        [Tooltip("Сколько секунд нужно удерживать выстрел, чтобы разброс сузился до минимального значения.")]
        [SerializeField, Min(0.01f)] private float aimFocusTime = 1.25f;

        public float FireRate => fireRate;
        public float SequentialFireDelay => sequentialFireDelay;
        public float Accuracy => accuracy;
        public float SpreadAngle => spreadAngle;
        public float MinSpreadAngle => minSpreadAngle;
        public float MaxAimSpreadAngle => maxAimSpreadAngle;
        public float MinAimSpreadAngle => minAimSpreadAngle;
        public float AimFocusTime => aimFocusTime;

        public float GetFocusedSpread(float holdTime)
        {
            float t = Mathf.Clamp01(holdTime / Mathf.Max(0.01f, aimFocusTime));
            return Mathf.Lerp(maxAimSpreadAngle, minAimSpreadAngle, t);
        }

        private void OnValidate()
        {
            fireRate = Mathf.Max(0.01f, fireRate);
            sequentialFireDelay = Mathf.Max(0f, sequentialFireDelay);
            accuracy = Mathf.Clamp(accuracy, 0f, 100f);
            spreadAngle = Mathf.Max(0f, spreadAngle);
            if (minSpreadAngle > spreadAngle) minSpreadAngle = -spreadAngle;
            maxAimSpreadAngle = Mathf.Max(0f, maxAimSpreadAngle);
            minAimSpreadAngle = Mathf.Clamp(minAimSpreadAngle, 0f, maxAimSpreadAngle);
            aimFocusTime = Mathf.Max(0.01f, aimFocusTime);
        }
    }
}
