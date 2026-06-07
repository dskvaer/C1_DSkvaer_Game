using System.Collections.Generic;
using Menu_Journal;
using Menu_Journal.Data;
using Menu_Journal.Systems;
using UnityEngine;

namespace Gameplay {
    [DisallowMultipleComponent]
    public class InventoryLootDropper : MonoBehaviour {
        [Header("Выпадение лута")]
        [InspectorLabel("Конфиг дропа")]
        [Tooltip("Настройки того, как инвентарь юнита превращается в контейнер с лутом.")]
        [SerializeField] private InventoryLootDropConfigSO config;
        [InspectorLabel("Инвентарь корабля")]
        [Tooltip("Основной инвентарь юнита. Если пусто, компонент будет найден автоматически.")]
        [SerializeField] private ShipInventory shipInventory;
        [InspectorLabel("Простой инвентарь")]
        [Tooltip("Дополнительный простой инвентарь для контейнеров или легких юнитов.")]
        [SerializeField] private SimpleInventory simpleInventory;
        [InspectorLabel("База предметов")]
        [Tooltip("База предметов нужна для чтения SimpleInventory по ID предметов.")]
        [SerializeField] private ItemDatabaseSO itemDatabase;
        [InspectorLabel("Источник здоровья")]
        [Tooltip("Компонент здоровья, по событию смерти которого будет создан контейнер.")]
        [SerializeField] private MonoBehaviour healthSource;
        [InspectorLabel("Дроп при смерти")]
        [Tooltip("Если включено, контейнер создается автоматически при смерти юнита.")]
        [SerializeField] private bool dropOnDeath = true;

        private IHealth health;
        private bool dropped;

        private void Awake()
        {
            CacheComponents();
        }

        private void OnEnable()
        {
            if (!dropOnDeath) {
                return;
            }

            CacheComponents();
            health?.OnDeath.AddListener(DropLoot);
        }

        private void OnDisable()
        {
            health?.OnDeath.RemoveListener(DropLoot);
        }

        [ContextMenu("Drop Loot Now")]
        public void DropLoot()
        {
            if (dropped || config == null || LootSpawner.Instance == null) {
                return;
            }

            if (Random.Range(0f, 100f) > config.ContainerDropChance) {
                dropped = true;
                return;
            }

            List<ItemDataSO> items = BuildDropItems();
            if (items.Count == 0) {
                dropped = true;
                return;
            }

            LootSpawner.Instance.SpawnFromItems(items, transform.position + config.SpawnOffset, config.ContainerTheme);

            if (config.ClearSourceInventoryAfterDrop) {
                ClearSourceInventory();
            }

            dropped = true;
        }

        private void CacheComponents()
        {
            if (shipInventory == null) {
                shipInventory = GetComponent<ShipInventory>() ?? GetComponentInChildren<ShipInventory>() ?? GetComponentInParent<ShipInventory>();
            }

            if (shipInventory == null && IsPlayerBoundUnit()) {
                shipInventory = PlayerInventorySystem.FindCargoInventory();
            }

            if (simpleInventory == null) {
                simpleInventory = GetComponent<SimpleInventory>() ?? GetComponentInChildren<SimpleInventory>() ?? GetComponentInParent<SimpleInventory>();
            }

            if (health == null) {
                health = healthSource as IHealth
                    ?? GetComponent<IHealth>()
                    ?? GetComponentInChildren<IHealth>()
                    ?? GetComponentInParent<IHealth>();
            }
        }

        private List<ItemDataSO> BuildDropItems()
        {
            var result = new List<ItemDataSO>();

            if (config.DropMode == InventoryLootDropMode.InventoryOnly || config.DropMode == InventoryLootDropMode.InventoryAndProfile) {
                AddShipInventoryItems(result);
                AddSimpleInventoryItems(result);
            }

            if ((config.DropMode == InventoryLootDropMode.ProfileOnly || config.DropMode == InventoryLootDropMode.InventoryAndProfile)
                && config.LootProfile != null
                && LootSpawner.Instance != null) {
                result.AddRange(LootSpawner.Instance.GenerateItems(config.LootProfile));
            }

            return result;
        }

        private void AddShipInventoryItems(List<ItemDataSO> result)
        {
            if (shipInventory == null) {
                return;
            }

            for (int i = 0; i < shipInventory.SlotCount; i++) {
                InventorySlot slot = shipInventory.GetSlot(i);
                if (slot == null || slot.IsEmpty || slot.Item == null) {
                    continue;
                }

                for (int amount = 0; amount < slot.Quantity; amount++) {
                    result.Add(slot.Item);
                }
            }
        }

        private void AddSimpleInventoryItems(List<ItemDataSO> result)
        {
            if (simpleInventory == null || itemDatabase == null) {
                return;
            }

            foreach (KeyValuePair<string, int> item in simpleInventory.GetGroupedItems()) {
                ItemDataSO data = itemDatabase.GetItem(item.Key);
                if (data == null) {
                    continue;
                }

                for (int amount = 0; amount < item.Value; amount++) {
                    result.Add(data);
                }
            }
        }

        private void ClearSourceInventory()
        {
            if (shipInventory != null) {
                var toRemove = new List<InventorySlot>();
                for (int i = 0; i < shipInventory.SlotCount; i++) {
                    InventorySlot slot = shipInventory.GetSlot(i);
                    if (slot != null && !slot.IsEmpty) {
                        toRemove.Add(new InventorySlot(slot.Item, slot.Quantity));
                    }
                }

                foreach (InventorySlot slot in toRemove) {
                    shipInventory.RemoveItem(slot.Item, slot.Quantity);
                }
            }

            simpleInventory?.Clear();
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
