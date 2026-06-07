using UnityEngine;

namespace Ship {
    /// <summary>
    /// Настройки улучшений корабля: здоровье, слоты оружия и боевые множители.
    /// </summary>
    [CreateAssetMenu(fileName = "ShipUpgradeConfig", menuName = "ShipConfigs/ShipUpgradeConfig", order = 5)]
    public class ShipUpgradeConfig : ScriptableObject {
        [Header("Корпус")]
        [InspectorLabel("Дополнительное здоровье")]
        [Tooltip("Сколько прочности добавляется к базовому здоровью корабля.")]
        [SerializeField] private int additionalHealth = 0;
        public int AdditionalHealth => additionalHealth;

        [Header("Слоты оружия")]
        [InspectorLabel("Носовой слот")]
        [Tooltip("Разрешает установить оружие в носовой слот корабля.")]
        [SerializeField] private bool noseWeaponSlot = false;
        public bool NoseWeaponSlot => noseWeaponSlot;

        [InspectorLabel("Бортовой слот")]
        [Tooltip("Разрешает установить дополнительное оружие на бортах корабля.")]
        [SerializeField] private bool sideWeaponSlot = false;
        public bool SideWeaponSlot => sideWeaponSlot;

        [InspectorLabel("Кормовой слот")]
        [Tooltip("Разрешает установить оружие в кормовой слот корабля.")]
        [SerializeField] private bool sternWeaponSlot = false;
        public bool SternWeaponSlot => sternWeaponSlot;

        [Header("Таран")]
        [InspectorLabel("Множитель урона тарана")]
        [Tooltip("Во сколько раз улучшение меняет урон тарана. 1 оставляет урон без изменений.")]
        [SerializeField] private float ramDamageModifier = 1.0f;
        public float RamDamageModifier => ramDamageModifier;

        [InspectorLabel("Снижение урона себе")]
        [Tooltip("Множитель самоповреждения при таране. Значение ниже 1 уменьшает урон себе.")]
        [SerializeField] private float ramSelfDamageReduction = 1.0f;
        public float RamSelfDamageReduction => ramSelfDamageReduction;

        [Header("Оружие")]
        [InspectorLabel("Множитель урона оружия")]
        [Tooltip("Во сколько раз улучшение меняет урон пушек и других орудий.")]
        [SerializeField] private float weaponDamageModifier = 1.0f;
        public float WeaponDamageModifier => weaponDamageModifier;

        [InspectorLabel("Множитель дальности оружия")]
        [Tooltip("Во сколько раз улучшение меняет дальность стрельбы.")]
        [SerializeField] private float weaponRangeModifier = 1.0f;
        public float WeaponRangeModifier => weaponRangeModifier;
    }
}
