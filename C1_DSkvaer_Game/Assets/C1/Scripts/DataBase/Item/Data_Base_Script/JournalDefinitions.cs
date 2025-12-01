// справочник типов
namespace Menu_Journal {

// Редкость предмета (влияет на цвет рамки)
public enum ItemRarity {
        Common,     // Обычный (Серый)
        Uncommon,   // Необычный (Зеленый)
        Rare,       // Редкий (Синий)
        Legendary   // Легендарный (Золотой)
    }

    // Экономическая категория (для логики торговцев)
    public enum ItemValueCategory {
        Common,     // Обычный товар
        Luxury,     // Роскошь (покупают богатые)
        Strategic,  // Стратегический (покупают военные)
        Currency,   // Валюта (деньги)
        Rare        // Редкость
    }

    // Типы вкладок нашего журнала
    public enum JournalTabType {
        Info,       // Информация о корабле/игроке
        Inventory,  // Инвентарь
        Quests,     // Задания
        Characters, // Персонажи
        Map,        // Карта
        Drop        // Окно сброса (оно часть системы, хоть и вызывается отдельно)
    }
}