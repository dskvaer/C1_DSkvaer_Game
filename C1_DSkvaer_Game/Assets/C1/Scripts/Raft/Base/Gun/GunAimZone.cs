// ====================================================================================================
// GunAimZone.cs – ЗОНА ПРИЦЕЛИВАНИЯ ПУШКИ (сектор)
// ====================================================================================================

using UnityEngine;

namespace Ship {
    /// <summary>
    /// Зона прицеливания пушки. Определяет, видит ли пушка цель.
    /// </summary>
    /// <remarks>
    /// **Inspector:**
    /// • AimAngle: Угол сектора (например, 60°)
    /// • AimRange: Дальность прицела (из ProjectileConfig.Range)
    /// • TargetTag: "Player"
    /// 
    /// **Настройка:**
    /// • Добавь на дочерний объект пушки (например, Gun_Arbalest/AimZone)
    /// • CircleCollider2D: isTrigger = true
    /// • Этот скрипт на тот же объект
    /// </remarks>
    [RequireComponent(typeof(Collider2D))]
    public class GunAimZone : MonoBehaviour {
        [SerializeField, Range(10f, 180f)] private float aimAngle = 60f;
        [SerializeField] private string targetTag = "Player";

        private GunWeaponSystem weaponSystem;
        private ProjectileConfig projConfig;
        private Transform target;

        private void Awake()
        {
            weaponSystem = GetComponentInParent<GunWeaponSystem>();
            if (weaponSystem != null)
            {
                projConfig = weaponSystem.GetProjectileConfig();
            }

            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
            if (projConfig != null && col is CircleCollider2D circle)
            {
                circle.radius = projConfig.Range;
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.CompareTag(targetTag)) return;
            if (target != null) return;

            Vector2 toTarget = other.transform.position - transform.position;
            float angle = Vector2.Angle(transform.up, toTarget.normalized);
            if (angle <= aimAngle * 0.5f)
            {
                target = other.transform;
                Debug.Log($"[GunAimZone] ЦЕЛЬ В ПРИЦЕЛЕ: {other.name} → {gameObject.name}");
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.transform == target)
            {
                target = null;
                Debug.Log($"[GunAimZone] ЦЕЛЬ ПОТЕРЯНА: {gameObject.name}");
            }
        }

        public bool CanSeeTarget() => target != null;
        public Transform GetTarget() => target;
        public float GetAimRange() => projConfig?.Range ?? 10f;
    }
}