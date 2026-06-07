using System.Collections.Generic;
using UnityEngine;

namespace Menu_Journal {
    /// <summary>
    /// Простой инвентарь для хранения предметов по ID и количеству.
    /// </summary>
    public class SimpleInventory : MonoBehaviour {
        [System.Serializable]
        public class SimpleSlot {
            [InspectorLabel("Предмет")]
            [Tooltip("ScriptableObject предмета. Если указан, ID берется из него.")]
            public ItemDataSO Item;

            [InspectorLabel("ID предмета")]
            [Tooltip("Резервный системный ID предмета из базы ItemDatabaseSO. Используется, если Item не назначен.")]
            public string ItemID;

            [InspectorLabel("Количество")]
            [Tooltip("Сколько единиц предмета хранится в этом слоте.")]
            [Min(0)] public int Amount;

            public string ResolvedItemId => Item != null ? Item.ID : ItemID;

            public SimpleSlot(string id, int amt)
            {
                ItemID = id;
                Amount = amt;
            }

            public SimpleSlot(ItemDataSO item, int amt)
            {
                Item = item;
                ItemID = item != null ? item.ID : string.Empty;
                Amount = amt;
            }
        }

        [Header("Хранилище")]
        [InspectorLabel("Слоты")]
        [Tooltip("Список предметов в формате ID предмета + количество. Используется простыми окнами инвентаря и лута.")]
        public List<SimpleSlot> slots = new List<SimpleSlot>();

        public void AddItem(string itemId, int amount = 1)
        {
            if (amount <= 0) return;

            var existingSlot = slots.Find(s => s.ResolvedItemId == itemId);
            if (existingSlot != null)
            {
                existingSlot.Amount += amount;
            }
            else
            {
                slots.Add(new SimpleSlot(itemId, amount));
            }
        }

        public void AddItem(ItemDataSO item, int amount = 1)
        {
            if (item == null || amount <= 0) return;

            var existingSlot = slots.Find(s => s.ResolvedItemId == item.ID);
            if (existingSlot != null)
            {
                existingSlot.Item = existingSlot.Item != null ? existingSlot.Item : item;
                existingSlot.ItemID = item.ID;
                existingSlot.Amount += amount;
            }
            else
            {
                slots.Add(new SimpleSlot(item, amount));
            }
        }

        public bool RemoveItem(string itemId, int amount = 1)
        {
            if (amount <= 0) return true;

            var existingSlot = slots.Find(s => s.ResolvedItemId == itemId);
            if (existingSlot == null || existingSlot.Amount < amount)
            {
                return false;
            }

            existingSlot.Amount -= amount;
            if (existingSlot.Amount <= 0)
            {
                slots.Remove(existingSlot);
            }

            return true;
        }

        public Dictionary<string, int> GetGroupedItems()
        {
            Dictionary<string, int> grouped = new Dictionary<string, int>();
            foreach (var slot in slots)
            {
                string itemId = slot.ResolvedItemId;
                if (string.IsNullOrWhiteSpace(itemId) || slot.Amount <= 0) {
                    continue;
                }

                if (grouped.ContainsKey(itemId))
                    grouped[itemId] += slot.Amount;
                else
                    grouped.Add(itemId, slot.Amount);
            }
            return grouped;
        }

        public void Clear()
        {
            slots.Clear();
        }

        public int GetTotalCount()
        {
            int total = 0;
            foreach (var slot in slots)
            {
                total += slot.Amount;
            }
            return total;
        }
    }
}
