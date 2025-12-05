using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Menu_Journal; // Пространство имен твоего инвентаря

namespace Menu_Journal.UI {
    public class LootWindowManager : MonoBehaviour {
        [Header("Data Base")]
        [Tooltip("Список всех предметов игры для поиска по ID")]
        [SerializeField] private List<ItemDataSO> _allGameItems;

        [Header("UI References")]
        [SerializeField] private GameObject _windowRoot;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _takeAllButton;

        [Header("Grids")]
        [SerializeField] private Transform _playerGridContent;
        [SerializeField] private Transform _containerGridContent;
        [SerializeField] private InventorySlotUI _slotPrefab; // ТЕПЕРЬ ИСПОЛЬЗУЕМ ТВОЙ ПРЕФАБ

        [Header("Info Panel")]
        [SerializeField] private Text _infoText; // Вес и имя
        [SerializeField] private Button _actionButton; // Взять / Положить
        [SerializeField] private Text _actionButtonText;

        // Состояние
        private LootContainer _currentContainer;
        private SimpleInventory _playerInventory;

        // Текущий выделенный слот (чтобы знать, что забирать)
        private InventorySlotUI _selectedSlotUI;
        private bool _isSelectionFromPlayer; // Откуда выбран слот?

        private void Start()
        {
            LootContainer.OnContainerInteracted += OpenWindow;
            _closeButton.onClick.AddListener(CloseWindow);
            _takeAllButton.onClick.AddListener(OnTakeAllClicked);
            _actionButton.onClick.AddListener(OnActionClicked);

            _windowRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            LootContainer.OnContainerInteracted -= OpenWindow;
        }

        private void OpenWindow(LootContainer container)
        {
            _currentContainer = container;

            // ИСПРАВЛЕНИЕ: Используем новый метод FindFirstObjectByType вместо устаревшего FindObjectOfType
            var player = FindFirstObjectByType<PlayerController>();
            _playerInventory = player != null ? player.GetComponent<SimpleInventory>() : null;

            // Внимание: поиск через Find - это временное решение. 
            // В идеале ссылку на игрока нужно передавать через Singleton или EventManager.

            _windowRoot.SetActive(true);
            RefreshUI();
            ClearSelection();
        }

        private void CloseWindow()
        {
            _windowRoot.SetActive(false);
            _currentContainer = null;
        }

        private void RefreshUI()
        {
            if (_currentContainer == null) return;

            // 1. Рисуем содержимое ящика
            DrawInventory(_currentContainer.Inventory, _containerGridContent, false);

            // 2. Рисуем инвентарь игрока (если есть)
            if (_playerInventory != null)
            {
                DrawInventory(_playerInventory, _playerGridContent, true);
            }
        }

        // Универсальный метод отрисовки для SimpleInventory
        private void DrawInventory(SimpleInventory inventory, Transform gridRoot, bool isPlayerSide)
        {
            // Очистка
            foreach (Transform child in gridRoot) Destroy(child.gameObject);

            // Группируем ID предметов (string -> count)
            Dictionary<string, int> grouped = inventory.GetGroupedItems();

            foreach (var kvp in grouped)
            {
                string id = kvp.Key;
                int count = kvp.Value;

                // Находим ItemDataSO по ID
                ItemDataSO data = _allGameItems.Find(x => x.ID == id);
                if (data == null) continue;

                // Создаем ВРЕМЕННЫЙ слот данных для UI.
                // Это связывает простую систему строк (SimpleInventory) с UI системой слотов (InventorySlotUI).
                InventorySlot tempSlotData = new InventorySlot(data, count);

                // Спавним префаб
                InventorySlotUI slotUI = Instantiate(_slotPrefab, gridRoot);
                slotUI.Setup(tempSlotData);

                // ПОДПИСЫВАЕМСЯ НА КЛИК
                // Используем лямбду, чтобы передать контекст (сторона игрока или ящика)
                slotUI.OnClick += (clickedSlot) => OnSlotClicked(clickedSlot, isPlayerSide);
            }
        }

        // Обработка клика
        private void OnSlotClicked(InventorySlotUI clickedUI, bool isPlayerSide)
        {
            // Снимаем выделение с предыдущего
            if (_selectedSlotUI != null) _selectedSlotUI.SetSelected(false);

            _selectedSlotUI = clickedUI;
            _isSelectionFromPlayer = isPlayerSide;

            _selectedSlotUI.SetSelected(true); // Включаем рамку

            UpdateInfoPanel(clickedUI.SlotData, isPlayerSide);
        }

        private void UpdateInfoPanel(InventorySlot slotData, bool isPlayerSide)
        {
            _infoText.text = $"{slotData.Item.Name}\nВес: {slotData.Item.Weight} кг";

            if (isPlayerSide)
            {
                _actionButtonText.text = "Положить";
                _actionButton.interactable = false; // Пока нельзя класть обратно (по ТЗ)
            }
            else
            {
                _actionButtonText.text = "Взять";
                _actionButton.interactable = true;
            }
        }

        private void OnActionClicked()
        {
            if (_selectedSlotUI == null || _currentContainer == null) return;

            if (!_isSelectionFromPlayer) // Забираем ИЗ ящика
            {
                string itemId = _selectedSlotUI.SlotData.Item.ID;

                // Удаляем из ящика
                if (_currentContainer.Inventory.RemoveItem(itemId, 1))
                {
                    // Добавляем игроку
                    _playerInventory.AddItem(itemId, 1);

                    RefreshUI();
                    ClearSelection();
                    _currentContainer.CheckIfEmpty();
                    if (_currentContainer == null) CloseWindow();
                }
            }
        }

        private void OnTakeAllClicked()
        {
            if (_currentContainer == null) return;

            // Копируем список, чтобы безопасно итерировать
            var itemsCopy = new List<string>(_currentContainer.Inventory.items);

            foreach (var id in itemsCopy)
            {
                _currentContainer.Inventory.RemoveItem(id, 1);
                _playerInventory.AddItem(id, 1);
            }

            _currentContainer.CheckIfEmpty();
            RefreshUI();
            if (_currentContainer == null) CloseWindow();
        }

        private void ClearSelection()
        {
            _selectedSlotUI = null;
            _infoText.text = "Выберите предмет";
            _actionButton.interactable = false;
        }
    }
}