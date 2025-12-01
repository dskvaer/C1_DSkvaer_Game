using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // Подключаем пространство имен новой системы ввода

namespace Menu_Journal {
    public class JournalController : MonoBehaviour {
        [Header("Input Settings")]
        // Сюда в инспекторе нужно перетащить Action "OpenJournal" из твоего файла Input Actions
        [SerializeField] private InputActionReference _toggleMenuAction;

        [Header("UI Elements")]
        [SerializeField] private GameObject _menuRoot; // Весь визуальный объект меню
        [SerializeField] private JournalTabType _defaultTab = JournalTabType.Info;

        // Список всех вкладок, которые мы нашли внутри меню
        private List<IJournalTab> _allTabs = new List<IJournalTab>();
        private IJournalTab _currentTab;
        private bool _isOpen = false;

        private void Awake()
        {
            // Автоматически находим все вкладки внутри этого объекта при старте игры
            _allTabs.AddRange(GetComponentsInChildren<IJournalTab>(true));

            // Скрываем меню при старте
            CloseMenu();
        }

        // --- БЛОК НОВОЙ СИСТЕМЫ ВВОДА ---
        private void OnEnable()
        {
            // Включаем прослушивание кнопки при активации объекта
            if (_toggleMenuAction != null)
            {
                _toggleMenuAction.action.Enable();
                _toggleMenuAction.action.performed += OnToggleInput;
            }
        }

        private void OnDisable()
        {
            // Выключаем прослушивание, чтобы не было утечек памяти
            if (_toggleMenuAction != null)
            {
                _toggleMenuAction.action.performed -= OnToggleInput;
                _toggleMenuAction.action.Disable();
            }
        }

        // Этот метод вызывается автоматически системой ввода при нажатии J
        private void OnToggleInput(InputAction.CallbackContext context)
        {
            ToggleMenu();
        }
        // --------------------------------

        // Переключить состояние (Открыть/Закрыть)
        // Этот метод PUBLIC, поэтому его можно вызвать и с кнопки на экране Android
        public void ToggleMenu()
        {
            if (_isOpen)
                CloseMenu();
            else
                OpenMenu();
        }

        public void OpenMenu()
        {
            _isOpen = true;
            if (_menuRoot != null) _menuRoot.SetActive(true);

            // При открытии сразу показываем вкладку по умолчанию (или последнюю открытую)
            if (_currentTab == null)
            {
                SwitchTab(_defaultTab);
            }
            else
            {
                _currentTab.OnOpen();
            }
        }

        public void CloseMenu()
        {
            _isOpen = false;
            if (_menuRoot != null) _menuRoot.SetActive(false);

            // Сообщаем текущей вкладке, что она закрылась
            if (_currentTab != null)
                _currentTab.OnClose();
        }

        // ГЛАВНЫЙ МЕТОД: Переключение вкладок
        public void SwitchTab(JournalTabType targetType)
        {
            // 1. Закрываем старую вкладку
            if (_currentTab != null)
            {
                if (_currentTab.TabType == targetType) return; // Уже открыта
                _currentTab.OnClose();
            }

            // 2. Ищем новую вкладку в списке
            IJournalTab newTab = _allTabs.Find(t => t.TabType == targetType);

            if (newTab != null)
            {
                _currentTab = newTab;
                _currentTab.OnOpen();
            }
            else
            {
                // Пока у нас нет реальных вкладок, эта ошибка нормальна, мы её игнорируем на этом этапе
                // Debug.LogWarning($"Вкладка {targetType} не найдена!");
            }
        }

        // Метод-обертка для кнопок Unity UI (вкладок)
        public void SwitchTabByIndex(int tabIndex)
        {
            SwitchTab((JournalTabType)tabIndex);
        }
    }
}