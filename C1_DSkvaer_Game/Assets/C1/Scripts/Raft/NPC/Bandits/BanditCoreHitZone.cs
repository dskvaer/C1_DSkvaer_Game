using UnityEngine;

namespace Ship {
    [RequireComponent(typeof(Collider2D))]
    public class BanditCoreHitZone : MonoBehaviour {
        [SerializeField, Min(0f)] private float damageMultiplier = 1f;

        private BanditEnemy owner;
        private IShipHealth shipHealth;
        private Collider2D zoneCollider;

        public float DamageMultiplier => damageMultiplier;
        public bool IsDestroyed => shipHealth == null || shipHealth.IsDead;

        private void Awake()
        {
            zoneCollider = GetComponent<Collider2D>();
            zoneCollider.isTrigger = true;
            shipHealth = GetComponentInParent<IShipHealth>();
            TrySetTag("HitArea");
            TrySetLayer("HitArea");
        }

        public void Configure(BanditEnemy zoneOwner, IShipHealth health, float multiplier)
        {
            owner = zoneOwner;
            shipHealth = health;
            damageMultiplier = Mathf.Max(0f, multiplier);

            zoneCollider = GetComponent<Collider2D>();
            if (zoneCollider != null) {
                zoneCollider.isTrigger = true;
                zoneCollider.enabled = true;
            }

            TrySetTag("HitArea");
            TrySetLayer("HitArea");
        }

        public void ApplyDamage(int amount, bool isRam)
        {
            if (owner != null && owner.HasActiveArmor) {
                return;
            }

            if (shipHealth == null || shipHealth.IsDead || amount <= 0) {
                return;
            }

            shipHealth.TakeShipDamage(amount, isRam);
        }

        private void TrySetTag(string tagName)
        {
            try {
                gameObject.tag = tagName;
            }
            catch (UnityException) {
                ;
            }
        }

        private void TrySetLayer(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            gameObject.layer = layer >= 0 ? layer : 0;
        }
    }
}
