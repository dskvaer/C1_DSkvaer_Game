using UnityEngine;

namespace Ship {
    [RequireComponent(typeof(Collider2D))]
    public class ShipRamAttack : MonoBehaviour {
        [Header("Таран")]
        [InspectorLabel("Конфиг тарана")]
        [Tooltip("Настройки урона, минимальной скорости, отбрасывания и самоповреждения при таране.")]
        [SerializeField] private ShipRamConfig ramConfig;

        [InspectorLabel("Конфиг улучшений")]
        [Tooltip("Дополнительные модификаторы тарана от улучшений корабля. Можно оставить пустым.")]
        [SerializeField] private ShipUpgradeConfig upgradeConfig;

        private Rigidbody2D rb;

        private void Awake()
        {
            if (ramConfig == null) {
                Debug.LogError($"ShipRamConfig is not assigned for {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"}).", this);
                enabled = false;
                return;
            }

            rb = GetComponentInParent<Rigidbody2D>();
            if (rb == null) {
                Debug.LogError($"Rigidbody2D was not found for {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"}).", this);
                enabled = false;
                return;
            }
        }

        public float CalculateDamage()
        {
            if (rb == null || ramConfig == null) {
                return 0f;
            }

            float currentSpeed = rb.linearVelocity.magnitude;
            return ramConfig.BaseDamage
                * (currentSpeed >= ramConfig.MinRamSpeed ? ramConfig.DamageBoost : 1f)
                * ramConfig.DamageMultiplier
                * (upgradeConfig?.RamDamageModifier ?? 1f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("HitArea")) {
                return;
            }

            int finalDamage = Mathf.RoundToInt(CalculateDamage());
            if (finalDamage <= 0) {
                return;
            }

            DestructibleHitZone destructibleZone = other.GetComponent<DestructibleHitZone>();
            if (destructibleZone != null && !destructibleZone.IsDestroyed) {
                destructibleZone.ApplyDamage(finalDamage, isRam: true);
                ApplyKnockback(other);
                ;
                return;
            }

            BanditCoreHitZone coreHitZone = other.GetComponent<BanditCoreHitZone>();
            if (coreHitZone != null && !coreHitZone.IsDestroyed) {
                int coreDamage = Mathf.RoundToInt(finalDamage * coreHitZone.DamageMultiplier);
                coreHitZone.ApplyDamage(coreDamage, isRam: true);
                ApplyKnockback(other);
                ;
                return;
            }

            IShipHealth targetHealth = other.GetComponentInParent<IShipHealth>();
            if (targetHealth == null) {
                ;
                return;
            }

            targetHealth.TakeShipDamage(finalDamage, isRam: true);
            ApplyKnockback(other);
            ;
        }

        private void ApplyKnockback(Collider2D other)
        {
            IShipDamageable targetDamageable = other.GetComponentInParent<IShipDamageable>();
            if (targetDamageable == null || ramConfig == null) {
                return;
            }

            Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
            float knockbackForce = ramConfig.BaseKnockbackForce * ramConfig.RamKnockbackMultiplier;
            targetDamageable.ApplyShipKnockback(knockbackDir * knockbackForce);
        }
    }
}
