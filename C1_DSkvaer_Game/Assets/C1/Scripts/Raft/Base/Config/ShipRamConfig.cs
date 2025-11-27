using UnityEngine;

namespace Ship {
    /// <summary>
    /// Настройки таранной атаки корабля.
    /// Универсальны для игрока и врагов (для торговцев необязательно, если они не атакуют).
    /// </summary>
    /// <remarks>
    /// Привязка в Unity Inspector:
    /// - Привязать к компоненту ShipRamAttack (ранее ShipCollision) на объекте AttackArea.
    /// - Привязать к компоненту ShipHealth для обработки собственного урона при таране.
    /// - Привязать к NPCAI для тактики тарана (через EnemyTacticConfig).
    /// - Параметры DamageModifier и SelfDamageReduction модифицируются через ShipUpgradeConfig (например, при улучшении носа).
    /// Настройка сцены:
    /// - Создайте ScriptableObject через меню (File > Create > ShipConfigs > ShipRamConfig).
    /// </remarks>
    [CreateAssetMenu(fileName = "ShipRamConfig", menuName = "ShipConfigs/ShipRamConfig", order = 3)]
    public class ShipRamConfig : ScriptableObject {
        [SerializeField] private int baseDamage = 10; // Базовый урон
        /// <summary>
        /// Базовый урон при столкновении.
        /// Используется в ShipRamAttack для расчёта урона.
        /// </summary>
        public int BaseDamage => baseDamage;

        [SerializeField] private float damageBoost = 1.25f; // Множитель урона при высокой скорости
        /// <summary>
        /// Множитель урона при высокой скорости (если скорость >= MaxSpeed).
        /// Используется в ShipRamAttack для увеличения урона.
        /// </summary>
        public float DamageBoost => damageBoost;

        [SerializeField] private float baseKnockbackForce = 15f; // Сила отбрасывания
        /// <summary>
        /// Сила отбрасывания при столкновении.
        /// Используется в ShipRamAttack для отбрасывания цели.
        /// </summary>
        public float BaseKnockbackForce => baseKnockbackForce;

        [SerializeField] private float damageMultiplier = 1.0f; // Множитель урона для зоны попадания
        /// <summary>
        /// Множитель урона для зоны попадания (HitArea).
        /// Используется в ShipHitArea для модификации входящего урона.
        /// </summary>
        public float DamageMultiplier => damageMultiplier;

        [SerializeField] private float minRamSpeed = 2f; // Минимальная скорость тарана
        /// <summary>
        /// Минимальная скорость для активации тарана (в морских узлах).
        /// Используется в ShipRamAttack для проверки возможности тарана.
        /// </summary>
        public float MinRamSpeed => minRamSpeed;

        [SerializeField] private float ramKnockbackMultiplier = 1.5f; // Множитель отбрасывания
        /// <summary>
        /// Множитель силы отбрасывания при таране.
        /// Используется в ShipRamAttack для усиления отбрасывания.
        /// </summary>
        public float RamKnockbackMultiplier => ramKnockbackMultiplier;

        [SerializeField] private float selfDamagePercentage = 0.5f; // Процент собственного урона
        /// <summary>
        /// Процент урона, наносимого атакующему при таране (например, 0.5 = 50% от нанесённого урона).
        /// Используется в ShipHealth для расчёта собственного урона.
        /// </summary>
        public float SelfDamagePercentage => selfDamagePercentage;

        [SerializeField] private float damageModifier = 1.0f; // Модификатор урона
        /// <summary>
        /// Модификатор урона от улучшений (например, 1.2 для железного носа).
        /// Используется в ShipRamAttack, модифицируется через ShipUpgradeConfig.
        /// </summary>
        public float DamageModifier => damageModifier;

        [SerializeField] private float selfDamageReduction = 1.0f; // Снижение собственного урона
        /// <summary>
        /// Снижение собственного урона от улучшений (например, 0.8 для железного носа).
        /// Используется в ShipHealth, модифицируется через ShipUpgradeConfig.
        /// </summary>
        public float SelfDamageReduction => selfDamageReduction;
    }
}