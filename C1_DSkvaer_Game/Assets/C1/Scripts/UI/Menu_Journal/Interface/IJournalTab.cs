namespace Menu_Journal {
    // Этот интерфейс мы повесим на каждый префаб вкладки (InventoryView, MapView и т.д.)
    public interface IJournalTab {
        // Какая это вкладка? (Инвентарь, Карта...)
        JournalTabType TabType { get; }

        // Метод, который вызовется, когда игрок нажмет кнопку этой вкладки
        void OnOpen();

        // Метод, который вызовется, когда игрок переключится на другую вкладку
        void OnClose();
    }
}