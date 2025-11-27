using UnityEngine;

namespace Ship
{
    /// <summary>
    /// Тактика стрельбы NPC:
    /// • Одна круглая зона детекции (CircleCollider2D)
    /// • ГИБКАЯ СКОРОСТЬ ПОВОРОТА: от 10°/с до 1000°/с
    /// • Плавный переход: медленный → быстрый
    /// • Пресеты в инспекторе
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class NPCShootingTactic : MonoBehaviour, IEnemyTactic
    {
        //=====================================================================
        // Inspector — НАСТРОЙКИ ПОВОРОТА
        //=====================================================================

        [Header("ЗОНА ДЕТЕКЦИИ И СТРЕЛЬБЫ")]
        [SerializeField] private CircleCollider2D detectionCollider;
        [SerializeField] private LayerMask targetLayer = -1;

        [Header("Пушки")]
        [SerializeField] private GameObject[] leftGuns;
        [SerializeField] private GameObject[] rightGuns;

        [Header("ГИБКАЯ СКОРОСТЬ ПОВОРОТА")]
        [Tooltip("Минимальная скорость поворота (°/с). При угле > 90°")]
        [SerializeField, Range(10f, 500f)] private float minTurnSpeed = 60f;

        [Tooltip("Максимальная скорость поворота (°/с). При угле ≤ 90°")]
        [SerializeField, Range(100f, 1000f)] private float maxTurnSpeed = 360f;

        [Tooltip("Угол, при котором начинается максимальная скорость")]
        [SerializeField, Range(30f, 120f)] private float fastTurnAngle = 90f;

        [Tooltip("Точность прицеливания (остановка поворота)")]
        [SerializeField, Range(1f, 30f)] private float aimTolerance = 8f;

        [Tooltip("Частота пересчёта цели")]
        [SerializeField, Range(0.05f, 1f)] private float retargetInterval = 0.2f;

        //=====================================================================
        // Private
        //=====================================================================

        private ShipMovement shipMovement;
        private Transform target;
        private ShipID shipID;

        private bool hasAcquiredTarget = false;
        private float targetLostTime = 0f;
        private const float TARGET_LOST_THRESHOLD = 0.5f;

        private float targetAngle = 0f;
        private float retargetTimer = 0f;
        private bool useLeftSide = true;

        private int leftGunIndex = 0;
        private int rightGunIndex = 0;

        private float lastLogTime = 0f;
        private const float LOG_INTERVAL = 0.5f;

        private readonly Collider2D[] overlapResults = new Collider2D[4];

        //=====================================================================
        // Awake
        //=====================================================================

        private void Awake()
        {
            shipID = GetComponentInParent<ShipID>();
            shipMovement = GetComponentInParent<ShipMovement>();

            if (shipID == null || shipMovement == null)
            {
                LogError("ShipID или ShipMovement не найдены!");
                enabled = false;
                return;
            }

            detectionCollider = GetComponentInChildren<CircleCollider2D>();
            if (detectionCollider == null)
            {
                LogError("detectionCollider не найден! ДОБАВЬТЕ ДОЧЕРНИЙ CircleCollider2D (isTrigger=true)");
                enabled = false;
                return;
            }
            detectionCollider.isTrigger = true;

            if ((leftGuns == null || leftGuns.Length == 0) && (rightGuns == null || rightGuns.Length == 0))
            {
                LogError("Пушки не заданы!");
                enabled = false;
                return;
            }

            LogMessage($"Инициализация OK | Поворот: {minTurnSpeed}-{maxTurnSpeed}°/с | ID: {shipID.ID}");
        }

        //=====================================================================
        // IEnemyTactic
        //=====================================================================

        public bool CanExecute(EnemyAIContext context)
        {
            if (context == null || context.Player == null)
            {
                if (hasAcquiredTarget) ResetTarget();
                return false;
            }

            target = context.Player;

            if (target == null || target.gameObject == null)
            {
                LogMessage("ЦЕЛЬ УНИЧТОЖЕНА → СБРОС");
                ResetTarget();
                return false;
            }

            bool inZone = IsTargetInZone();

            if (inZone && !hasAcquiredTarget)
            {
                hasAcquiredTarget = true;
                targetLostTime = 0f;
                retargetTimer = 0f;
                LogMessage($"ОХОТА НАЧАТА | Дист: {Vector2.Distance(transform.position, target.position):F1}m");
            }

            return hasAcquiredTarget || inZone;
        }

        public void Execute(EnemyAIContext context, float deltaTime)
        {
            if (context == null || context.Player == null || target == null || target.gameObject == null)
            {
                ResetTarget();
                return;
            }

            UpdateTargetStatus(deltaTime);
            if (!hasAcquiredTarget) return;

            ContinuousTracking(deltaTime);
        }

        //=====================================================================
        // UpdateTargetStatus
        //=====================================================================

        private void UpdateTargetStatus(float deltaTime)
        {
            if (target == null || target.gameObject == null)
            {
                LogMessage("ЦЕЛЬ УНИЧТОЖЕНА");
                ResetTarget();
                return;
            }

            if (IsTargetInZone())
            {
                targetLostTime = 0f;
                hasAcquiredTarget = true;
            }
            else
            {
                targetLostTime += deltaTime;
                if (targetLostTime >= TARGET_LOST_THRESHOLD)
                {
                    LogMessage("ОХОТА ПРЕКРАЩЕНА");
                    ResetTarget();
                }
            }
        }

        //=====================================================================
        // IsTargetInZone
        //=====================================================================

        private bool IsTargetInZone()
        {
            if (detectionCollider == null || target == null) return false;

            Vector2 center = (Vector2)transform.position + detectionCollider.offset;
            float radius = detectionCollider.radius;

            var filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = targetLayer
            };

            int count = Physics2D.OverlapCircle(center, radius, filter, overlapResults);

            for (int i = 0; i < count; i++)
            {
                if (overlapResults[i] != null && overlapResults[i].transform == target)
                    return true;
            }

            return false;
        }

        //=====================================================================
        // ContinuousTracking
        //=====================================================================

        private void ContinuousTracking(float deltaTime)
        {
            if (target == null || target.gameObject == null)
            {
                ResetTarget();
                return;
            }

            retargetTimer += deltaTime;
            if (retargetTimer >= retargetInterval)
            {
                CalculateTargetAngle();
                retargetTimer = 0f;
            }

            RotateToTarget(deltaTime);
            TryFire();
        }

        //=====================================================================
        // CalculateTargetAngle
        //=====================================================================

        private void CalculateTargetAngle()
        {
            if (target == null || target.gameObject == null) return;

            Vector2 toTarget = (target.position - transform.position).normalized;
            targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg - 90f;
            targetAngle = NormalizeAngle(targetAngle);

            float angleFromNose = Mathf.DeltaAngle(transform.eulerAngles.z, targetAngle);
            useLeftSide = angleFromNose < 0;

            LogMessage($"НАЦЕЛИВАНИЕ | Борт: {(useLeftSide ? "ЛЕВЫЙ" : "ПРАВЫЙ")} | Угол: {targetAngle:F1}°");
        }

        //=====================================================================
        // RotateToTarget — ГИБКАЯ СКОРОСТЬ
        //=====================================================================

        private void RotateToTarget(float deltaTime)
        {
            if (target == null || target.gameObject == null)
            {
                shipMovement?.ShipRotate(Vector2.zero, 0f);
                return;
            }

            float currentAngle = transform.eulerAngles.z;
            float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);
            float absDiff = Mathf.Abs(angleDiff);

            // Остановка при прицеле
            if (absDiff < aimTolerance)
            {
                shipMovement.ShipRotate(Vector2.zero, 0f);
                return;
            }

            // ГИБКАЯ СКОРОСТЬ: от min до max
            float speedRatio = Mathf.InverseLerp(fastTurnAngle, 0f, absDiff); // 1 при малом угле
            float turnSpeed = Mathf.Lerp(minTurnSpeed, maxTurnSpeed, speedRatio);

            // Input для ShipRotate (-1..1)
            float maxInputPerFrame = turnSpeed * deltaTime / 90f;
            float rotationInput = Mathf.Clamp(angleDiff / 90f, -maxInputPerFrame, maxInputPerFrame);

            shipMovement.ShipRotate(new Vector2(rotationInput, 0f), 1f);

            // Лог
            if (Time.time - lastLogTime > LOG_INTERVAL)
            {
                string dir = angleDiff > 0 ? "ВПРАВО" : "ВЛЕВО";
                LogMessage($"{dir} | Δ={absDiff:F1}° | Скорость={turnSpeed:F0}°/с | Input={rotationInput:F3}");
                lastLogTime = Time.time;
            }
        }

        //=====================================================================
        // TryFire
        //=====================================================================

        private void TryFire()
        {
            if (target == null || target.gameObject == null) return;
            if (!IsTargetInZone()) return;

            if (useLeftSide && leftGuns != null && leftGuns.Length > 0)
            {
                FireSide(leftGuns, ref leftGunIndex, "ЛЕВЫЙ");
            }
            else if (!useLeftSide && rightGuns != null && rightGuns.Length > 0)
            {
                FireSide(rightGuns, ref rightGunIndex, "ПРАВЫЙ");
            }
        }

        private void FireSide(GameObject[] guns, ref int index, string side)
        {
            int shots = 0;
            for (int i = 0; i < guns.Length; i++)
            {
                int idx = (index + i) % guns.Length;
                var gun = guns[idx];
                if (gun == null) continue;

                var weapon = gun.GetComponent<GunWeaponSystem>();
                if (weapon != null && weapon.IsReadyToFire())
                {
                    weapon.Fire();
                    shots++;

                    if (target == null || target.gameObject == null)
                    {
                        LogMessage($"ЦЕЛЬ УНИЧТОЖЕНА ПОСЛЕ ВЫСТРЕЛА!");
                        ResetTarget();
                        return;
                    }

                    if (shots == 1)
                        index = (idx + 1) % guns.Length;
                }
            }

            if (shots > 0)
                LogMessage($"ВЫСТРЕЛ {side} x{shots}");
        }

        //=====================================================================
        // Helpers
        //=====================================================================

        private float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle > 180f) angle -= 360f;
            if (angle < -180f) angle += 360f;
            return angle;
        }

        private void ResetTarget()
        {
            if (hasAcquiredTarget)
            {
                LogMessage("ОХОТА ПРЕКРАЩЕНА → СБРОС");
            }

            hasAcquiredTarget = false;
            target = null;
            targetLostTime = 0f;
            retargetTimer = 0f;

            shipMovement?.ShipRotate(Vector2.zero, 0f);
        }

        private void LogMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            if (Time.time - lastLogTime < LOG_INTERVAL) return;

            Debug.Log($"[NPCShoot #{shipID?.ID}] {message}");
            lastLogTime = Time.time;
        }

        private void LogError(string message)
        {
            Debug.LogError($"[NPCShoot #{shipID?.ID}] {message}", this);
        }

        //=====================================================================
        // Gizmos
        //=====================================================================

        private void OnDrawGizmosSelected()
        {
            if (detectionCollider == null) return;

            Vector2 center = (Vector2)transform.position + detectionCollider.offset;
            float radius = detectionCollider.radius;

            Gizmos.color = hasAcquiredTarget ? Color.yellow : Color.gray;
            Gizmos.DrawWireSphere(center, radius);

            if (target != null)
            {
                float angleDiff = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, targetAngle));
                Gizmos.color = angleDiff <= aimTolerance ? Color.green : Color.red;
                Gizmos.DrawLine(transform.position, target.position);

                Vector3 targetDir = Quaternion.Euler(0, 0, targetAngle) * Vector3.up * 5f;
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, targetDir);
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, transform.up * 4f);
        }
    }
}