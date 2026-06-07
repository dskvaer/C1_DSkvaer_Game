using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Ship {
    [DisallowMultipleComponent]
    public class BanditEnemy : MonoBehaviour {
        [Header("Конструктор бандита")]
        [InspectorLabel("Конфиг типа")]
        [Tooltip("ScriptableObject, который задает ID, ранг, здоровье корпуса и список броневых зон.")]
        [SerializeField] private BanditEnemyTypeConfig config;
        [InspectorLabel("ID корабля")]
        [Tooltip("Компонент ID, который получает уникальный ID из конфига бандита.")]
        [SerializeField] private ShipID shipId;
        [InspectorLabel("Здоровье корабля")]
        [Tooltip("Компонент здоровья основного корпуса корабля.")]
        [SerializeField] private ShipHealth shipHealth;
        [InspectorLabel("Корень зон поражения")]
        [Tooltip("Transform, внутрь которого конструктор создает основную зону корпуса и броневые зоны.")]
        [SerializeField] private Transform hitZoneRoot;
        [InspectorLabel("Основная зона корпуса")]
        [Tooltip("Зона, через которую урон проходит в здоровье корабля после уничтожения защиты.")]
        [SerializeField] private BanditCoreHitZone coreHitZone;
        [InspectorLabel("Строить зоны при старте")]
        [Tooltip("Если включено, зоны поражения будут созданы автоматически при Awake.")]
        [SerializeField] private bool buildHitZonesOnAwake = true;
        [InspectorLabel("Уничтожать без брони")]
        [Tooltip("Если включено, корабль погибает после уничтожения всех броневых зон.")]
        [SerializeField] private bool destroyShipWhenAllZonesDestroyed = false;

        private readonly List<DestructibleHitZone> hitZones = new();

        public UnityEvent OnHitZonesRebuilt { get; } = new();
        public UnityEvent OnAllHitZonesDestroyed { get; } = new();

        public BanditEnemyTypeConfig Config => config;
        public IReadOnlyList<DestructibleHitZone> HitZones => hitZones;
        public bool HasActiveArmor {
            get {
                for (int i = 0; i < hitZones.Count; i++) {
                    if (hitZones[i] != null && !hitZones[i].IsDestroyed) {
                        return true;
                    }
                }

                return false;
            }
        }

        private void Awake()
        {
            ApplyConfig();
        }

        public void SetConfig(BanditEnemyTypeConfig newConfig, bool rebuildHitZones = true)
        {
            config = newConfig;
            ApplyConfig(rebuildHitZones);
        }

        public void ApplyConfig(bool rebuildHitZones = true)
        {
            CacheComponents();

            if (config == null) {
                ;
                return;
            }

            TrySetTag("Enemy");

            if (shipHealth != null) {
                shipHealth.SetMaxShipHealthOverride(config.BaseShipHealth);
            }

            if (buildHitZonesOnAwake && rebuildHitZones) {
                RebuildConstructorParts();
            }
        }

        public void RebuildConstructorParts()
        {
            CacheComponents();
            ClearGeneratedHitZones();
            hitZones.Clear();

            if (config == null) {
                return;
            }

            EnsureHitZoneRoot();
            BuildCoreHitZone();

            foreach (BanditHitZoneDefinition zoneDefinition in config.ArmorSlots) {
                if (zoneDefinition == null) {
                    continue;
                }

                DestructibleHitZone zone = CreateHitZone(zoneDefinition);
                if (zone != null) {
                    int zoneHealth = config.GetZoneHealth(zoneDefinition);
                    zone.Configure(this, zoneDefinition, zoneHealth);
                    hitZones.Add(zone);
                }
            }

            OnHitZonesRebuilt.Invoke();
        }

        public void NotifyHitZoneDamaged(DestructibleHitZone zone, int damage, bool isRam)
        {
            // Armor slots fully absorb direct damage while they are alive.
            // The core collider behind them becomes vulnerable when a shield collider is disabled.
        }

        public void NotifyHitZoneDestroyed(DestructibleHitZone zone)
        {
            if (!destroyShipWhenAllZonesDestroyed || hitZones.Count == 0) {
                return;
            }

            for (int i = 0; i < hitZones.Count; i++) {
                if (hitZones[i] != null && !hitZones[i].IsDestroyed) {
                    return;
                }
            }

            OnAllHitZonesDestroyed.Invoke();
            shipHealth?.SetShipHealth(0);
        }

        private void CacheComponents()
        {
            if (shipId == null) {
                shipId = GetComponent<ShipID>();
            }

            if (shipHealth == null) {
                shipHealth = GetComponent<ShipHealth>();
            }
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

        private void EnsureHitZoneRoot()
        {
            if (hitZoneRoot != null) {
                return;
            }

            Transform existingRoot = transform.Find("BanditHitZones");
            if (existingRoot != null) {
                hitZoneRoot = existingRoot;
                return;
            }

            var root = new GameObject("BanditHitZones");
            root.transform.SetParent(transform, false);
            hitZoneRoot = root.transform;
        }

        private void BuildCoreHitZone()
        {
            if (coreHitZone == null) {
                Transform existingCore = hitZoneRoot.Find("CoreHitArea");
                if (existingCore != null) {
                    coreHitZone = existingCore.GetComponent<BanditCoreHitZone>();
                }
            }

            if (coreHitZone == null) {
                var coreObject = new GameObject("CoreHitArea");
                coreObject.transform.SetParent(hitZoneRoot, false);
                var collider = coreObject.AddComponent<BoxCollider2D>();
                coreHitZone = coreObject.AddComponent<BanditCoreHitZone>();
            }

            coreHitZone.transform.localPosition = config.CoreLocalPosition;

            if (coreHitZone.TryGetComponent(out BoxCollider2D boxCollider)) {
                boxCollider.size = config.CoreLocalSize;
            }

            coreHitZone.Configure(this, shipHealth, config.CoreDamageMultiplier);
        }

        private DestructibleHitZone CreateHitZone(BanditHitZoneDefinition definition)
        {
            GameObject zoneObject;

            if (definition.ZonePrefab != null) {
                zoneObject = Instantiate(definition.ZonePrefab, hitZoneRoot);
                zoneObject.name = definition.ZoneId;
            }
            else {
                zoneObject = new GameObject(definition.ZoneId);
                zoneObject.transform.SetParent(hitZoneRoot, false);
                var collider = zoneObject.AddComponent<BoxCollider2D>();
                collider.size = definition.LocalSize;
            }

            zoneObject.transform.localPosition = definition.LocalPosition;

            DestructibleHitZone zone = zoneObject.GetComponent<DestructibleHitZone>();
            if (zone == null) {
                zone = zoneObject.AddComponent<DestructibleHitZone>();
            }

            return zone;
        }

        private void ClearGeneratedHitZones()
        {
            if (hitZoneRoot == null) {
                return;
            }

            if (Application.isPlaying) {
                Destroy(hitZoneRoot.gameObject);
                hitZoneRoot = null;
                coreHitZone = null;
                return;
            }

            for (int i = hitZoneRoot.childCount - 1; i >= 0; i--) {
                Transform child = hitZoneRoot.GetChild(i);
                DestroyImmediate(child.gameObject);
            }

            coreHitZone = null;
        }
    }
}
