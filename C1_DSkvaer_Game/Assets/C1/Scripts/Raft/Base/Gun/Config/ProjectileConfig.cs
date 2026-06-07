using UnityEngine;

namespace Ship {
    /// <summary>
    /// Настройки снаряда: урон, дальность, скорость и AOE.
    /// </summary>
    [CreateAssetMenu(fileName = "ProjectileConfig", menuName = "ShipConfigs/ProjectileConfig", order = 1)]
    public sealed class ProjectileConfig : ScriptableObject {
        [Header("Параметры снаряда")]
        [InspectorLabel("Урон")]
        [Tooltip("Базовый урон за прямое попадание.")]
        [SerializeField, Min(0f)]
        private float damage = 10f;
        public float Damage => damage;

        [InspectorLabel("Дальность")]
        [Tooltip("Максимальная дальность полета снаряда в единицах Unity.")]
        [SerializeField, Min(0f)]
        private float range = 50f;
        public float Range => range;

        [InspectorLabel("Скорость снаряда")]
        [Tooltip("Скорость движения снаряда в единицах Unity в секунду.")]
        [SerializeField, Min(0.1f)]
        private float projectileSpeed = 20f;
        public float ProjectileSpeed => projectileSpeed;

        [InspectorLabel("Радиус AOE")]
        [Tooltip("Радиус урона по площади. 0 означает, что AOE-урона нет.")]
        [SerializeField, Min(0f)]
        private float areaOfEffectRadius = 0f;
        public float AreaOfEffectRadius => areaOfEffectRadius;

        private void OnValidate()
        {
            if (damage < 0f) damage = 0f;
            if (range < 0f) range = 0f;
            if (projectileSpeed < 0.1f) projectileSpeed = 0.1f;
            if (areaOfEffectRadius < 0f) areaOfEffectRadius = 0f;
        }
    }
}
