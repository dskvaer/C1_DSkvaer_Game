using UnityEngine;

namespace Menu_Journal {
    // Это базовый класс. В будущем мы унаследуем от него InventoryTab или MapTab, 
    // если нам понадобится сложная логика при открытии.
    // А пока он просто включает и выключает объект.

    public class JournalTabBase : MonoBehaviour, IJournalTab {
        [SerializeField] private JournalTabType _tabType; // Выбираем в инспекторе, какая это вкладка

        // Реализация интерфейса IJournalTab
        public JournalTabType TabType => _tabType;

        public virtual void OnOpen()
        {
            // Логика при открытии (например, обновить список товаров)
            gameObject.SetActive(true);
        }

        public virtual void OnClose()
        {
            // Логика при закрытии
            gameObject.SetActive(false);
        }
    }
}