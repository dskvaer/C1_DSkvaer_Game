using System.Collections;
using UnityEngine;

namespace Ship {
    [RequireComponent(typeof(GunEffects), typeof(GunSound), typeof(GunVisualStates))]
    public sealed class GunWeaponSystem : MonoBehaviour {
        [Header("Настройки орудия")]
        [InspectorLabel("Конфиг пушки")]
        [Tooltip("ScriptableObject с настройками скорострельности, разброса и времени сведения.")]
        [SerializeField] private GunConfig gunConfig;
        [InspectorLabel("Точка выстрела")]
        [Tooltip("Transform, из которого появляется снаряд. Его направление вверх считается направлением стрельбы.")]
        [SerializeField] private Transform weaponPoint;
        [InspectorLabel("Префаб снаряда")]
        [Tooltip("Префаб с компонентом Projectile, который будет создан при выстреле.")]
        [SerializeField] private GameObject bulletPrefab;

        private GunEffects gunEffects;
        private GunSound gunSound;
        private GunVisualStates gunVisualStates;
        private GunAimZone aimZone;
        private GunTurretController turretController;
        private ProjectileConfig cachedProjectileConfig;
        private Transform ownerRoot;
        private IShipHealth ownerHealth;
        private bool isReadyToFire = true;

        private void Awake()
        {
            gunEffects = GetComponent<GunEffects>();
            gunSound = GetComponent<GunSound>();
            gunVisualStates = GetComponent<GunVisualStates>();
            aimZone = GetComponentInChildren<GunAimZone>(true);
            turretController = GetComponent<GunTurretController>() ?? GetComponentInParent<GunTurretController>();
            CacheOwner();

            if (gunConfig == null || bulletPrefab == null || weaponPoint == null) {
                Debug.LogError($"[GunWeaponSystem] Missing config, bullet prefab, or weapon point on {name}.", this);
                enabled = false;
                return;
            }

            Projectile prefabProjectile = bulletPrefab.GetComponent<Projectile>();
            if (prefabProjectile == null) {
                Debug.LogError($"[GunWeaponSystem] Bullet prefab {bulletPrefab.name} has no Projectile component.", this);
                enabled = false;
                return;
            }

            cachedProjectileConfig = prefabProjectile.GetProjectileConfig();
            gunVisualStates?.SetReadyState();
        }

        public GameObject Fire()
        {
            float quickSpread = gunConfig != null ? gunConfig.MaxAimSpreadAngle : 30f;
            return FireWithSpread(quickSpread);
        }

        public GameObject FireWithSpread(float spreadAngle)
        {
            if (Time.timeSinceLevelLoad < 0.15f || !CanFire()) {
                return null;
            }

            float halfSpread = Mathf.Max(0f, spreadAngle) * 0.5f;
            float randomAngle = Random.Range(-halfSpread, halfSpread);
            Quaternion rotation = weaponPoint.rotation * Quaternion.Euler(0f, 0f, randomAngle);
            GameObject projectile = Instantiate(bulletPrefab, weaponPoint.position, rotation);
            if (projectile == null) {
                return null;
            }

            Projectile projectileComponent = projectile.GetComponent<Projectile>();
            projectileComponent?.SetOwner(ownerRoot, ownerHealth);

            gunEffects?.PlayMuzzleFlash(weaponPoint.position, weaponPoint.rotation);
            gunEffects?.PlayTrailEffect(projectile);
            gunSound?.PlayShotSound(weaponPoint.position);

            isReadyToFire = false;
            gunVisualStates?.SetReloadingState();
            gunSound?.PlayReloadSound(weaponPoint.position);

            float cooldown = 1f / Mathf.Max(0.0001f, gunConfig.FireRate);
            StartCoroutine(UnlockAfter(cooldown));
            return projectile;
        }

        public float GetCurrentSpreadForHold(float holdTime)
        {
            return gunConfig != null ? gunConfig.GetFocusedSpread(holdTime) : 30f;
        }

        public float GetMaxAimSpread() => gunConfig != null ? gunConfig.MaxAimSpreadAngle : 30f;
        public float GetMinAimSpread() => gunConfig != null ? gunConfig.MinAimSpreadAngle : 0f;
        public float GetSequentialFireDelay() => gunConfig != null ? gunConfig.SequentialFireDelay : 0.12f;
        public Transform WeaponPoint => weaponPoint != null ? weaponPoint : transform;
        public Vector2 FireOrigin => WeaponPoint.position;
        public Vector2 Forward => WeaponPoint.up;
        public bool HasTurret => turretController != null && turretController.enabled;

        public float AimAt(Vector2 worldPoint, float deltaTime)
        {
            return HasTurret ? turretController.AimAt(worldPoint, deltaTime) : GetAimError(worldPoint);
        }

        public float GetAimError(Vector2 worldPoint)
        {
            Vector2 toTarget = worldPoint - FireOrigin;
            if (toTarget.sqrMagnitude <= 0.0001f) {
                return 0f;
            }

            return Mathf.Abs(Vector2.SignedAngle(Forward, toTarget.normalized));
        }

        public void SetAimPreview(bool visible, float holdTime)
        {
            if (aimZone == null) {
                return;
            }

            aimZone.SetAimPreview(visible, GetMaxAimSpread(), GetCurrentSpreadForHold(holdTime), aimZone.GetAimRange());
        }

        private IEnumerator UnlockAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            isReadyToFire = true;
            gunVisualStates?.SetReadyState();
        }

        private void CacheOwner()
        {
            ownerHealth = GetComponentInParent<IShipHealth>();
            if (ownerHealth is Component ownerComponent) {
                ownerRoot = ownerComponent.transform;
                return;
            }

            ownerRoot = transform.root;
        }

        public GunConfig GetGunConfig() => gunConfig;
        public bool CanFire() => isReadyToFire && enabled && gameObject.activeInHierarchy;
        public ProjectileConfig GetProjectileConfig() => cachedProjectileConfig;
        public bool IsReadyToFire() => isReadyToFire;
    }
}
