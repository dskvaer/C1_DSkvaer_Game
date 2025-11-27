using UnityEngine;

namespace Ship {
    /// <summary>
    /// Настройки для тактики тарана. Хранит параметры, определяющие поведение врага при атаке игрока с тараном и последующим отходом.
    /// </summary>
    /// <remarks>
    /// Привязка в Unity Inspector:
    /// - Создать через Assets > Create > ShipConfigs > TacticConfigs > RamAttackTacticConfig.
    /// - AttackSpeed: Скорость движения при приближении и таране (например, 1.0).
    /// - RamDistance: Дистанция для начала тарана (например, 4).
    /// - ContactDistance: Дистанция для нанесения урона (например, 0.8).
    /// - RetreatTime: Время отхода после тарана (например, 1.2).
    /// - MinRamSpeed: Минимальная скорость для урона (например, 2).
    /// - SmoothTurnSpeed: Скорость поворота (например, 0.3).
    /// - DamageCooldown: Кулдаун урона (например, 1).
    /// - HealthThreshold: Порог здоровья для выполнения тактики (например, 0.2).
    /// Настройка сцены:
    /// - Привязать к компоненту RamAttackTactic на объекте Enemy_Ship.
    /// </remarks>
    [CreateAssetMenu(fileName = "RamAttackTacticConfig", menuName = "ShipConfigs/TacticConfigs/RamAttackTacticConfig", order = 9)]
    public class RamAttackTacticConfig : ScriptableObject {
        [SerializeField] private float attackSpeed = 1.0f; // Скорость атаки
        /// <summary>
        /// Скорость движения врага при приближении к игроку и во время тарана (в единицах Unity в секунду).
        /// </summary>
        public float AttackSpeed => attackSpeed;

        [SerializeField] private float ramDistance = 4f; // Дистанция тарана
        /// <summary>
        /// Дистанция (в единицах Unity), на которой враг переходит от приближения к тарану.
        /// </summary>
        public float RamDistance => ramDistance;

        [SerializeField] private float contactDistance = 0.8f; // Дистанция контакта
        /// <summary>
        /// Дистанция (в единицах Unity), на которой враг наносит урон при таране.
        /// </summary>
        public float ContactDistance => contactDistance;

        [SerializeField] private float retreatTime = 1.2f; // Время отхода
        /// <summary>
        /// Время (в секундах), в течение которого враг отходит после тарана.
        /// </summary>
        public float RetreatTime => retreatTime;

        [SerializeField] private float minRamSpeed = 2f; // Минимальная скорость тарана
        /// <summary>
        /// Минимальная скорость (в единицах Unity в секунду), необходимая для нанесения урона при таране.
        /// </summary>
        public float MinRamSpeed => minRamSpeed;

        [SerializeField] private float smoothTurnSpeed = 0.3f; // Скорость поворота
        /// <summary>
        /// Скорость плавного поворота к цели (значение от 0 до 1, где 1 — мгновенный поворот).
        /// </summary>
        public float SmoothTurnSpeed => smoothTurnSpeed;

        [SerializeField] private float damageCooldown = 1f; // Кулдаун урона
        /// <summary>
        /// Время (в секундах) между нанесениями урона при таране.
        /// </summary>
        public float DamageCooldown => damageCooldown;

        [SerializeField] private float healthThreshold = 0.2f; // Порог здоровья
        /// <summary>
        /// Порог здоровья (в процентах от максимального), ниже которого тактика не выполняется.
        /// </summary>
        public float HealthThreshold => healthThreshold;
    }
}