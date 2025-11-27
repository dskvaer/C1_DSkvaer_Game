using UnityEngine;

namespace Ship {
    /// <summary>
    /// Настройки движения корабля (скорость, ускорение, повороты, гребля).
    /// Универсальны для игрока, врагов и торговцев.
    /// </summary>
    /// <remarks>
    /// Привязка в Unity Inspector:
    /// - Привязать к компоненту ShipMovement на объекте корабля (Player_Ship, Enemy_Ship, Trader_Ship).
    /// - Убедитесь, что RowingSteps и ReverseRowingSteps содержат хотя бы один элемент, если используется механика гребли.
    /// - Для кораблей без гребли (например, торговцев) задайте пустые массивы RowingSteps и ReverseRowingSteps.
    /// Настройка сцены:
    /// - Создайте ScriptableObject через меню (File > Create > ShipConfigs > ShipMovementConfig).
    /// </remarks>
    [CreateAssetMenu(fileName = "ShipMovementConfig", menuName = "ShipConfigs/ShipMovementConfig", order = 1)]
    public class ShipMovementConfig : ScriptableObject {
        [SerializeField] private float baseSpeed = 10f; // Базовая скорость
        /// <summary>
        /// Базовая скорость корабля (в морских узлах, 10 узлов = 100% скорости).
        /// Используется в ShipMovement для расчёта скорости.
        /// </summary>
        public float BaseSpeed => baseSpeed;

        [SerializeField] private float maxSpeed = 10f; // Максимальная скорость
        /// <summary>
        /// Максимальная скорость корабля (ограничение, обычно равно BaseSpeed).
        /// Используется в ShipMovement для ограничения скорости.
        /// </summary>
        public float MaxSpeed => maxSpeed;

        [SerializeField] private float acceleration = 20f; // Ускорение
        /// <summary>
        /// Ускорение (как быстро корабль достигает целевой скорости, в единицах/с).
        /// Используется в ShipMovement для плавного разгона.
        /// </summary>
        public float Acceleration => acceleration;

        [SerializeField] private float friction = 10f; // Трение
        /// <summary>
        /// Трение (как быстро корабль замедляется без ввода, в единицах/с).
        /// Используется в ShipMovement для замедления.
        /// </summary>
        public float Friction => friction;

        [SerializeField] private float deceleration = 20f; // Замедление отбрасывания
        /// <summary>
        /// Замедление отбрасывания (как быстро гасится импульс от столкновений, в единицах/с).
        /// Используется в ShipMovement для обработки отбрасывания.
        /// </summary>
        public float Deceleration => deceleration;

        [SerializeField] private float turnSpeed = 90f; // Скорость поворота
        /// <summary>
        /// Скорость поворота (градусы в секунду).
        /// Используется в ShipMovement для поворотов.
        /// </summary>
        public float TurnSpeed => turnSpeed;

        [SerializeField] private float rowingInterval = 0.5f; // Интервал гребков
        /// <summary>
        /// Интервал между гребками (в секундах, как часто можно "переключать передачу").
        /// Используется в ShipMovement для механики гребли.
        /// </summary>
        public float RowingInterval => rowingInterval;

        [SerializeField] private float[] rowingSteps = { 0f, 0.25f, 0.5f, 0.75f, 1.0f }; // Уровни гребков вперёд
        /// <summary>
        /// Уровни скорости гребков вперёд (0%, 25%, 50%, 75%, 100% от BaseSpeed).
        /// Используется в ShipMovement для движения вперёд.
        /// </summary>
        public float[] RowingSteps => rowingSteps;

        [SerializeField] private float[] reverseRowingSteps = { -0.25f }; // Уровни гребков назад
        /// <summary>
        /// Уровни скорости гребков назад (-25% от BaseSpeed).
        /// Используется в ShipMovement для движения назад.
        /// </summary>
        public float[] ReverseRowingSteps => reverseRowingSteps;
    }
}