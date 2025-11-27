using UnityEngine;

namespace Ship {
    /// <summary>
    /// Настройки улучшений корабля (здоровье, таран, оружие).
    /// Универсальны для игрока, врагов и торговцев (опционально).
    /// </summary>
    /// <remarks>
    /// Привязка в Unity Inspector:
    /// - Привязать к компоненту ShipHealth для дополнительного здоровья.
    /// - Привязать к компоненту ShipRamAttack для модификации тарана.
    /// - Привязать к компоненту ShipWeaponSystem для модификации оружия.
    /// - Может быть null, если улучшения не применяются.
    /// Настройка сцены:
    /// - Создайте ScriptableObject через меню (File > Create > ShipConfigs > ShipUpgradeConfig).
    /// </remarks>
    [CreateAssetMenu(fileName = "ShipUpgradeConfig", menuName = "ShipConfigs/ShipUpgradeConfig", order = 5)]
    public class ShipUpgradeConfig : ScriptableObject {
        [SerializeField] private int additionalHealth = 0; // Дополнительное здоровье
        /// <summary>
        /// Дополнительное здоровье корабля.
        /// Используется в ShipHealth для увеличения максимального здоровья.
        /// </summary>
        public int AdditionalHealth => additionalHealth;

        [SerializeField] private bool noseWeaponSlot = false; // Слот носового оружия
        /// <summary>
        /// Доступность слота носового оружия.
        /// Используется в ShipWeaponSystem для проверки возможности установки оружия.
        /// </summary>
        public bool NoseWeaponSlot => noseWeaponSlot;

        [SerializeField] private bool sideWeaponSlot = false; // Слот бортового оружия
        /// <summary>
        /// Доступность слота бортового оружия.
        /// Используется в ShipWeaponSystem для проверки возможности установки оружия.
        /// </summary>
        public bool SideWeaponSlot => sideWeaponSlot;

        [SerializeField] private bool sternWeaponSlot = false; // Слот кормового оружия
        /// <summary>
        /// Доступность слота кормового оружия.
        /// Используется в ShipWeaponSystem для проверки возможности установки оружия.
        /// </summary>
        public bool SternWeaponSlot => sternWeaponSlot;

        [SerializeField] private float ramDamageModifier = 1.0f; // Модификатор урона тарана
        /// <summary>
        /// Модификатор урона тарана (например, 1.2 для железного носа).
        /// Используется в ShipRamAttack для увеличения урона.
        /// </summary>
        public float RamDamageModifier => ramDamageModifier;

        [SerializeField] private float ramSelfDamageReduction = 1.0f; // Снижение собственного урона
        /// <summary>
        /// Снижение собственного урона при таране (например, 0.8 для железного носа).
        /// Используется в ShipHealth для уменьшения урона атакующему.
        /// </summary>
        public float RamSelfDamageReduction => ramSelfDamageReduction;

        [SerializeField] private float weaponDamageModifier = 1.0f; // Модификатор урона оружия
        /// <summary>
        /// Модификатор урона оружия.
        /// Используется в ShipWeaponSystem для увеличения урона.
        /// </summary>
        public float WeaponDamageModifier => weaponDamageModifier;

        [SerializeField] private float weaponRangeModifier = 1.0f; // Модификатор дальности оружия
        /// <summary>
        /// Модификатор дальности оружия.
        /// Используется в ShipWeaponSystem для увеличения дальности.
        /// </summary>
        public float WeaponRangeModifier => weaponRangeModifier;
    }
}