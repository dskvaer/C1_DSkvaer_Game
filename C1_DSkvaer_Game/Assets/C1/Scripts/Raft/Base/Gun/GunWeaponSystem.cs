// ====================================================================================================
// GunWeaponSystem.cs – ДИАГНОСТИЧЕСКАЯ ВЕРСИЯ + CanFire() + GetProjectileConfig()
// ====================================================================================================

using System.Collections;
using UnityEngine;

namespace Ship {
    /// <summary>
    /// Пушка: стреляет, создаёт снаряд, полные логи.
    /// </summary>
    /// <remarks>
    /// **ДОБАВЛЕНО:**
    /// • `CanFire()` → проверяет `isReadyToFire`
    /// • `GetProjectileConfig()` → возвращает `projectileConfig` из снаряда-префаба
    /// 
    /// **Inspector:**
    /// • Gun Config – FireRate, Accuracy, Spread
    /// • Weapon Point – точка спавна
    /// • Bullet Prefab – префаб с Projectile + ProjectileConfig!
    /// </remarks>
    [RequireComponent(typeof(GunEffects), typeof(GunSound), typeof(GunVisualStates))]
    public sealed class GunWeaponSystem : MonoBehaviour {
        // --------------------------------------------------------------------- Inspector
        [Header("Config")]
        [SerializeField, Tooltip("Параметры пушки: FireRate, Accuracy, Spread")]
        private GunConfig gunConfig;

        [SerializeField, Tooltip("Точка спавна снаряда")]
        private Transform weaponPoint;

        [SerializeField, Tooltip("ПРЕФАБ СНАРЯДА ИЗ PROJECT (с Projectile + ProjectileConfig)")]
        private GameObject bulletPrefab;

        // --------------------------------------------------------------------- Private
        private GunEffects gunEffects;
        private GunSound gunSound;
        private GunVisualStates gunVisualStates;
        private bool isReadyToFire = true;
        private ProjectileConfig cachedProjectileConfig; // кэшируем из префаба

        // --------------------------------------------------------------------- Unity: Awake
        private void Awake()
        {
            Debug.Log($"[GunWeaponSystem] ШАГ 1: Awake {name} начат", this);

            gunEffects = GetComponent<GunEffects>();
            gunSound = GetComponent<GunSound>();
            gunVisualStates = GetComponent<GunVisualStates>();

            if (gunConfig == null)
            {
                Debug.LogError($"[GunWeaponSystem] GunConfig == null у {name} → ОТКЛЮЧАЕМ", this);
                enabled = false; return;
            }
            Debug.Log($"[GunWeaponSystem] GunConfig OK: {gunConfig.name} (FireRate={gunConfig.FireRate:F2})", this);

            if (bulletPrefab == null)
            {
                Debug.LogError($"[GunWeaponSystem] bulletPrefab == null у {name} → ОТКЛЮЧАЕМ", this);
                enabled = false; return;
            }
            Debug.Log($"[GunWeaponSystem] bulletPrefab OK: {bulletPrefab.name}", this);

            if (weaponPoint == null)
            {
                Debug.LogError($"[GunWeaponSystem] weaponPoint == null у {name} → ОТКЛЮЧАЕМ", this);
                enabled = false; return;
            }
            Debug.Log($"[GunWeaponSystem] weaponPoint OK: {weaponPoint.name}", this);

            var prefabProjectile = bulletPrefab.GetComponent<Projectile>();
            if (prefabProjectile == null)
            {
                Debug.LogError($"[GunWeaponSystem] На префабе {bulletPrefab.name} нет Projectile! → ОТКЛЮЧАЕМ", this);
                enabled = false; return;
            }
            Debug.Log($"[GunWeaponSystem] Projectile на префабе OK", this);

            // Кэшируем ProjectileConfig из префаба
            cachedProjectileConfig = prefabProjectile.GetProjectileConfig();
            if (cachedProjectileConfig == null)
            {
                Debug.LogWarning($"[GunWeaponSystem] ProjectileConfig НЕ НАЗНАЧЕН в префабе {bulletPrefab.name}!", this);
            }
            else
            {
                Debug.Log($"[GunWeaponSystem] ProjectileConfig кэширован: {cachedProjectileConfig.name} (Range={cachedProjectileConfig.Range:F1})", this);
            }

            gunVisualStates?.SetReadyState();
            Debug.Log($"[GunWeaponSystem] {name} ПОЛНОСТЬЮ ГОТОВ", this);
        }

        // --------------------------------------------------------------------- Public: Fire()
        public GameObject Fire()
        {
            Debug.Log($"[GunWeaponSystem] ШАГ 2: Fire() вызван для {name}", this);

            if (Time.timeSinceLevelLoad < 0.15f)
            {
                Debug.Log($"[GunWeaponSystem] Time.timeSinceLevelLoad={Time.timeSinceLevelLoad:F3} < 0.15 → БЛОКИРУЕМ", this);
                return null;
            }

            if (!isReadyToFire)
            {
                Debug.Log($"[GunWeaponSystem] isReadyToFire=false → ПЕРЕЗАРЯДКА", this);
                return null;
            }

            float spread = Random.Range(gunConfig.MinSpreadAngle, gunConfig.SpreadAngle);
            float accuracyFactor = 1f - Mathf.Clamp01(gunConfig.Accuracy / 100f);
            float angle = spread * accuracyFactor;
            Quaternion rotation = weaponPoint.rotation * Quaternion.Euler(0f, 0f, angle);
            Debug.Log($"[GunWeaponSystem] Разброс: {angle:F2}°", this);

            Debug.Log($"[GunWeaponSystem] Instantiate({bulletPrefab.name}, {weaponPoint.position}, {rotation.eulerAngles})", this);
            GameObject proj = Instantiate(bulletPrefab, weaponPoint.position, rotation);
            Debug.Log($"[GunWeaponSystem] Instantiate вернул: {(proj != null ? proj.name : "NULL!")}", this);

            if (proj == null)
            {
                Debug.LogError($"[GunWeaponSystem] Instantiate ПРОВАЛ! proj == null", this);
                return null;
            }

            gunEffects?.PlayMuzzleFlash(weaponPoint.position, weaponPoint.rotation);
            gunEffects?.PlayTrailEffect(proj);
            gunSound?.PlayShotSound(weaponPoint.position);
            Debug.Log($"[GunWeaponSystem] Эффекты запущены", this);

            isReadyToFire = false;
            gunVisualStates?.SetReloadingState();
            float cooldown = 1f / Mathf.Max(0.0001f, gunConfig.FireRate);
            StartCoroutine(UnlockAfter(cooldown));
            Debug.Log($"[GunWeaponSystem] Перезарядка: {cooldown:F3}s", this);

            Debug.Log($"[GunWeaponSystem] Fire() УСПЕШЕН → {proj.name}", this);
            return proj;
        }

        // --------------------------------------------------------------------- Coroutine: разблокировка
        private IEnumerator UnlockAfter(float seconds)
        {
            Debug.Log($"[GunWeaponSystem] UnlockAfter({seconds:F3}) запущен", this);
            yield return new WaitForSeconds(seconds);
            isReadyToFire = true;
            gunVisualStates?.SetReadyState();
            Debug.Log($"[GunWeaponSystem] {name} ГОТОВА К ВЫСТРЕЛУ", this);
        }

        // --------------------------------------------------------------------- Public API
        public GunConfig GetGunConfig() => gunConfig;

        /// <summary>Можно ли стрелять? (учитывает перезарядку)</summary>
        public bool CanFire()
        {
            bool can = isReadyToFire;
            Debug.Log($"[GunWeaponSystem] CanFire() → {can} (isReadyToFire={isReadyToFire})", this);
            return can;
        }

        /// <summary>Возвращает ProjectileConfig из префаба (кэшировано)</summary>
        public ProjectileConfig GetProjectileConfig()
        {
            Debug.Log($"[GunWeaponSystem] GetProjectileConfig() → {(cachedProjectileConfig != null ? cachedProjectileConfig.name : "null")}", this);
            return cachedProjectileConfig;
        }

        public bool IsReadyToFire() => isReadyToFire;
    }
}