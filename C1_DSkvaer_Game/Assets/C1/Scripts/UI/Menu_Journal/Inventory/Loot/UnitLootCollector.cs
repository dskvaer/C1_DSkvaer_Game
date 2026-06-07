using System.Collections.Generic;
using Menu_Journal.Data;
using Menu_Journal.Systems;
using UnityEngine;

namespace Menu_Journal {
    [DisallowMultipleComponent]
    public class UnitLootCollector : MonoBehaviour {
        [Header("Подбор лута")]
        [InspectorLabel("Инвентарь")]
        [Tooltip("Инвентарь юнита, в который будут складываться подобранные предметы.")]
        [SerializeField] private ShipInventory inventory;
        [InspectorLabel("База предметов")]
        [Tooltip("База предметов для перевода ID из контейнера в ItemDataSO.")]
        [SerializeField] private ItemDatabaseSO itemDatabase;
        [InspectorLabel("Радиус подбора")]
        [Tooltip("Расстояние, на котором юнит видит и может подобрать контейнер.")]
        [SerializeField, Min(0.1f)] private float collectRadius = 2.5f;
        [InspectorLabel("Интервал подбора")]
        [Tooltip("Как часто юнит проверяет контейнеры вокруг себя.")]
        [SerializeField, Min(0.1f)] private float collectInterval = 0.5f;
        [InspectorLabel("Слои контейнеров")]
        [Tooltip("Слои, на которых находятся контейнеры с лутом.")]
        [SerializeField] private LayerMask containerLayers = -1;
        [InspectorLabel("Подбирать автоматически")]
        [Tooltip("Если включено, юнит сам забирает доступный лут в радиусе.")]
        [SerializeField] private bool collectAutomatically = true;

        private float nextCollectTime;

        private void Awake()
        {
            if (inventory == null) {
                inventory = GetComponent<ShipInventory>() ?? GetComponentInChildren<ShipInventory>() ?? GetComponentInParent<ShipInventory>();
            }

            if (inventory == null && IsPlayerBoundUnit()) {
                inventory = PlayerInventorySystem.FindCargoInventory();
            }
        }

        private void Update()
        {
            if (!collectAutomatically || Time.time < nextCollectTime) {
                return;
            }

            nextCollectTime = Time.time + collectInterval;
            CollectNearbyLoot();
        }

        [ContextMenu("Collect Nearby Loot")]
        public void CollectNearbyLoot()
        {
            if (inventory == null || itemDatabase == null) {
                return;
            }

            Collider2D[] results = Physics2D.OverlapCircleAll(transform.position, collectRadius, containerLayers);
            for (int i = 0; i < results.Length; i++) {
                LootContainer container = results[i] != null ? results[i].GetComponentInParent<LootContainer>() : null;
                if (container == null || container.Inventory == null) {
                    continue;
                }

                TransferAllPossible(container);
                container.CheckIfEmpty();
            }
        }

        private void TransferAllPossible(LootContainer container)
        {
            Dictionary<string, int> groupedItems = container.Inventory.GetGroupedItems();
            foreach (KeyValuePair<string, int> pair in groupedItems) {
                ItemDataSO item = itemDatabase.GetItem(pair.Key);
                if (item == null) {
                    continue;
                }

                int moved = 0;
                for (int amount = 0; amount < pair.Value; amount++) {
                    if (!inventory.CanAddItem(item, 1) || !inventory.AddItem(item, 1)) {
                        break;
                    }

                    moved++;
                }

                if (moved > 0) {
                    container.Inventory.RemoveItem(pair.Key, moved);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, collectRadius);
        }

        private bool IsPlayerBoundUnit()
        {
            if (GetComponentInParent<Ship.PlayerShipBinding>() != null) {
                return true;
            }

            return HasTag(gameObject, "Player") || (transform.root != null && HasTag(transform.root.gameObject, "Player"));
        }

        private static bool HasTag(GameObject target, string tag)
        {
            if (target == null) {
                return false;
            }

            try {
                return target.CompareTag(tag);
            }
            catch (UnityException) {
                return false;
            }
        }
    }
}
