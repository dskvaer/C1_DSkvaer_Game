using System;

namespace Menu_Journal {
    public interface IItemContainer {
        // Событие: вызывается, когда содержимое меняется (чтобы UI перерисовался)
        event Action OnInventoryUpdated;

        // Основные параметры
        int SlotCount { get; }          // Сколько всего ячеек
        float CurrentWeight { get; }    // Текущий вес
        float MaxWeight { get; }        // Максимальная грузоподъемность

        // Методы взаимодействия
        InventorySlot GetSlot(int index); // Получить данные конкретной ячейки

        bool CanAddItem(ItemDataSO item, int amount); // Проверка: влезет ли?
        bool AddItem(ItemDataSO item, int amount);    // Добавить
        void RemoveItem(ItemDataSO item, int amount); // Удалить
        bool HasItem(ItemDataSO item, int amount);    // Проверка наличия (для квестов/крафта)
    }
}