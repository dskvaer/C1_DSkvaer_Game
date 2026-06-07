using UnityEngine;

namespace Ship {
    /// <summary>
    /// Настройки тарана корабля: урон, отбрасывание и самоповреждение.
    /// </summary>
    [CreateAssetMenu(fileName = "ShipRamConfig", menuName = "ShipConfigs/ShipRamConfig", order = 3)]
    public class ShipRamConfig : ScriptableObject {
        [Header("Урон тарана")]
        [InspectorLabel("Базовый урон")]
        [Tooltip("Основной урон, который наносится цели при успешном таране.")]
        [SerializeField] private int baseDamage = 10;
        public int BaseDamage => baseDamage;

        [InspectorLabel("Бонус урона на скорости")]
        [Tooltip("Множитель урона при таране на высокой скорости. 1.25 означает +25% урона.")]
        [SerializeField] private float damageBoost = 1.25f;
        public float DamageBoost => damageBoost;

        [InspectorLabel("Множитель зоны попадания")]
        [Tooltip("Дополнительный множитель урона для зоны корпуса, через которую прошел удар.")]
        [SerializeField] private float damageMultiplier = 1.0f;
        public float DamageMultiplier => damageMultiplier;

        [InspectorLabel("Минимальная скорость тарана")]
        [Tooltip("Скорость, ниже которой столкновение не считается тараном.")]
        [SerializeField] private float minRamSpeed = 2f;
        public float MinRamSpeed => minRamSpeed;

        [InspectorLabel("Модификатор урона")]
        [Tooltip("Итоговый множитель урона тарана. Может усиливаться улучшениями корабля.")]
        [SerializeField] private float damageModifier = 1.0f;
        public float DamageModifier => damageModifier;

        [Header("Отбрасывание")]
        [InspectorLabel("Базовая сила отбрасывания")]
        [Tooltip("Сила импульса, который таран передает цели.")]
        [SerializeField] private float baseKnockbackForce = 15f;
        public float BaseKnockbackForce => baseKnockbackForce;

        [InspectorLabel("Множитель отбрасывания")]
        [Tooltip("Дополнительный множитель силы отбрасывания при таране.")]
        [SerializeField] private float ramKnockbackMultiplier = 1.5f;
        public float RamKnockbackMultiplier => ramKnockbackMultiplier;

        [Header("Самоповреждение")]
        [InspectorLabel("Доля ответного урона")]
        [Tooltip("Какая часть нанесенного урона возвращается кораблю-таранщику. 0.5 означает 50%.")]
        [SerializeField] private float selfDamagePercentage = 0.5f;
        public float SelfDamagePercentage => selfDamagePercentage;

        [InspectorLabel("Снижение самоповреждения")]
        [Tooltip("Множитель снижения ответного урона. Значение ниже 1 уменьшает вред от собственного тарана.")]
        [SerializeField] private float selfDamageReduction = 1.0f;
        public float SelfDamageReduction => selfDamageReduction;
    }
}
