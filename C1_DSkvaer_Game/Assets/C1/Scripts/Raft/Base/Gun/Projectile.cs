// ====================================================================================================
// Projectile.cs – ИСПРАВЛЕНО: AOE + ПРЯМОЙ УРОН + GetProjectileConfig()
// ====================================================================================================

using UnityEngine;

namespace Ship {
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class Projectile : MonoBehaviour {
        // --------------------------------------------------------------------- Inspector
        [SerializeField, Tooltip("ОБЯЗАТЕЛЬНО: ProjectileConfig из Project!")]
        public ProjectileConfig config;

        [SerializeField] private ProjectileEffects projectileEffects;

        // --------------------------------------------------------------------- Private
        private Rigidbody2D rb;
        private float lifetime;

        // --------------------------------------------------------------------- Unity: Awake
        private void Awake()
        {
            Debug.Log($"[Projectile] ШАГ 1: Awake {name} начат", this);

            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogError($"[Projectile] Rigidbody2D отсутствует у {name}!", this);
                Destroy(gameObject);
                return;
            }

            if (config == null)
            {
                Debug.LogError($"[Projectile] config == null у {name}! УБЕДИТЕСЬ, ЧТО НАЗНАЧЕН В ПРЕФАБЕ!", this);
                Destroy(gameObject);
                return;
            }

            var col = GetComponent<Collider2D>();
            if (col == null)
            {
                Debug.LogError($"[Projectile] Collider2D отсутствует у {name}!", this);
                Destroy(gameObject);
                return;
            }

            col.isTrigger = true;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;

            lifetime = config.Range / Mathf.Max(0.0001f, config.ProjectileSpeed);
            projectileEffects?.PlayTrailEffect();

            Debug.Log($"[Projectile] {name} запущен: Damage={config.Damage}, AOE={config.AreaOfEffectRadius}, Range={config.Range}, Speed={config.ProjectileSpeed}", this);
        }

        // --------------------------------------------------------------------- Unity: FixedUpdate
        private void FixedUpdate()
        {
            if (rb == null || config == null) return;

            rb.MovePosition(rb.position + (Vector2)transform.up * config.ProjectileSpeed * Time.fixedDeltaTime);
            lifetime -= Time.fixedDeltaTime;

            if (lifetime <= 0f)
            {
                projectileEffects?.PlayMissEffect(transform.position);
                Destroy(gameObject);
            }
        }

        // --------------------------------------------------------------------- Unity: OnTriggerEnter2D
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null || other.CompareTag("Projectile")) return;

            if (other.CompareTag("HitArea"))
            {
                var hitArea = other.GetComponent<ShipHitArea>();
                if (hitArea == null) return;

                var health = hitArea.GetComponentInParent<IShipHealth>();
                if (health == null || health.IsDead) return;

                float finalDamage = config.Damage * hitArea.DamageMultiplier;

                // ПРЯМОЙ УРОН
                health.TakeShipDamage(Mathf.RoundToInt(finalDamage));
                Debug.Log($"[Projectile] ПРЯМОЙ УРОН: {Mathf.RoundToInt(finalDamage)} → {other.name}", this);

                // AOE
                if (config.AreaOfEffectRadius > 0f)
                {
                    var hits = Physics2D.OverlapCircleAll(
                        transform.position,
                        config.AreaOfEffectRadius,
                        LayerMask.GetMask("HitArea"));

                    foreach (var h in hits)
                    {
                        if (h == other) continue;
                        var hh = h.GetComponentInParent<IShipHealth>();
                        if (hh != null && !hh.IsDead)
                        {
                            hh.TakeShipDamage(Mathf.RoundToInt(finalDamage));
                            Debug.Log($"[Projectile] AOE УРОН: {h.name} → {Mathf.RoundToInt(finalDamage)}", this);
                        }
                    }
                }

                projectileEffects?.PlayHitEffect(transform.position);
                Destroy(gameObject);
                return;
            }

            // Столкновение с твёрдым объектом
            if (!other.isTrigger)
            {
                projectileEffects?.PlayHitEffect(transform.position);
                Destroy(gameObject);
            }
        }

        // --------------------------------------------------------------------- Public API: GetProjectileConfig
        /// <summary>
        /// Возвращает ProjectileConfig (для GunWeaponSystem).
        /// </summary>
        /// <returns>config или null (если не назначен)</returns>
        public ProjectileConfig GetProjectileConfig()
        {
            Debug.Log($"[Projectile] GetProjectileConfig() → {(config != null ? config.name : "null")}", this);
            return config;
        }
    }
}