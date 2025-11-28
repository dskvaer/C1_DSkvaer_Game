// ====================================================================================================
// GunConfig.cs – ÓÏÐÎÙ¨ÍÍÛÉ (áåç ProjectileConfig)
// ====================================================================================================

using UnityEngine;

namespace Ship {
    [CreateAssetMenu(fileName = "GunConfig", menuName = "ShipConfigs/Gun/GunConfig")]
    public sealed class GunConfig : ScriptableObject {
        [Header("Fire")]
        [SerializeField, Min(0.01f)] private float fireRate = 2f;
        public float FireRate => fireRate;

        [Header("Accuracy")]
        [SerializeField, Range(0f, 100f)] private float accuracy = 80f;
        public float Accuracy => accuracy;

        [SerializeField] private float spreadAngle = 30f;
        public float SpreadAngle => spreadAngle;

        [SerializeField] private float minSpreadAngle = -30f;
        public float MinSpreadAngle => minSpreadAngle;

        private void OnValidate()
        {
            fireRate = Mathf.Max(0.01f, fireRate);
            accuracy = Mathf.Clamp(accuracy, 0f, 100f);
            spreadAngle = Mathf.Max(0f, spreadAngle);
            if (minSpreadAngle > spreadAngle) minSpreadAngle = -spreadAngle;
        }
    }
}