using System.Collections.Generic;
using UnityEngine;

namespace Ship {
    public class NPCShootingTactic : MonoBehaviour, IEnemyTactic {
        [Header("Настройки стрельбы")]
        [InspectorLabel("Конфиг стрельбы NPC")]
        [Tooltip("Настройки наведения, выбора борта, памяти цели и задержек между выстрелами.")]
        [SerializeField] private NPCShootingConfig config;

        [Header("Компоненты")]
        [InspectorLabel("Коллайдер обзора")]
        [Tooltip("Круговая зона обзора для поиска цели. Если не указана, будет найдена зона Detection.")]
        [SerializeField] private CircleCollider2D detectionCollider;
        [InspectorLabel("Слой цели")]
        [Tooltip("Слой, на котором NPC ищет игрока через круговое сканирование.")]
        [SerializeField] private LayerMask targetLayer = -1;
        [InspectorLabel("Пушки")]
        [Tooltip("Явный список пушек NPC. Если пусто, пушки будут найдены автоматически в дочерних объектах корабля.")]
        [SerializeField] private GameObject[] allGunObjects;

        private ShipMovement shipMovement;
        private Transform shipTransform;
        private Transform target;
        private Rigidbody2D targetRb;
        private GunWeaponSystem activeGun;
        private bool hasAcquiredTarget;
        private float targetLostTime;
        private float activeAimTime;

        private readonly List<GunWeaponSystem> activeGuns = new();
        private readonly Collider2D[] overlapResults = new Collider2D[64];
        private float nextGunFireTime;

        private readonly struct GunAimChoice {
            public readonly GunWeaponSystem Gun;
            public readonly Vector2 AimPoint;
            public readonly float Distance;
            public readonly float AngleError;
            public readonly bool IsValid;

            public GunAimChoice(GunWeaponSystem gun, Vector2 aimPoint, float distance, float angleError)
            {
                Gun = gun;
                AimPoint = aimPoint;
                Distance = distance;
                AngleError = angleError;
                IsValid = gun != null;
            }
        }

        private void Awake()
        {
            shipMovement = GetComponentInParent<ShipMovement>();
            shipTransform = shipMovement != null ? shipMovement.transform : transform.root;
            detectionCollider = detectionCollider != null ? detectionCollider : FindDetectionCollider();

            if (config == null || shipMovement == null || detectionCollider == null) {
                Debug.LogError($"[NPCShootingTactic] Missing config, ShipMovement, or detection collider on {name}.", this);
                enabled = false;
                return;
            }

            detectionCollider.isTrigger = true;
        }

        private void Start()
        {
            CollectGuns();
        }

        public bool CanExecute(EnemyAIContext context)
        {
            return TryAcquireTarget(context);
        }

        public void Execute(EnemyAIContext context, float deltaTime)
        {
            if (!TryAcquireTarget(context)) {
                return;
            }

            if (activeGuns.Count == 0) {
                CollectGuns();
            }

            Vector2 targetVelocity = targetRb != null ? targetRb.linearVelocity : Vector2.zero;
            GunAimChoice choice = SelectGun(targetVelocity);
            if (!choice.IsValid) {
                SetActiveGun(null);
                return;
            }

            if (choice.Gun != activeGun) {
                SetActiveGun(choice.Gun);
            }

            activeAimTime += deltaTime;
            float aimError = AimSelectedGun(choice, deltaTime);
            activeGun.SetAimPreview(true, activeAimTime);

            if (Time.time < nextGunFireTime || !activeGun.CanFire() || aimError > config.FiringAngleThreshold) {
                return;
            }

            float spread = Mathf.Max(config.ShotSpreadAngle, activeGun.GetCurrentSpreadForHold(activeAimTime));
            activeGun.FireWithSpread(spread);
            nextGunFireTime = Time.time + Mathf.Max(config.SequentialGunDelay, activeGun.GetSequentialFireDelay());
            SetActiveGun(null);
        }

        private void CollectGuns()
        {
            activeGuns.Clear();

            if (allGunObjects != null) {
                for (int i = 0; i < allGunObjects.Length; i++) {
                    AddGun(allGunObjects[i] != null ? allGunObjects[i].GetComponent<GunWeaponSystem>() : null);
                }
            }

            if (activeGuns.Count == 0 && shipTransform != null) {
                GunWeaponSystem[] guns = shipTransform.GetComponentsInChildren<GunWeaponSystem>(true);
                for (int i = 0; i < guns.Length; i++) {
                    AddGun(guns[i]);
                }
            }
        }

        private void AddGun(GunWeaponSystem gun)
        {
            if (gun != null && !activeGuns.Contains(gun)) {
                activeGuns.Add(gun);
            }
        }

        private bool TryAcquireTarget(EnemyAIContext context)
        {
            Transform contextTarget = context?.Player;
            if (IsTargetInScanRadius(contextTarget)) {
                SetTarget(context, contextTarget);
                return true;
            }

            Transform scannedTarget = ScanForTarget();
            if (scannedTarget != null) {
                SetTarget(context, scannedTarget);
                return true;
            }

            if (hasAcquiredTarget) {
                targetLostTime += Time.deltaTime;
                if (targetLostTime < config.TargetMemoryTime && target != null) {
                    return true;
                }
            }

            ResetTarget();
            return false;
        }

        private void SetTarget(EnemyAIContext context, Transform newTarget)
        {
            target = newTarget;
            targetRb = target != null
                ? target.GetComponent<Rigidbody2D>() ?? target.GetComponentInParent<Rigidbody2D>() ?? target.GetComponentInChildren<Rigidbody2D>()
                : null;
            hasAcquiredTarget = target != null;
            targetLostTime = 0f;

            if (context != null && target != null) {
                context.Player = target;
                context.PlayerHealth = target.GetComponent<IShipHealth>()
                    ?? target.GetComponentInParent<IShipHealth>()
                    ?? target.GetComponentInChildren<IShipHealth>();
            }
        }

        private Transform ScanForTarget()
        {
            if (detectionCollider == null) {
                return null;
            }

            Vector2 center = GetDetectionCenter();
            float radius = GetDetectionRadius();
            var filter = new ContactFilter2D {
                useLayerMask = true,
                layerMask = targetLayer,
                useTriggers = true
            };
            int count = Physics2D.OverlapCircle(center, radius, filter, overlapResults);

            Transform bestTarget = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < count; i++) {
                Collider2D hit = overlapResults[i];
                if (hit == null || IsSelfCollider(hit)) {
                    continue;
                }

                Transform candidate = ResolveTargetRoot(hit);
                if (candidate == null) {
                    continue;
                }

                float distance = ((Vector2)candidate.position - center).sqrMagnitude;
                if (distance < bestDistance) {
                    bestDistance = distance;
                    bestTarget = candidate;
                }
            }

            return bestTarget;
        }

        private Transform ResolveTargetRoot(Collider2D hit)
        {
            if (hit.CompareTag("Player")) {
                return hit.transform;
            }

            IShipHealth health = hit.GetComponentInParent<IShipHealth>();
            if (health is Component healthComponent && healthComponent.CompareTag("Player")) {
                return healthComponent.transform;
            }

            Transform root = hit.transform.root;
            return root.CompareTag("Player") ? root : null;
        }

        private bool IsTargetInScanRadius(Transform candidate)
        {
            if (candidate == null || detectionCollider == null) {
                return false;
            }

            float radius = GetDetectionRadius();
            return Vector2.Distance(GetDetectionCenter(), candidate.position) <= radius;
        }

        private bool IsSelfCollider(Collider2D hit)
        {
            return shipTransform != null && (hit.transform == shipTransform || hit.transform.IsChildOf(shipTransform));
        }

        private GunAimChoice SelectGun(Vector2 targetVelocity)
        {
            GunAimChoice bestChoice = FindBestReadyGun(targetVelocity);
            if (!bestChoice.IsValid) {
                return default;
            }

            if (activeGun == null) {
                return bestChoice;
            }

            GunAimChoice currentChoice = BuildChoice(activeGun, targetVelocity, requireReady: true);
            if (!currentChoice.IsValid) {
                return bestChoice;
            }

            bool shouldSwitch = bestChoice.Gun != activeGun
                && currentChoice.AngleError - bestChoice.AngleError >= config.SideSwitchAngleThreshold;

            return shouldSwitch ? bestChoice : currentChoice;
        }

        private GunAimChoice FindBestReadyGun(Vector2 targetVelocity)
        {
            GunAimChoice bestChoice = default;
            float bestScore = float.MaxValue;

            for (int i = 0; i < activeGuns.Count; i++) {
                GunAimChoice choice = BuildChoice(activeGuns[i], targetVelocity, requireReady: true);
                if (!choice.IsValid) {
                    continue;
                }

                float score = choice.AngleError <= config.FiringAngleThreshold
                    ? choice.Distance
                    : choice.AngleError * 1000f + choice.Distance;

                if (score < bestScore) {
                    bestScore = score;
                    bestChoice = choice;
                }
            }

            return bestChoice;
        }

        private GunAimChoice BuildChoice(GunWeaponSystem gun, Vector2 targetVelocity, bool requireReady)
        {
            if (gun == null || (requireReady && !gun.CanFire())) {
                return default;
            }

            ProjectileConfig projectileConfig = gun.GetProjectileConfig();
            if (projectileConfig == null || target == null) {
                return default;
            }

            Vector2 origin = gun.FireOrigin;
            Vector2 aimPoint = CalculateInterceptPoint(origin, target.position, targetVelocity, projectileConfig.ProjectileSpeed);
            float distance = Vector2.Distance(origin, aimPoint);

            if (distance > projectileConfig.Range) {
                aimPoint = target.position;
                distance = Vector2.Distance(origin, aimPoint);
                if (distance > projectileConfig.Range) {
                    return default;
                }
            }

            return new GunAimChoice(gun, aimPoint, distance, gun.GetAimError(aimPoint));
        }

        private float AimSelectedGun(GunAimChoice choice, float deltaTime)
        {
            if (choice.Gun.HasTurret) {
                return choice.Gun.AimAt(choice.AimPoint, deltaTime);
            }

            return RotateShipBodyForGun(choice.Gun, choice.AimPoint, deltaTime);
        }

        private float RotateShipBodyForGun(GunWeaponSystem gun, Vector2 aimPosition, float deltaTime)
        {
            if (shipTransform == null || gun == null) {
                return 180f;
            }

            Vector2 directionToAim = aimPosition - gun.FireOrigin;
            if (directionToAim.sqrMagnitude <= 0.0001f) {
                return 0f;
            }

            float targetGunWorldAngle = GetAngleFromUp(directionToAim.normalized);
            Vector2 localGunForward = shipTransform.InverseTransformDirection(gun.Forward).normalized;
            float localGunAngle = GetAngleFromUp(localGunForward);
            float desiredShipAngle = targetGunWorldAngle - localGunAngle;

            float currentAngle = shipTransform.eulerAngles.z;
            float angleDiff = Mathf.DeltaAngle(currentAngle, desiredShipAngle);
            float absDiff = Mathf.Abs(angleDiff);

            if (absDiff <= config.RotationAimTolerance) {
                return gun.GetAimError(aimPosition);
            }

            float speedRatio = Mathf.InverseLerp(0f, config.FastTurnAngle, absDiff);
            float minTurnSpeed = config.MinTurnSpeed * config.MinTurnSpeedMultiplier;
            float maxTurnSpeed = config.MaxTurnSpeed * config.MaxTurnSpeedMultiplier;
            float currentTurnSpeed = Mathf.Lerp(minTurnSpeed, maxTurnSpeed, speedRatio);
            float step = Mathf.Clamp(angleDiff, -currentTurnSpeed * deltaTime, currentTurnSpeed * deltaTime);
            shipTransform.Rotate(0f, 0f, step);

            return gun.GetAimError(aimPosition);
        }

        private static float GetAngleFromUp(Vector2 direction)
        {
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        }

        private Vector2 CalculateInterceptPoint(Vector2 shooterPos, Vector2 targetPos, Vector2 targetVel, float projectileSpeed)
        {
            Vector2 toTarget = targetPos - shooterPos;
            float time = toTarget.magnitude / Mathf.Max(0.001f, projectileSpeed);
            return targetPos + targetVel * time;
        }

        private void SetActiveGun(GunWeaponSystem gun)
        {
            if (activeGun == gun) {
                return;
            }

            activeGun?.SetAimPreview(false, 0f);
            activeGun = gun;
            activeAimTime = 0f;
        }

        private void ResetTarget()
        {
            hasAcquiredTarget = false;
            target = null;
            targetRb = null;
            targetLostTime = 0f;
            SetActiveGun(null);
            shipMovement?.ShipRotate(Vector2.zero, 0f);
        }

        private CircleCollider2D FindDetectionCollider()
        {
            NPCAI ai = GetComponentInParent<NPCAI>();
            CircleCollider2D[] colliders = ai != null
                ? ai.GetComponentsInChildren<CircleCollider2D>(true)
                : GetComponentsInParent<CircleCollider2D>(true);

            for (int i = 0; i < colliders.Length; i++) {
                if (colliders[i] != null && colliders[i].name.Contains("Detection")) {
                    return colliders[i];
                }
            }

            CircleCollider2D ownCollider = GetComponent<CircleCollider2D>();
            return ownCollider != null ? ownCollider : (colliders.Length > 0 ? colliders[0] : null);
        }

        private Vector2 GetDetectionCenter()
        {
            return detectionCollider.transform.TransformPoint(detectionCollider.offset);
        }

        private float GetDetectionRadius()
        {
            return detectionCollider.radius * Mathf.Max(
                Mathf.Abs(detectionCollider.transform.lossyScale.x),
                Mathf.Abs(detectionCollider.transform.lossyScale.y)
            );
        }

        private void OnDrawGizmosSelected()
        {
            if (detectionCollider != null) {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(GetDetectionCenter(), GetDetectionRadius());
            }
        }
    }
}
