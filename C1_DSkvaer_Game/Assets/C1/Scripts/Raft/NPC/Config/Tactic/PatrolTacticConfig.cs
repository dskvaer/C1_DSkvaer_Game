using UnityEngine;

namespace Ship {
    /// <summary>
    /// Настройки для тактики побега. Хранит параметры, определяющие поведение врага при побеге от игрока при низком здоровье.
    /// </summary>
    /// <remarks>
    /// Привязка в Unity Inspector:
    /// - Создать через Assets > Create > ShipConfigs > TacticConfigs > FleeTacticConfig.
    /// - FleeSpeed: Скорость побега (например, 1.2).
    /// - FleeDistance: Дистанция побега (например, 5).
    /// - HealthThreshold: Порог здоровья для побега (например, 0.2).
    /// - SmoothTurnSpeed: Скорость поворота (например, 0.3).
    /// Настройка сцены:
    /// - Привязать к компоненту FleeTactic на объекте Enemy_Ship.
    /// - Убедитесь, что Enemy_Ship имеет компоненты NPCAI, ShipHealth, ShipMovement.
    /// Логика работы:
    /// - Хранит параметры для FleeTactic, такие как скорость и дистанция побега.
    /// - Проверяется на null в FleeTactic.Awake.
    /// </remarks>
    [CreateAssetMenu(fileName = "FleeTacticConfig", menuName = "ShipConfigs/TacticConfigs/FleeTacticConfig", order = 10)]
    public class FleeTacticConfig : ScriptableObject {
        [SerializeField] private float fleeSpeed = 1.2f; // Скорость побега
        /// <summary>
        /// Скорость движения врага при побеге (в единицах Unity в секунду).
        /// </summary>
        public float FleeSpeed => fleeSpeed;

        [SerializeField] private float fleeDistance = 5f; // Дистанция побега
        /// <summary>
        /// Дистанция (в единицах Unity), на которую враг пытается отойти от игрока при побеге.
        /// </summary>
        public float FleeDistance => fleeDistance;

        [SerializeField] private float healthThreshold = 0.2f; // Порог здоровья
        /// <summary>
        /// Порог здоровья (в процентах от максимального), ниже которого активируется тактика побега.
        /// </summary>
        public float HealthThreshold => healthThreshold;

        [SerializeField] private float smoothTurnSpeed = 0.3f; // Скорость поворота
        /// <summary>
        /// Скорость плавного поворота к цели (значение от 0 до 1, где 1 — мгновенный поворот).
        /// </summary>
        public float SmoothTurnSpeed => smoothTurnSpeed;

        // Проверка корректности значений при загрузке
        private void OnValidate()
        {
            if (fleeSpeed < 0f) // Проверяем, что скорость не отрицательная
            {
                Debug.LogWarning($"FleeSpeed не может быть отрицательной для {name}. Установлено значение 0.", this); // Логируем предупреждение
                fleeSpeed = 0f; // Устанавливаем значение по умолчанию
            }
            if (fleeDistance < 0f) // Проверяем, что дистанция не отрицательная
            {
                Debug.LogWarning($"FleeDistance не может быть отрицательной для {name}. Установлено значение 0.", this); // Логируем предупреждение
                fleeDistance = 0f; // Устанавливаем значение по умолчанию
            }
            if (healthThreshold < 0f || healthThreshold > 1f) // Проверяем диапазон порога здоровья
            {
                Debug.LogWarning($"HealthThreshold должен быть в диапазоне [0, 1] для {name}. Установлено значение 0.2.", this); // Логируем предупреждение
                healthThreshold = 0.2f; // Устанавливаем значение по умолчанию
            }
            if (smoothTurnSpeed < 0f || smoothTurnSpeed > 1f) // Проверяем диапазон скорости поворота
            {
                Debug.LogWarning($"SmoothTurnSpeed должен быть в диапазоне [0, 1] для {name}. Установлено значение 0.3.", this); // Логируем предупреждение
                smoothTurnSpeed = 0.3f; // Устанавливаем значение по умолчанию
            }
        }
    }
}