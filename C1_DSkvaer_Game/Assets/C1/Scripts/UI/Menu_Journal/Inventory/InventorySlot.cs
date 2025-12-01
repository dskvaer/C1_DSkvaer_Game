using System;
using UnityEngine;

namespace Menu_Journal {
    [Serializable] // Позволяет видеть этот класс в Инспекторе Unity
    public class InventorySlot {
        [SerializeField] private ItemDataSO _item;
        [SerializeField] private int _quantity;

        // Свойства для чтения
        public ItemDataSO Item => _item;
        public int Quantity => _quantity;
        public bool IsEmpty => _item == null || _quantity <= 0;

        // Конструктор
        public InventorySlot(ItemDataSO item, int quantity)
        {
            _item = item;
            _quantity = quantity;
        }

        // --- Методы изменения состояния ---

        public void AddAmount(int amount)
        {
            _quantity += amount;
        }

        public void RemoveAmount(int amount)
        {
            _quantity -= amount;
            if (_quantity <= 0)
            {
                Clear();
            }
        }

        public void SetItem(ItemDataSO newItem, int amount)
        {
            _item = newItem;
            _quantity = amount;
        }

        public void Clear()
        {
            _item = null;
            _quantity = 0;
        }
    }
}