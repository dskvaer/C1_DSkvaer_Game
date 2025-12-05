using System.Collections.Generic;
using UnityEngine;

namespace Menu_Journal {
    // Этот компонент вешается на тот же объект, что и LootContainer,
    // или на игрока, чтобы хранить список предметов (строковые ID).
    public class SimpleInventory : MonoBehaviour {
        [Header("Debug Info")]
        // Публичный список, чтобы LootContainer мог делать foreach по items
        public List<string> items = new List<string>();

        /// <summary>
        /// Добавляет предмет в список.
        /// </summary>
        public void AddItem(string itemId, int amount = 1)
        {
            for (int i = 0; i < amount; i++)
            {
                items.Add(itemId);
            }
        }

        /// <summary>
        /// Удаляет определенное количество предметов по ID.
        /// Возвращает true, если удалось удалить все запрошенное количество.
        /// </summary>
        public bool RemoveItem(string itemId, int amount = 1)
        {
            // Сначала проверяем, есть ли нужное количество
            int count = 0;
            foreach (var i in items) if (i == itemId) count++;

            if (count < amount) return false;

            // Удаляем
            for (int i = 0; i < amount; i++)
            {
                items.Remove(itemId); // Remove удаляет первое вхождение
            }
            return true;
        }

        /// <summary>
        /// Возвращает содержимое, сгруппированное по ID (для UI).
        /// Key: ItemID, Value: Количество.
        /// </summary>
        public Dictionary<string, int> GetGroupedItems()
        {
            Dictionary<string, int> grouped = new Dictionary<string, int>();
            foreach (var id in items)
            {
                if (grouped.ContainsKey(id)) grouped[id]++;
                else grouped.Add(id, 1);
            }
            return grouped;
        }

        /// <summary>
        /// Очищает инвентарь (нужно для LootContainer после того, как игрок всё забрал).
        /// </summary>
        public void Clear()
        {
            items.Clear();
        }

        /// <summary>
        /// Возвращает текущий вес (если у ItemDataSO есть вес, тут нужна база данных. 
        /// Пока возвращаем просто кол-во предметов для примера).
        /// </summary>
        public float GetTotalWeight()
        {
            // В идеале тут нужен lookup в базу данных предметов по ID, чтобы узнать вес.
            // Пока считаем 1 предмет = 1 кг.
            return items.Count;
        }
    }
}