using System.Collections.Generic;
using Menu_Journal;
using Menu_Journal.Data;
using Menu_Journal.UI;
using UnityEngine;

namespace Menu_Journal.Systems {
    public class LootSpawner : MonoBehaviour {
        public static LootSpawner Instance { get; private set; }

        [Header("Настройки спавна")]
        [InspectorLabel("Префаб контейнера")]
        [Tooltip("Префаб контейнера, который создается при выпадении или сбросе лута.")]
        [SerializeField] private LootContainer _containerPrefab;
        [InspectorLabel("База тем")]
        [Tooltip("База визуальных тем контейнеров по префиксам ID.")]
        [SerializeField] private ContainerThemeDatabaseSO _themeDatabase;

        [Header("Значения по умолчанию")]
        [InspectorLabel("ID темы игрока")]
        [Tooltip("Префикс темы, которая используется для контейнеров, сброшенных игроком.")]
        [SerializeField] private string _playerThemeID = "PLAYER";

        private void Awake()
        {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                return;
            }

            Destroy(gameObject);
        }

        public LootContainer SpawnPlayerDrop(Vector3 position)
        {
            ContainerThemeSO theme = _themeDatabase != null ? _themeDatabase.GetTheme(_playerThemeID) : null;
            return SpawnFromItems(new List<ItemDataSO>(), position, theme, allowEmpty: true);
        }

        public LootContainer SpawnFromItems(List<ItemDataSO> items, Vector3 position, ContainerThemeSO theme, bool allowEmpty = false)
        {
            if (_containerPrefab == null || items == null || (!allowEmpty && items.Count == 0)) {
                return null;
            }

            LootContainer container = Instantiate(_containerPrefab, position, Quaternion.identity);
            container.Initialize(items, theme);
            return container;
        }

        public void SpawnFromProfile(LootProfileSO profile, Vector3 position)
        {
            if (profile == null) {
                return;
            }

            if (Random.Range(0f, 100f) > profile.ContainerDropChance) {
                return;
            }

            List<ItemDataSO> items = GenerateItems(profile);
            SpawnFromItems(items, position, profile.Theme);
        }

        public List<ItemDataSO> GenerateItems(LootProfileSO profile)
        {
            var result = new List<ItemDataSO>();
            if (profile == null || profile.PossibleLoot == null || profile.PossibleLoot.Count == 0) {
                return result;
            }

            int count = Random.Range(profile.MinItemsCount, profile.MaxItemsCount + 1);
            int safeGuard = 0;

            while (result.Count < count && safeGuard < 100) {
                safeGuard++;
                LootDropItem drop = profile.PossibleLoot[Random.Range(0, profile.PossibleLoot.Count)];
                if (drop == null || drop.Item == null) {
                    continue;
                }

                if (Random.Range(0f, 100f) <= drop.DropChance) {
                    int amount = Random.Range(drop.MinAmount, drop.MaxAmount + 1);
                    for (int i = 0; i < amount; i++) {
                        result.Add(drop.Item);
                    }
                }
            }

            return result;
        }
    }
}
