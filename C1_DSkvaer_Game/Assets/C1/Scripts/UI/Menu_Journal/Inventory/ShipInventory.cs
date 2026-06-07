using System;
using System.Collections.Generic;
using UnityEngine;

namespace Menu_Journal {
    public class ShipInventory : MonoBehaviour, IItemContainer {
        [Header("Настройки трюма")]
        [InspectorLabel("Количество слотов")]
        [Tooltip("Сколько ячеек доступно в инвентаре корабля или юнита.")]
        [SerializeField] private int _slotCount = 20;

        [InspectorLabel("Максимальный вес")]
        [Tooltip("Максимальная грузоподъемность инвентаря в килограммах.")]
        [SerializeField] private float _maxWeight = 100f;

        [Header("Отладка и стартовые предметы")]
        [InspectorLabel("Ячейки")]
        [Tooltip("Текущий список ячеек инвентаря. Можно использовать для стартового наполнения в редакторе.")]
        [SerializeField] private List<InventorySlot> _slots;

        public event Action OnInventoryUpdated;

        public int SlotCount => _slots.Count;
        public float MaxWeight => _maxWeight;

        public float CurrentWeight
        {
            get
            {
                float total = 0;
                foreach (var slot in _slots)
                {
                    if (!slot.IsEmpty)
                        total += slot.Item.Weight * slot.Quantity;
                }
                return total;
            }
        }

        private void Awake()
        {
            if (_slots == null || _slots.Count != _slotCount)
            {
                _slots = new List<InventorySlot>();
                for (int i = 0; i < _slotCount; i++)
                {
                    _slots.Add(new InventorySlot(null, 0));
                }
            }
        }

        public bool CanAddItem(ItemDataSO item, int amount)
        {
            float newWeight = CurrentWeight + (item.Weight * amount);
            if (newWeight > MaxWeight) return false;

            return true;
        }

        public bool AddItem(ItemDataSO item, int amount)
        {
            if (!CanAddItem(item, amount)) return false;

            foreach (var slot in _slots)
            {
                if (!slot.IsEmpty && slot.Item.ID == item.ID)
                {
                    int spaceInStack = item.MaxStackSize - slot.Quantity;
                    if (spaceInStack > 0)
                    {
                        int toAdd = Mathf.Min(amount, spaceInStack);
                        slot.AddAmount(toAdd);
                        amount -= toAdd;

                        if (amount <= 0)
                        {
                            OnInventoryUpdated?.Invoke();
                            return true;
                        }
                    }
                }
            }

            while (amount > 0)
            {
                InventorySlot emptySlot = GetEmptySlot();
                if (emptySlot != null)
                {
                    int toAdd = Mathf.Min(amount, item.MaxStackSize);
                    emptySlot.SetItem(item, toAdd);
                    amount -= toAdd;
                }
                else
                {
                    break;
                }
            }

            OnInventoryUpdated?.Invoke();
            return true;
        }

        public void RemoveItem(ItemDataSO item, int amount)
        {
            foreach (var slot in _slots)
            {
                if (amount <= 0) break;

                if (!slot.IsEmpty && slot.Item.ID == item.ID)
                {
                    int toRemove = Mathf.Min(amount, slot.Quantity);
                    slot.RemoveAmount(toRemove);
                    amount -= toRemove;
                }
            }
            OnInventoryUpdated?.Invoke();
        }

        public bool HasItem(ItemDataSO item, int amount)
        {
            int found = 0;
            foreach (var slot in _slots)
            {
                if (!slot.IsEmpty && slot.Item.ID == item.ID)
                    found += slot.Quantity;
            }
            return found >= amount;
        }

        public InventorySlot GetSlot(int index)
        {
            if (index >= 0 && index < _slots.Count)
                return _slots[index];
            return null;
        }

        private InventorySlot GetEmptySlot()
        {
            return _slots.Find(slot => slot.IsEmpty);
        }
    }
}
