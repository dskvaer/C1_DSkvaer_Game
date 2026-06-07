using UnityEngine;

namespace Ship {
    public class SuicideExplosionTactic : MonoBehaviour, IEnemyTactic {
        [Header("Самоподрыв")]
        [InspectorLabel("Скорость сближения")]
        [Tooltip("Скорость, с которой взрывной юнит идет к цели.")]
        [SerializeField, Min(0.1f)] private float approachSpeed = 1.8f;
        [InspectorLabel("Дистанция подрыва")]
        [Tooltip("На каком расстоянии от цели юнит взрывается.")]
        [SerializeField, Min(0.1f)] private float detonationDistance = 1.6f;
        [InspectorLabel("Радиус взрыва")]
        [Tooltip("Радиус поражения взрыва.")]
        [SerializeField, Min(0.1f)] private float explosionRadius = 3.5f;
        [InspectorLabel("Урон взрыва")]
        [Tooltip("Сколько урона получают корабли в радиусе взрыва.")]
        [SerializeField, Min(1)] private int explosionDamage = 80;
        [InspectorLabel("Сила отбрасывания")]
        [Tooltip("Сила импульса, который взрыв применяет к поврежденным кораблям.")]
        [SerializeField, Min(0f)] private float knockbackForce = 18f;
        [InspectorLabel("Слои урона")]
        [Tooltip("Какие слои получают урон от взрыва.")]
        [SerializeField] private LayerMask damageLayers = -1;
        [InspectorLabel("Префаб взрыва")]
        [Tooltip("Визуальный эффект, который создается в точке подрыва.")]
        [SerializeField] private GameObject explosionPrefab;
        [InspectorLabel("Взрыв при критическом здоровье")]
        [Tooltip("Если включено, юнит взрывается при падении здоровья ниже порога.")]
        [SerializeField] private bool explodeWhenCriticallyDamaged = true;
        [InspectorLabel("Порог критического здоровья")]
        [Tooltip("Процент здоровья, при котором срабатывает самоподрыв.")]
        [SerializeField, Range(0f, 1f)] private float criticalHealthThreshold = 0.15f;

        private NPCAI npcAI;
        private bool exploded;

        private void Awake()
        {
            npcAI = GetComponentInParent<NPCAI>();
        }

        public bool CanExecute(EnemyAIContext context)
        {
            return !exploded && npcAI != null && context?.Player != null && context.Rigidbody != null;
        }

        public void Execute(EnemyAIContext context, float deltaTime)
        {
            Vector2 targetPosition = BanditTacticUtility.PlayerPosition(context);
            npcAI.MoveToSmooth(context, targetPosition, approachSpeed, 0.65f);

            float distance = Vector2.Distance(context.Rigidbody.position, targetPosition);
            bool lowHealth = explodeWhenCriticallyDamaged
                && context.ShipHealth != null
                && context.ShipHealth.GetCurrentShipHealth() <= context.ShipHealth.GetMaxShipHealth() * criticalHealthThreshold;

            if (distance <= detonationDistance || lowHealth) {
                Explode(context);
            }
        }

        private void Explode(EnemyAIContext context)
        {
            if (exploded) {
                return;
            }

            exploded = true;
            Vector2 center = context.Rigidbody.position;

            if (explosionPrefab != null) {
                Instantiate(explosionPrefab, center, Quaternion.identity);
            }

            Collider2D[] hits = Physics2D.OverlapCircleAll(center, explosionRadius, damageLayers);
            foreach (Collider2D hit in hits) {
                if (hit == null || hit.transform.IsChildOf(transform.root)) {
                    continue;
                }

                IShipHealth health = hit.GetComponentInParent<IShipHealth>();
                if (health != null) {
                    health.TakeShipDamage(explosionDamage, isRam: false);
                }

                IShipDamageable damageable = hit.GetComponentInParent<IShipDamageable>();
                if (damageable != null) {
                    Vector2 direction = ((Vector2)hit.transform.position - center).normalized;
                    damageable.ApplyShipKnockback(direction * knockbackForce);
                }
            }

            context.ShipHealth?.SetShipHealth(0);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.35f, 0.05f, 0.85f);
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
