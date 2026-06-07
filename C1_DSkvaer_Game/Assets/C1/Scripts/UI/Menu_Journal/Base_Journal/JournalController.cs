using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Menu_Journal {
    public class JournalController : MonoBehaviour {
        [Header("Input Settings")]
        [SerializeField] private InputActionReference _toggleMenuAction;

        [Header("UI Elements")]
        [SerializeField] private GameObject _menuRoot;
        [SerializeField] private JournalTabType _defaultTab = JournalTabType.Info;

        private List<IJournalTab> _allTabs = new List<IJournalTab>();
        private IJournalTab _currentTab;
        private bool _isOpen = false;

        private void Awake()
        {
            // Находим все вкладки (даже выключенные)
            _allTabs.AddRange(GetComponentsInChildren<IJournalTab>(true));

            // ВАЖНОЕ ИСПРАВЛЕНИЕ:
            // Сразу выключаем все вкладки визуально, чтобы они не накладывались друг на друга
            foreach (var tab in _allTabs)
            {
                // Трюк: приводим интерфейс к MonoBehaviour, чтобы выключить GameObject
                if (tab is MonoBehaviour tabMono)
                {
                    tabMono.gameObject.SetActive(false);
                }
            }

            CloseMenu();
        }

        private void OnEnable()
        {
            if (_toggleMenuAction != null)
            {
                _toggleMenuAction.action.Enable();
                _toggleMenuAction.action.performed += OnToggleInput;
            }
        }

        private void OnDisable()
        {
            if (_toggleMenuAction != null)
            {
                _toggleMenuAction.action.performed -= OnToggleInput;
                _toggleMenuAction.action.Disable();
            }
        }

        private void OnToggleInput(InputAction.CallbackContext context)
        {
            ToggleMenu();
        }

        public void ToggleMenu()
        {
            if (_isOpen) CloseMenu();
            else OpenMenu();
        }

        public void OpenMenu()
        {
            _isOpen = true;
            if (_menuRoot != null) _menuRoot.SetActive(true);

            // Если вкладка еще не выбрана, открываем дефолтную
            if (_currentTab == null)
            {
                SwitchTab(_defaultTab);
            }
            else
            {
                // Если была выбрана, открываем её снова
                _currentTab.OnOpen();
            }
        }

        public void CloseMenu()
        {
            _isOpen = false;
            if (_menuRoot != null) _menuRoot.SetActive(false);

            if (_currentTab != null)
                _currentTab.OnClose();
        }

        public void SwitchTab(JournalTabType targetType)
        {
            // 1. Закрываем старую вкладку
            if (_currentTab != null)
            {
                // Если нажали на ту же самую кнопку, ничего не делаем
                if (_currentTab.TabType == targetType && _currentTab is MonoBehaviour mono && mono.gameObject.activeSelf)
                    return;

                _currentTab.OnClose();
            }

            // 2. Ищем новую
            IJournalTab newTab = _allTabs.Find(t => t.TabType == targetType);

            if (newTab != null)
            {
                _currentTab = newTab;
                _currentTab.OnOpen();
            }
            else
            {
                Debug.LogWarning($"Вкладка {targetType} не найдена в списке дочерних объектов JournalController!");
            }
        }

        // Этот метод нужно назначить кнопкам вкладок в Inspector (OnClick)
        public void SwitchTabByIndex(int tabIndex)
        {
            SwitchTab((JournalTabType)tabIndex);
        }
    }
}