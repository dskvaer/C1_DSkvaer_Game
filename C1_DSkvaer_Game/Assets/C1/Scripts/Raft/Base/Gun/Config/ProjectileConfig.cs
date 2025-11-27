// ====================================================================================================
// ProjectileConfig.cs
// ====================================================================================================

using UnityEngine;

namespace Ship {
    /// <summary>
    /// Параметры снаряда (урон, дальность, скорость, AOE).
    /// </summary>
    [CreateAssetMenu(fileName = "ProjectileConfig", menuName = "ShipConfigs/ProjectileConfig", order = 1)]
    public sealed class ProjectileConfig : ScriptableObject {
        [SerializeField, Min(0f), Tooltip("Урон за попадание")]
        private float damage = 10f;
        public float Damage => damage;

        [SerializeField, Min(0f), Tooltip("Дальность полёта")]
        private float range = 50f;
        public float Range => range;

        [SerializeField, Min(0.1f), Tooltip("Скорость снаряда (ед/сек)")]
        private float projectileSpeed = 20f;
        public float ProjectileSpeed => projectileSpeed;

        [SerializeField, Min(0f), Tooltip("Радиус AOE-урона (0 = нет)")]
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