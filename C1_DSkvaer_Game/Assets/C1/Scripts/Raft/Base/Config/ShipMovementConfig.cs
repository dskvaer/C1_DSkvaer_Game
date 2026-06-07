using UnityEngine;

namespace Ship {
    /// <summary>
    /// Настройки движения корабля: скорость, ускорение, торможение, поворот и ступени гребли.
    /// </summary>
    [CreateAssetMenu(fileName = "ShipMovementConfig", menuName = "ShipConfigs/ShipMovementConfig", order = 1)]
    public class ShipMovementConfig : ScriptableObject {
        [Header("Скорость")]
        [InspectorLabel("Базовая скорость")]
        [Tooltip("Основная скорость корабля. Значение 10 обычно соответствует 100% хода.")]
        [SerializeField] private float baseSpeed = 10f;
        public float BaseSpeed => baseSpeed;

        [InspectorLabel("Максимальная скорость")]
        [Tooltip("Верхний предел скорости корабля после ускорения и модификаторов.")]
        [SerializeField] private float maxSpeed = 10f;
        public float MaxSpeed => maxSpeed;

        [InspectorLabel("Ускорение")]
        [Tooltip("Как быстро корабль набирает скорость при движении вперед.")]
        [SerializeField] private float acceleration = 20f;
        public float Acceleration => acceleration;

        [InspectorLabel("Трение воды")]
        [Tooltip("Как быстро корабль замедляется без активного движения.")]
        [SerializeField] private float friction = 10f;
        public float Friction => friction;

        [InspectorLabel("Торможение реверсом")]
        [Tooltip("Как быстро корабль сбрасывает скорость при движении назад или активном торможении.")]
        [SerializeField] private float deceleration = 20f;
        public float Deceleration => deceleration;

        [Header("Маневренность")]
        [InspectorLabel("Скорость поворота")]
        [Tooltip("Скорость вращения корабля в градусах за секунду.")]
        [SerializeField] private float turnSpeed = 90f;
        public float TurnSpeed => turnSpeed;

        [Header("Ступени гребли")]
        [InspectorLabel("Интервал гребли")]
        [Tooltip("Пауза между сменой ступеней хода при пошаговом управлении скоростью.")]
        [SerializeField] private float rowingInterval = 0.5f;
        public float RowingInterval => rowingInterval;

        [InspectorLabel("Ступени хода вперед")]
        [Tooltip("Множители базовой скорости для движения вперед. Например 0, 0.25, 0.5, 0.75, 1.")]
        [SerializeField] private float[] rowingSteps = { 0f, 0.25f, 0.5f, 0.75f, 1.0f };
        public float[] RowingSteps => rowingSteps;

        [InspectorLabel("Ступени хода назад")]
        [Tooltip("Множители базовой скорости для движения назад. Отрицательные значения дают задний ход.")]
        [SerializeField] private float[] reverseRowingSteps = { -0.25f };
        public float[] ReverseRowingSteps => reverseRowingSteps;
    }
}
