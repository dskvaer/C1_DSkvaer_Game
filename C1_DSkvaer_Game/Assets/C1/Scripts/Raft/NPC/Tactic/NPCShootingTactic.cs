using System.Collections.Generic;
using UnityEngine;

namespace Ship {
    /// <summary>
    /// Тактика стрельбы с использованием ScriptableObject настроек (NPCShootingConfig).
    /// Реализует гибридное наведение (Упреждение + Прямой огонь).
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class NPCShootingTactic : MonoBehaviour, IEnemyTactic {
        [Header("Настройки")]
        [SerializeField] private NPCShootingConfig config; // Ссылка на ScriptableObject

        [Header("Компоненты")]
        [SerializeField] private CircleCollider2D detectionCollider;
        [SerializeField] private LayerMask targetLayer = -1;
        [SerializeField] private GameObject[] allGunObjects;

        // Приватные поля
        private ShipMovement shipMovement;
        private ShipID shipID;

        // Кэшированные данные цели
        private Transform target;
        private Rigidbody2D targetRb;
        private bool hasAcquiredTarget = false;
        private float targetLostTime = 0f;
        private const float TARGET_LOST_THRESHOLD = 1.0f;

        private List<GunWeaponSystem> activeGuns = new List<GunWeaponSystem>();
        private float calculatedAvgProjectileSpeed = 20f;
        private readonly Collider2D[] overlapResults = new Collider2D[4];

        private float lastLogTime;
        private const float LOG_INTERVAL = 1f;

        //=====================================================================
        // Unity: Awake / Start
        //=====================================================================

        private void Awake()
        {
            shipID = GetComponentInParent<ShipID>();
            shipMovement = GetComponentInParent<ShipMovement>();
            detectionCollider = GetComponentInChildren<CircleCollider2D>();

            if (config == null)
            {
                Debug.LogError($"[NPCShoot] НЕТ КОНФИГА NPCShootingConfig у {name}!");
                enabled = false; return;
            }

            if (!shipID || !shipMovement || !detectionCollider)
            {
                Debug.LogError($"[NPCShoot] Ошибка компонентов у {name}");
                enabled = false; return;
            }
            detectionCollider.isTrigger = true;
        }

        private void Start()
        {
            CollectGunsAndCalculateSpeed();
        }

        private void CollectGunsAndCalculateSpeed()
        {
            activeGuns.Clear();
            float totalSpeed = 0f;
            int count = 0;

            if (allGunObjects != null)
            {
                foreach (var obj in allGunObjects)
                {
                    if (obj == null) continue;
                    var gun = obj.GetComponent<GunWeaponSystem>();
                    if (gun != null)
                    {
                        activeGuns.Add(gun);
                        var c = gun.GetProjectileConfig();
                        if (c != null)
                        {
                            totalSpeed += c.ProjectileSpeed;
                            count++;
                        }
                    }
                }
            }

            if (count > 0)
            {
                calculatedAvgProjectileSpeed = totalSpeed / count;
                Debug.Log($"[NPCShoot] Найдено {count} пушек. Средняя скорость снаряда: {calculatedAvgProjectileSpeed:F1}");
            }
            else
            {
                calculatedAvgProjectileSpeed = 20f;
                Debug.LogWarning("[NPCShoot] Пушки не найдены или нет конфигов. Используем стандартную скорость: 20");
            }
        }

        //=====================================================================
        // IEnemyTactic Implementation
        //=====================================================================

        public bool CanExecute(EnemyAIContext context)
        {
            if (context?.Player == null)
            {
                if (hasAcquiredTarget) ResetTarget();
                return false;
            }

            target = context.Player;
            if (targetRb == null) targetRb = target.GetComponent<Rigidbody2D>();

            bool inZone = CheckTargetInZone();
            if (inZone)
            {
                hasAcquiredTarget = true;
                targetLostTime = 0f;
            }

            return hasAcquiredTarget || inZone;
        }

        public void Execute(EnemyAIContext context, float deltaTime)
        {
            if (target == null) { ResetTarget(); return; }

            UpdateTargetStatus(deltaTime);
            if (!hasAcquiredTarget) return;

            // Используем linearVelocity (Unity 6 / 2023.3+)
            Vector2 targetVelocity = (targetRb != null) ? targetRb.linearVelocity : Vector2.zero;

            // 1. Вращение корпуса (на упреждение)
            Vector2 aimPoint = CalculateInterceptPoint(transform.position, target.position, targetVelocity, calculatedAvgProjectileSpeed);
            RotateShipBody(aimPoint, deltaTime);

            // 2. Стрельба
            ProcessIndependentGunfire(targetVelocity);
        }

        //=====================================================================
        // Логика обнаружения
        //=====================================================================

        private void UpdateTargetStatus(float deltaTime)
        {
            if (CheckTargetInZone())
            {
                targetLostTime = 0f;
                hasAcquiredTarget = true;
            }
            else
            {
                targetLostTime += deltaTime;
                if (targetLostTime >= TARGET_LOST_THRESHOLD) ResetTarget();
            }
        }

        private bool CheckTargetInZone()
        {
            if (!detectionCollider || !target) return false;
            Vector2 center = (Vector2)transform.position + detectionCollider.offset;
            int count = Physics2D.OverlapCircle(center, detectionCollider.radius, new ContactFilter2D { useLayerMask = true, layerMask = targetLayer }, overlapResults);

            for (int i = 0; i < count; i++)
                if (overlapResults[i] != null && overlapResults[i].transform == target) return true;

            return false;
        }

        //=====================================================================
        // Логика Вращения (Корпус)
        //=====================================================================

        private void RotateShipBody(Vector2 aimPosition, float deltaTime)
        {
            Vector2 directionToAim = aimPosition - (Vector2)transform.position;
            float targetAngle = Mathf.Atan2(directionToAim.y, directionToAim.x) * Mathf.Rad2Deg - 90f;
            float currentAngle = transform.eulerAngles.z;
            float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);
            float absDiff = Mathf.Abs(angleDiff);

            if (absDiff < config.RotationAimTolerance)
            {
                shipMovement.ShipRotate(Vector2.zero, 0f);
                return;
            }

            // Используем настройки из КОНФИГА
            float speedRatio = Mathf.InverseLerp(config.FastTurnAngle, 0f, absDiff);
            float currentTurnSpeed = Mathf.Lerp(config.MinTurnSpeed, config.MaxTurnSpeed, speedRatio);

            float maxInput = currentTurnSpeed * deltaTime / 90f;
            float input = Mathf.Clamp(angleDiff / 90f, -maxInput, maxInput);

            shipMovement.ShipRotate(new Vector2(input, 0f), 1f);
        }

        //=====================================================================
        // Логика "Умных Турелей" (Independent Gunfire)
        //=====================================================================

        private void ProcessIndependentGunfire(Vector2 targetVelocity)
        {
            foreach (var gun in activeGuns)
            {
                if (gun == null || !gun.CanFire()) continue;
                var c = gun.GetProjectileConfig();
                if (c == null) continue;

                Vector2 gunPos = gun.transform.position;
                Vector2 gunForward = gun.transform.up;
                bool shouldFire = false;

                // 1. Упреждение
                Vector2 intercept = CalculateInterceptPoint(gunPos, target.position, targetVelocity, c.ProjectileSpeed);
                if (Vector2.Distance(gunPos, intercept) <= c.Range)
                {
                    // Используем порог из конфига
                    if (Vector2.Angle(gunForward, (intercept - gunPos).normalized) <= config.FiringAngleThreshold)
                        shouldFire = true;
                }

                // 2. Прямой огонь (если упреждение не сработало)
                if (!shouldFire && Vector2.Distance(gunPos, target.position) <= c.Range)
                {
                    if (Vector2.Angle(gunForward, ((Vector2)target.position - gunPos).normalized) <= config.FiringAngleThreshold)
                        shouldFire = true;
                }

                if (shouldFire) gun.Fire();
            }
        }

        private Vector2 CalculateInterceptPoint(Vector2 shooterPos, Vector2 targetPos, Vector2 targetVel, float projectileSpeed)
        {
            Vector2 toTarget = targetPos - shooterPos;
            float time = toTarget.magnitude / Mathf.Max(0.001f, projectileSpeed);
            return targetPos + (targetVel * time);
        }

        private void ResetTarget()
        {
            hasAcquiredTarget = false;
            target = null;
            targetRb = null;
            targetLostTime = 0f;
            shipMovement?.ShipRotate(Vector2.zero, 0f);
        }

        private void OnDrawGizmosSelected()
        {
            if (detectionCollider != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere((Vector2)transform.position + detectionCollider.offset, detectionCollider.radius);
            }

            if (target != null && activeGuns.Count > 0)
            {
                var gun = activeGuns[0];
                if (gun != null && gun.GetProjectileConfig() != null)
                {
                    Vector2 targetVel = (targetRb != null) ? targetRb.linearVelocity : Vector2.zero;

                    Vector2 aim = CalculateInterceptPoint(gun.transform.position, target.position, targetVel, gun.GetProjectileConfig().ProjectileSpeed);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(gun.transform.position, aim);
                    Gizmos.DrawSphere(aim, 0.5f);

                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(gun.transform.position, target.position);
                }
            }
        }
    }
}