using UnityEngine;

namespace Ship {
    /// <summary>
    /// Настройки для тактики патрулирования. Хранит параметры, определяющие поведение врага при движении по случайным точкам в пределах водного тайлмапа.
    /// </summary>
    /// <remarks>
    /// Привязка в Unity Inspector:
    /// - Создать через Assets > Create > ShipConfigs > TacticConfigs > PatrolTacticConfig.
    /// - PatrolSpeed: Скорость патрулирования (например, 0.5).
    /// - MinPatrolTime: Минимальное время движения к точке (например, 3).
    /// - MaxPatrolTime: Максимальное время движения к точке (например, 7).
    /// - SmoothTurnSpeed: Скорость поворота (например, 0.3).
    /// Настройка сцены:
    /// - Привязать к компоненту PatrolTactic на объекте Enemy_Ship.
    /// </remarks>
    [CreateAssetMenu(fileName = "PatrolTacticConfig", menuName = "ShipConfigs/TacticConfigs/PatrolTacticConfig", order = 8)]
    public class PatrolTacticConfig : ScriptableObject {
        [SerializeField] private float patrolSpeed = 0.5f; // Скорость патрулирования
        /// <summary>
        /// Скорость движения врага во время патрулирования (в единицах Unity в секунду).
        /// </summary>
        public float PatrolSpeed => patrolSpeed;

        [SerializeField] private float minPatrolTime = 3f; // Минимальное время патрулирования
        /// <summary>
        /// Минимальное время (в секундах), которое враг проводит, двигаясь к одной точке патрулирования, перед выбором новой.
        /// </summary>
        public float MinPatrolTime => minPatrolTime;

        [SerializeField] private float maxPatrolTime = 7f; // Максимальное время патрулирования
        /// <summary>
        /// Максимальное время (в секундах), которое враг проводит, двигаясь к одной точке патрулирования, перед выбором новой.
        /// </summary>
        public float MaxPatrolTime => maxPatrolTime;

        [SerializeField] private float smoothTurnSpeed = 0.3f; // Скорость поворота
        /// <summary>
        /// Скорость плавного поворота к цели (значение от 0 до 1, где 1 — мгновенный поворот).
        /// </summary>
        public float SmoothTurnSpeed => smoothTurnSpeed;
    }
}