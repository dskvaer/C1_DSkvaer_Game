#if UNITY_EDITOR
using System.IO;
using System.Linq;
using Gameplay;
using Menu_Journal;
using Menu_Journal.Data;
using UnityEditor;
using UnityEngine;

namespace C1.EditorTools {
    public static class InventoryLootDropSetupBuilder {
        private const string ConfigPath = "Assets/C1/Scripts/UI/Menu_Journal/Inventory/SO/DefaultInventoryLootDropConfig.asset";
        private const string ItemDatabasePath = "Assets/C1/Scripts/DataBase/Item/Settings/GameItemDatabase.asset";

        private static readonly string[] PrefabsToPatch = {
            "Assets/C1/Prefabs/NPC/Bandits/Base/Bandit_Junior_Raider_Base.prefab",
            "Assets/C1/Prefabs/NPC/Bandits/Base/Bandit_Middle_WolfHunter_Base.prefab",
            "Assets/C1/Prefabs/NPC/Bandits/Base/Bandit_Senior_Ambusher_Base.prefab",
            "Assets/C1/Prefabs/NPC/Bandits/Base/Bandit_Suicide_FireShip_Base.prefab",
            "Assets/C1/Prefabs/Player/Raft/Raft_Player_0.prefab"
        };

        [MenuItem("C1/Loot/Create Inventory Death Drop Setup")]
        public static void CreateInventoryDeathDropSetup()
        {
            InventoryLootDropConfigSO config = CreateOrLoadConfig();

            foreach (string prefabPath in PrefabsToPatch) {
                PatchPrefab(prefabPath, config);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static InventoryLootDropConfigSO CreateOrLoadConfig()
        {
            EnsureFolder(Path.GetDirectoryName(ConfigPath)?.Replace('\\', '/'));

            InventoryLootDropConfigSO config = AssetDatabase.LoadAssetAtPath<InventoryLootDropConfigSO>(ConfigPath);
            if (config != null) {
                return config;
            }

            config = ScriptableObject.CreateInstance<InventoryLootDropConfigSO>();
            AssetDatabase.CreateAsset(config, ConfigPath);
            EditorUtility.SetDirty(config);
            return config;
        }

        private static void PatchPrefab(string prefabPath, InventoryLootDropConfigSO config)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) {
                return;
            }

            GameObject instance = PrefabUtility.LoadPrefabContents(prefabPath);
            try {
                ShipInventory inventory = instance.GetComponent<ShipInventory>() ?? instance.GetComponentInChildren<ShipInventory>(true);
                if (inventory == null) {
                    inventory = instance.AddComponent<ShipInventory>();
                }

                InventoryLootDropper dropper = instance.GetComponent<InventoryLootDropper>() ?? instance.GetComponentInChildren<InventoryLootDropper>(true);
                if (dropper == null) {
                    dropper = instance.AddComponent<InventoryLootDropper>();
                }

                MonoBehaviour healthSource = instance.GetComponentsInChildren<MonoBehaviour>(true)
                    .FirstOrDefault(component => component is IHealth);

                var serialized = new SerializedObject(dropper);
                serialized.FindProperty("config").objectReferenceValue = config;
                serialized.FindProperty("shipInventory").objectReferenceValue = inventory;
                serialized.FindProperty("healthSource").objectReferenceValue = healthSource;
                serialized.FindProperty("dropOnDeath").boolValue = true;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                if (prefabPath.Contains("/NPC/Bandits/")) {
                    UnitLootCollector collector = instance.GetComponent<UnitLootCollector>() ?? instance.AddComponent<UnitLootCollector>();
                    ItemDatabaseSO database = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(ItemDatabasePath);
                    var collectorSerialized = new SerializedObject(collector);
                    collectorSerialized.FindProperty("inventory").objectReferenceValue = inventory;
                    collectorSerialized.FindProperty("itemDatabase").objectReferenceValue = database;
                    collectorSerialized.FindProperty("collectAutomatically").boolValue = true;
                    collectorSerialized.ApplyModifiedPropertiesWithoutUndo();
                }

                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            }
            finally {
                PrefabUtility.UnloadPrefabContents(instance);
            }
        }

        private static void EnsureFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder)) {
                return;
            }

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
    }
}
#endif
