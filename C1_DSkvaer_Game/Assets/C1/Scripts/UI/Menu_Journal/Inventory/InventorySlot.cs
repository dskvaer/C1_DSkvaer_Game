using System;
using UnityEngine;

namespace Menu_Journal {
    [Serializable]
    public class InventorySlot {
        [InspectorLabel("Предмет")]
        [Tooltip("Данные предмета в этом слоте.")]
        [SerializeField] private ItemDataSO _item;

        [InspectorLabel("Количество")]
        [Tooltip("Сколько единиц предмета хранится в этом слоте.")]
        [SerializeField] private int _quantity;

        public ItemDataSO Item => _item;
        public int Quantity => _quantity;
        public bool IsEmpty => _item == null || _quantity <= 0;

        public InventorySlot(ItemDataSO item, int quantity)
        {
            _item = item;
            _quantity = quantity;
        }

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
