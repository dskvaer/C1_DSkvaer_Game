using System;
using System.Collections.Generic;
using UnityEngine;

namespace Menu_Journal {
    public class ShipInventory : MonoBehaviour, IItemContainer {
        [Header("Settings")]
        [SerializeField] private int _slotCount = 20; // Размер трюма
        [SerializeField] private float _maxWeight = 100f; // Грузоподъемность

        [Header("Debug / Starting Items")]
        [SerializeField] private List<InventorySlot> _slots; // Список ячеек

        // Реализация события интерфейса
        public event Action OnInventoryUpdated;

        // Реализация свойств интерфейса
        public int SlotCount => _slots.Count;
        public float MaxWeight => _maxWeight;

        // Вычисляем текущий вес динамически, проходясь по всем предметам
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
            // Инициализация пустых слотов при старте
            if (_slots == null || _slots.Count != _slotCount)
            {
                _slots = new List<InventorySlot>();
                for (int i = 0; i < _slotCount; i++)
                {
                    _slots.Add(new InventorySlot(null, 0));
                }
            }
        }

        // --- ЛОГИКА ДОБАВЛЕНИЯ ПРЕДМЕТА ---
        public bool CanAddItem(ItemDataSO item, int amount)
        {
            // 1. Проверка веса
            float newWeight = CurrentWeight + (item.Weight * amount);
            if (newWeight > MaxWeight) return false;

            // 2. Проверка места (есть ли пустой слот или слот с тем же предметом, где есть место)
            // Упрощенная проверка: считаем, что место есть, если вес позволяет 
            // (В идеале нужно проверять StackSize, но для начала хватит веса)
            return true;
        }

        public bool AddItem(ItemDataSO item, int amount)
        {
            if (!CanAddItem(item, amount)) return false;

            // Шаг 1: Ищем существующий стак этого предмета
            foreach (var slot in _slots)
            {
                if (!slot.IsEmpty && slot.Item.ID == item.ID)
                {
                    // Проверяем, влезает ли в стак
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

            // Шаг 2: Если осталось что добавить, ищем пустой слот
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
                    // Слоты кончились, а предметы еще остались (вернем false или true с оговоркой)
                    // Для простоты считаем, что операция частично удалась
                    break;
                }
            }

            OnInventoryUpdated?.Invoke();
            return true;
        }

        // --- ЛОГИКА УДАЛЕНИЯ ПРЕДМЕТА ---
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