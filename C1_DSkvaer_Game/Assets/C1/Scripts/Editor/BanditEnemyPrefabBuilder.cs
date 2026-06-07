#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Ship;
using UnityEditor;
using UnityEngine;

namespace C1.EditorTools {
    [InitializeOnLoad]
    public static class BanditEnemyPrefabBuilder {
        private const string PrefabFolder = "Assets/C1/Prefabs/NPC/Bandits/Base";
        private const string ConfigFolder = "Assets/C1/Prefabs/NPC/Bandits/Base/Configs";
        private const string GunPrefabPath = "Assets/C1/Prefabs/Weapon/Ship_GUN/Arbalest/Gun_Arbalest.prefab";

        private static readonly string[] ExpectedPrefabs = {
            $"{PrefabFolder}/Bandit_Junior_Raider_Base.prefab",
            $"{PrefabFolder}/Bandit_Middle_WolfHunter_Base.prefab",
            $"{PrefabFolder}/Bandit_Senior_Ambusher_Base.prefab",
            $"{PrefabFolder}/Bandit_Suicide_FireShip_Base.prefab",
            $"{PrefabFolder}/Bandit_Universal_Enemy_Base.prefab",
            $"{PrefabFolder}/Bandit_Spawner_Base.prefab"
        };

        static BanditEnemyPrefabBuilder()
        {
            EditorApplication.delayCall += CreateMissingPrefabsOnce;
        }

        [MenuItem("C1/Enemies/Create Base Bandit Prefabs")]
        public static void CreateBaseBanditPrefabs()
        {
            EnsureFolders();

            CreateBanditPrefab(new BanditPreset(
                "Bandit_Junior_Raider_Base",
                BanditEnemyRank.Junior,
                "junior_raider",
                "Junior Raider",
                70,
                8f,
                new[] {
                    TacticSlot(EnemyTacticKind.Flee, 0, 1f, 0f),
                    TacticSlot(EnemyTacticKind.Shooting, 1, 1f, 0.2f),
                    TacticSlot(EnemyTacticKind.Patrol, 5, 1f, 0f)
                },
                new[] {
                    typeof(FleeTactic),
                    typeof(NPCShootingTactic),
                    typeof(PatrolTactic)
                }));

            CreateBanditPrefab(new BanditPreset(
                "Bandit_Middle_WolfHunter_Base",
                BanditEnemyRank.Middle,
                "middle_wolf_hunter",
                "Middle Wolf Hunter",
                120,
                10f,
                new[] {
                    TacticSlot(EnemyTacticKind.Flee, 0, 1f, 0f),
                    TacticSlot(EnemyTacticKind.WolfHunt, 1, 1f, 1.5f),
                    TacticSlot(EnemyTacticKind.HookAndRam, 2, 0.7f, 3f),
                    TacticSlot(EnemyTacticKind.Shooting, 3, 1f, 0.25f),
                    TacticSlot(EnemyTacticKind.Patrol, 8, 1f, 0f)
                },
                new[] {
                    typeof(FleeTactic),
                    typeof(WolfHuntTactic),
                    typeof(HookAndRamTactic),
                    typeof(NPCShootingTactic),
                    typeof(PatrolTactic)
                }));

            CreateBanditPrefab(new BanditPreset(
                "Bandit_Senior_Ambusher_Base",
                BanditEnemyRank.Senior,
                "senior_ambusher",
                "Senior Ambusher",
                220,
                7f,
                new[] {
                    TacticSlot(EnemyTacticKind.Flee, 0, 1f, 0f),
                    TacticSlot(EnemyTacticKind.DeadRockAmbush, 1, 1f, 0f),
                    TacticSlot(EnemyTacticKind.SmokeVeil, 2, 0.8f, 8f),
                    TacticSlot(EnemyTacticKind.FalseRetreat, 3, 0.8f, 10f),
                    TacticSlot(EnemyTacticKind.HazardDropping, 4, 0.7f, 4f),
                    TacticSlot(EnemyTacticKind.Shooting, 5, 1f, 0.35f),
                    TacticSlot(EnemyTacticKind.Patrol, 9, 1f, 0f)
                },
                new[] {
                    typeof(FleeTactic),
                    typeof(DeadRockAmbushTactic),
                    typeof(SmokeVeilTactic),
                    typeof(FalseRetreatTactic),
                    typeof(HazardDroppingTactic),
                    typeof(NPCShootingTactic),
                    typeof(PatrolTactic)
                }));

            CreateBanditPrefab(new BanditPreset(
                "Bandit_Suicide_FireShip_Base",
                BanditEnemyRank.Junior,
                "suicide_fire_ship",
                "Suicide Fire Ship",
                45,
                13f,
                new[] {
                    TacticSlot(EnemyTacticKind.SuicideExplosion, 0, 1f, 0f),
                    TacticSlot(EnemyTacticKind.Patrol, 9, 1f, 0f)
                },
                new[] {
                    typeof(SuicideExplosionTactic),
                    typeof(PatrolTactic)
                }));

            CreateBanditPrefab(new BanditPreset(
                "Bandit_Universal_Enemy_Base",
                BanditEnemyRank.Middle,
                "universal_bandit",
                "Universal Bandit",
                110,
                9f,
                new[] {
                    TacticSlot(EnemyTacticKind.Flee, 0, 1f, 0f),
                    TacticSlot(EnemyTacticKind.Shooting, 1, 1f, 0.25f),
                    TacticSlot(EnemyTacticKind.HookAndRam, 2, 0.45f, 4f),
                    TacticSlot(EnemyTacticKind.Patrol, 8, 1f, 0f)
                },
                new[] {
                    typeof(FleeTactic),
                    typeof(NPCShootingTactic),
                    typeof(HookAndRamTactic),
                    typeof(PatrolTactic)
                }));

            CreateSpawnerPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateMissingPrefabsOnce()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) {
                return;
            }

            if (ExpectedPrefabs.All(path => AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)) {
                return;
            }

            CreateBaseBanditPrefabs();
        }

        private static void CreateBanditPrefab(BanditPreset preset)
        {
            string basePath = $"{ConfigFolder}/{preset.Name}";
            EnsureFolder(basePath);

            ShipMovementConfig movementConfig = CreateOrLoadAsset<ShipMovementConfig>($"{basePath}/{preset.Name}_Movement.asset");
            SetSerialized(movementConfig, "baseSpeed", preset.BaseSpeed);
            SetSerialized(movementConfig, "maxSpeed", preset.BaseSpeed);
            SetSerialized(movementConfig, "acceleration", preset.BaseSpeed * 2f);
            SetSerialized(movementConfig, "turnSpeed", preset.Rank == BanditEnemyRank.Senior ? 75f : 110f);

            ShipHealthConfig healthConfig = CreateOrLoadAsset<ShipHealthConfig>($"{basePath}/{preset.Name}_Health.asset");
            SetSerialized(healthConfig, "maxHealth", preset.BaseHealth);

            ShipRamConfig ramConfig = CreateOrLoadAsset<ShipRamConfig>($"{basePath}/{preset.Name}_Ram.asset");
            SetSerialized(ramConfig, "baseDamage", preset.Name.Contains("Suicide") ? 8 : 14);
            SetSerialized(ramConfig, "baseKnockbackForce", 12f);
            SetSerialized(ramConfig, "minRamSpeed", 1.5f);

            NPCShootingConfig shootingConfig = CreateOrLoadAsset<NPCShootingConfig>($"{basePath}/{preset.Name}_Shooting.asset");
            shootingConfig.FiringAngleThreshold = preset.Rank == BanditEnemyRank.Junior ? 16f : 10f;
            shootingConfig.ShotSpreadAngle = preset.Rank == BanditEnemyRank.Senior ? 3f : 8f;
            shootingConfig.SequentialGunDelay = 0.18f;
            shootingConfig.SideSwitchAngleThreshold = 30f;
            shootingConfig.TargetMemoryTime = 0.75f;
            EditorUtility.SetDirty(shootingConfig);

            PatrolTacticConfig patrolConfig = CreateOrLoadAsset<PatrolTacticConfig>($"{basePath}/{preset.Name}_Patrol.asset");
            SetSerialized(patrolConfig, "patrolSpeed", 0.55f);
            SetSerialized(patrolConfig, "smoothTurnSpeed", 0.3f);

            FleeTacticConfig fleeConfig = CreateOrLoadAsset<FleeTacticConfig>($"{basePath}/{preset.Name}_Flee.asset");
            SetSerialized(fleeConfig, "healthThreshold", 0.15f);
            SetSerialized(fleeConfig, "fleeSpeed", preset.BaseSpeed * 0.15f);
            SetSerialized(fleeConfig, "fleeDistance", 8f);
            SetSerialized(fleeConfig, "exhaustionTime", 10f);
            SetSerialized(fleeConfig, "exhaustedSpeedMultiplier", 0.25f);

            BanditEnemyTypeConfig banditConfig = CreateOrLoadAsset<BanditEnemyTypeConfig>($"{basePath}/{preset.Name}_BanditType.asset");
            ConfigureBanditType(banditConfig, preset);

            EnemyBehaviorProfileConfig profile = CreateOrLoadAsset<EnemyBehaviorProfileConfig>($"{basePath}/{preset.Name}_Behavior.asset");
            ConfigureProfile(profile, preset.TacticSlots);

            GameObject root = new GameObject(preset.Name);
            try {
                ConfigureRoot(root, preset, movementConfig, healthConfig, ramConfig, banditConfig, profile, shootingConfig, patrolConfig, fleeConfig);
                string prefabPath = $"{PrefabFolder}/{preset.Name}.prefab";
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        private static void ConfigureRoot(
            GameObject root,
            BanditPreset preset,
            ShipMovementConfig movementConfig,
            ShipHealthConfig healthConfig,
            ShipRamConfig ramConfig,
            BanditEnemyTypeConfig banditConfig,
            EnemyBehaviorProfileConfig profile,
            NPCShootingConfig shootingConfig,
            PatrolTacticConfig patrolConfig,
            FleeTacticConfig fleeConfig)
        {
            TrySetTag(root, "Enemy");
            TrySetLayer(root, "Enemy");

            Rigidbody2D rb = root.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            CapsuleCollider2D bodyCollider = root.AddComponent<CapsuleCollider2D>();
            bodyCollider.size = preset.Rank == BanditEnemyRank.Senior ? new Vector2(2.4f, 4.2f) : new Vector2(1.7f, 3.1f);

            SpriteRenderer spriteRenderer = CreateChild(root.transform, "Sprite_To_Assign").AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = 10;

            ShipMovement movement = root.AddComponent<ShipMovement>();
            SetObject(movement, "config", movementConfig);
            SetObject(movement, "shipSpriteRenderer", spriteRenderer);

            ShipHealth health = root.AddComponent<ShipHealth>();
            SetObject(health, "config", healthConfig);
            SetObject(health, "ramConfig", ramConfig);
            SetObject(health, "shipMovement", movement);
            SetObject(health, "shipSpriteRenderer", spriteRenderer);

            ShipID shipId = root.AddComponent<ShipID>();
            shipId.ID = $"BANDIT_{preset.Name}";

            ShipWeaponSystem weaponSystem = root.AddComponent<ShipWeaponSystem>();
            GameObject[] guns = CreateGuns(root.transform);
            SetObjectArray(weaponSystem, "leftGuns", guns.Take(1).ToArray());
            SetObjectArray(weaponSystem, "rightGuns", guns.Skip(1).Take(1).ToArray());

            CircleCollider2D detection = CreateChild(root.transform, "DetectionArea").AddComponent<CircleCollider2D>();
            detection.isTrigger = true;
            detection.radius = preset.Rank == BanditEnemyRank.Senior ? 18f : 14f;

            GameObject ramArea = CreateChild(root.transform, "RamArea");
            BoxCollider2D ramCollider = ramArea.AddComponent<BoxCollider2D>();
            ramCollider.isTrigger = true;
            ramCollider.size = new Vector2(1.4f, 0.8f);
            ramArea.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            ShipRamAttack ram = ramArea.AddComponent<ShipRamAttack>();
            SetObject(ram, "ramConfig", ramConfig);

            BanditEnemy banditEnemy = root.AddComponent<BanditEnemy>();
            SetObject(banditEnemy, "config", banditConfig);
            SetObject(banditEnemy, "shipId", shipId);
            SetObject(banditEnemy, "shipHealth", health);

            BanditShipIdProvider idProvider = root.AddComponent<BanditShipIdProvider>();
            SetObject(idProvider, "banditEnemy", banditEnemy);
            SetObject(idProvider, "configOverride", banditConfig);

            NPCAI npcAI = root.AddComponent<NPCAI>();
            SetObject(npcAI, "detectionArea", detection);
            SetSerialized(npcAI, "detectionRadius", detection.radius);
            SetObject(npcAI, "ramConfig", ramConfig);
            SetObject(npcAI, "weaponSystem", weaponSystem);
            SetObject(npcAI, "behaviorProfile", profile);

            GameObject tacticsRoot = CreateChild(root.transform, "Tactics");
            List<MonoBehaviour> tacticComponents = new();
            foreach (System.Type tacticType in preset.TacticTypes) {
                GameObject tacticObject = CreateChild(tacticsRoot.transform, tacticType.Name);
                MonoBehaviour tactic = tacticObject.AddComponent(tacticType) as MonoBehaviour;
                if (tactic == null) {
                    continue;
                }

                if (tactic is NPCShootingTactic) {
                    SetObject(tactic, "config", shootingConfig);
                    SetObject(tactic, "detectionCollider", detection);
                    SetObjectArray(tactic, "allGunObjects", guns);
                }
                else if (tactic is PatrolTactic) {
                    SetObject(tactic, "config", patrolConfig);
                }
                else if (tactic is FleeTactic) {
                    SetObject(tactic, "config", fleeConfig);
                }

                tacticComponents.Add(tactic);
            }

            EnemyBehaviorConstructor constructor = root.AddComponent<EnemyBehaviorConstructor>();
            SetObject(constructor, "profile", profile);
            SetObject(constructor, "npcAI", npcAI);
            SetObjectArray(constructor, "tacticComponents", tacticComponents.Cast<Object>().ToArray());

            SetObjectArray(npcAI, "tactics", tacticComponents.Cast<Object>().ToArray());
        }

        private static void CreateSpawnerPrefab()
        {
            EnsureFolder(PrefabFolder);
            string configPath = $"{ConfigFolder}/Bandit_Spawner_Base_Config.asset";
            EnemySpawnerConfig spawnerConfig = CreateOrLoadAsset<EnemySpawnerConfig>(configPath);
            GameObject defaultEnemy = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/Bandit_Universal_Enemy_Base.prefab")
                ?? AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/Bandit_Junior_Raider_Base.prefab");
            SetObject(spawnerConfig, "enemyPrefab", defaultEnemy);
            SetSerialized(spawnerConfig, "maxEnemies", 3);
            SetSerialized(spawnerConfig, "spawnInterval", 10f);
            SetSerialized(spawnerConfig, "minSpawnCount", 1);
            SetSerialized(spawnerConfig, "maxSpawnCount", 3);

            GameObject root = new GameObject("Bandit_Spawner_Base");
            try {
                NPCPatrolZone zone = root.AddComponent<NPCPatrolZone>();
                EnemySpawner spawner = root.AddComponent<EnemySpawner>();
                SetObject(spawner, "config", spawnerConfig);
                SetObject(spawner, "enemyPrefab", defaultEnemy);
                SetObject(spawner, "spawnZone", zone);

                PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabFolder}/Bandit_Spawner_Base.prefab");
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        private static GameObject[] CreateGuns(Transform root)
        {
            GameObject gunPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GunPrefabPath);
            var guns = new List<GameObject>();

            for (int i = 0; i < 2; i++) {
                string name = i == 0 ? "LeftGun_To_Adjust" : "RightGun_To_Adjust";
                GameObject gun = gunPrefab != null
                    ? PrefabUtility.InstantiatePrefab(gunPrefab) as GameObject
                    : new GameObject(name);

                if (gun == null) {
                    continue;
                }

                gun.name = name;
                gun.transform.SetParent(root, false);
                gun.transform.localPosition = new Vector3(i == 0 ? -1.1f : 1.1f, 0.25f, 0f);
                gun.transform.localRotation = Quaternion.Euler(0f, 0f, i == 0 ? 90f : -90f);
                guns.Add(gun);
            }

            return guns.ToArray();
        }

        private static void ConfigureBanditType(BanditEnemyTypeConfig config, BanditPreset preset)
        {
            var serialized = new SerializedObject(config);
            serialized.FindProperty("typeId").stringValue = preset.TypeId;
            serialized.FindProperty("displayName").stringValue = preset.DisplayName;
            SetEnum(serialized.FindProperty("rank"), preset.Rank);
            serialized.FindProperty("idPrefix").stringValue = "RB";
            serialized.FindProperty("baseShipHealth").intValue = preset.BaseHealth;
            serialized.FindProperty("coreLocalSize").vector2Value = new Vector2(1.1f, 1.7f);

            SerializedProperty armorSlots = serialized.FindProperty("armorSlots");
            int armorCount = preset.Rank switch {
                BanditEnemyRank.Junior => 0,
                BanditEnemyRank.Middle => 2,
                BanditEnemyRank.Senior => 4,
                _ => 0
            };

            armorSlots.arraySize = armorCount;
            for (int i = 0; i < armorCount; i++) {
                SerializedProperty slot = armorSlots.GetArrayElementAtIndex(i);
                slot.FindPropertyRelative("zoneId").stringValue = $"armor_{i + 1}";
                slot.FindPropertyRelative("displayName").stringValue = $"Armor {i + 1}";
                slot.FindPropertyRelative("maxHealth").intValue = 25 + i * 12;
                slot.FindPropertyRelative("damageMultiplier").floatValue = 1f;
                slot.FindPropertyRelative("localPosition").vector2Value = ArmorPosition(i, armorCount);
                slot.FindPropertyRelative("localSize").vector2Value = new Vector2(1.2f, 1.4f);
                slot.FindPropertyRelative("disableColliderWhenDestroyed").boolValue = true;
                slot.FindPropertyRelative("disableVisualsWhenDestroyed").boolValue = true;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }

        private static void ConfigureProfile(EnemyBehaviorProfileConfig profile, IReadOnlyList<TacticSlotData> slots)
        {
            var serialized = new SerializedObject(profile);
            serialized.FindProperty("decisionInterval").floatValue = 0.25f;
            SerializedProperty tactics = serialized.FindProperty("tactics");
            tactics.arraySize = slots.Count;

            for (int i = 0; i < slots.Count; i++) {
                SerializedProperty slot = tactics.GetArrayElementAtIndex(i);
                SetEnum(slot.FindPropertyRelative("kind"), slots[i].Kind);
                slot.FindPropertyRelative("priority").intValue = slots[i].Priority;
                slot.FindPropertyRelative("weight").floatValue = slots[i].Weight;
                slot.FindPropertyRelative("cooldown").floatValue = slots[i].Cooldown;
                slot.FindPropertyRelative("enabled").boolValue = true;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static Vector2 ArmorPosition(int index, int count)
        {
            if (count <= 1) {
                return Vector2.zero;
            }

            float spacing = 1.15f;
            float offset = (count - 1) * spacing * 0.5f;
            return new Vector2(index * spacing - offset, 0f);
        }

        private static GameObject CreateChild(Transform parent, string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static T CreateOrLoadAsset<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolders()
        {
            EnsureFolder(PrefabFolder);
            EnsureFolder(ConfigFolder);
        }

        private static void EnsureFolder(string folder)
        {
            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++) {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next)) {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }

        private static void SetSerialized(Object target, string propertyName, float value)
        {
            SerializedProperty property = FindProperty(target, propertyName);
            if (property != null) {
                property.floatValue = value;
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(target);
            }
        }

        private static void SetSerialized(Object target, string propertyName, int value)
        {
            SerializedProperty property = FindProperty(target, propertyName);
            if (property != null) {
                property.intValue = value;
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(target);
            }
        }

        private static void SetObject(Object target, string propertyName, Object value)
        {
            SerializedProperty property = FindProperty(target, propertyName);
            if (property != null) {
                property.objectReferenceValue = value;
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(target);
            }
        }

        private static void SetObjectArray(Object target, string propertyName, Object[] values)
        {
            SerializedProperty property = FindProperty(target, propertyName);
            if (property == null || !property.isArray) {
                return;
            }

            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static SerializedProperty FindProperty(Object target, string propertyName)
        {
            var serialized = new SerializedObject(target);
            return serialized.FindProperty(propertyName);
        }

        private static void SetEnum<TEnum>(SerializedProperty property, TEnum value) where TEnum : System.Enum
        {
            int index = System.Array.IndexOf(System.Enum.GetNames(typeof(TEnum)), value.ToString());
            if (index >= 0) {
                property.enumValueIndex = index;
            }
        }

        private static void TrySetTag(GameObject gameObject, string tag)
        {
            try {
                gameObject.tag = tag;
            }
            catch (UnityException) {
                gameObject.tag = "Untagged";
            }
        }

        private static void TrySetLayer(GameObject gameObject, string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer >= 0) {
                gameObject.layer = layer;
            }
        }

        private static TacticSlotData TacticSlot(EnemyTacticKind kind, int priority, float weight, float cooldown)
        {
            return new TacticSlotData(kind, priority, weight, cooldown);
        }

        private readonly struct BanditPreset {
            public readonly string Name;
            public readonly BanditEnemyRank Rank;
            public readonly string TypeId;
            public readonly string DisplayName;
            public readonly int BaseHealth;
            public readonly float BaseSpeed;
            public readonly IReadOnlyList<TacticSlotData> TacticSlots;
            public readonly IReadOnlyList<System.Type> TacticTypes;

            public BanditPreset(
                string name,
                BanditEnemyRank rank,
                string typeId,
                string displayName,
                int baseHealth,
                float baseSpeed,
                IReadOnlyList<TacticSlotData> tacticSlots,
                IReadOnlyList<System.Type> tacticTypes)
            {
                Name = name;
                Rank = rank;
                TypeId = typeId;
                DisplayName = displayName;
                BaseHealth = baseHealth;
                BaseSpeed = baseSpeed;
                TacticSlots = tacticSlots;
                TacticTypes = tacticTypes;
            }
        }

        private readonly struct TacticSlotData {
            public readonly EnemyTacticKind Kind;
            public readonly int Priority;
            public readonly float Weight;
            public readonly float Cooldown;

            public TacticSlotData(EnemyTacticKind kind, int priority, float weight, float cooldown)
            {
                Kind = kind;
                Priority = priority;
                Weight = weight;
                Cooldown = cooldown;
            }
        }
    }
}
#endif
