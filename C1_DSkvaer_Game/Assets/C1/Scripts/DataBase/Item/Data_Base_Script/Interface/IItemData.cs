using UnityEngine;

namespace Menu_Journal {
    public interface IItemData {
        // Основная информация
        string ID { get; }
        string Name { get; }
        Sprite Icon { get; }
        string Description { get; }

        // Физические свойства
        float Weight { get; }
        int MaxStackSize { get; }

        // Классификация и Торговля
        ItemCategorySO Category { get; }         // Ссылка на ассет категории
        ItemValueCategory ValueCategory { get; } // Тип ценности
        int BasePrice { get; }                   // Цена
        bool IsCurrency { get; }                 // Это деньги?
        ItemRarity Rarity { get; }               // Редкость
    }
}