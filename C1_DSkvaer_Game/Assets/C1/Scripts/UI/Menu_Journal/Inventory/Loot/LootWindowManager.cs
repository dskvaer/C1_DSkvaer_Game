using System.Collections.Generic;
using Menu_Journal.Data;
using Menu_Journal.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu_Journal.UI {
    public class LootWindowManager : MonoBehaviour {
        [Header("Данные и окна")]
        [InspectorLabel("База предметов")]
        [Tooltip("База предметов, по которой окно переводит ID из контейнера в данные предметов.")]
        [SerializeField] private ItemDatabaseSO _database;
        [InspectorLabel("Окно количества")]
        [Tooltip("Всплывающее окно выбора количества предметов для переноса.")]
        [SerializeField] private QuantityPopup _quantityPopup;
        [InspectorLabel("Универсальное окно")]
        [Tooltip("Окно подтверждений и уведомлений: взять все, сбросить все, отменить изменения.")]
        [SerializeField] private UniversalPopup _universalPopup;

        [Header("Основное окно")]
        [InspectorLabel("Корень окна")]
        [Tooltip("Главный GameObject окна дропа. Включается при открытии и выключается при закрытии.")]
        [SerializeField] private GameObject _windowRoot;

        [Header("Панель игрока")]
        [InspectorLabel("Инвентарь игрока")]
        [Tooltip("Инвентарь корабля игрока. Если пусто, будет найден автоматически.")]
        [SerializeField] private ShipInventory _playerShipInventory;
        [InspectorLabel("Сетка игрока")]
        [Tooltip("Контейнер UI-слотов инвентаря игрока.")]
        [SerializeField] private Transform _playerGridContent;
        [InspectorLabel("Заголовок игрока")]
        [Tooltip("Текст заголовка панели игрока с весом трюма.")]
        [SerializeField] private TextMeshProUGUI _playerTitleText;
        [InspectorLabel("Кнопка сбросить")]
        [Tooltip("Кнопка сброса выбранного предмета в контейнер.")]
        [SerializeField] private Button _dropButton;
        [InspectorLabel("Кнопка сбросить все")]
        [Tooltip("Кнопка сброса всего груза игрока в контейнер.")]
        [SerializeField] private Button _dropAllButton;

        [Header("Панель контейнера")]
        [InspectorLabel("Сетка контейнера")]
        [Tooltip("Контейнер UI-слотов открытого контейнера.")]
        [SerializeField] private Transform _containerGridContent;
        [InspectorLabel("Заголовок контейнера")]
        [Tooltip("Текст заголовка контейнера с ID и весом груза.")]
        [SerializeField] private TextMeshProUGUI _containerTitleText;
        [InspectorLabel("Кнопка забрать")]
        [Tooltip("Кнопка переноса выбранного предмета из контейнера игроку.")]
        [SerializeField] private Button _takeButton;
        [InspectorLabel("Кнопка забрать все")]
        [Tooltip("Кнопка переноса всего содержимого контейнера в трюм игрока.")]
        [SerializeField] private Button _takeAllButton;
        [InspectorLabel("Кнопка закрытия")]
        [Tooltip("Кнопка закрытия окна с отменой неподтвержденных изменений.")]
        [SerializeField] private Button _xButton;
        [InspectorLabel("Кнопка подтверждения")]
        [Tooltip("Кнопка подтверждения операций и закрытия окна.")]
        [SerializeField] private Button _okButton;

        [Header("Информационная панель")]
        [InspectorLabel("Название предмета")]
        [Tooltip("Текст названия выбранного предмета.")]
        [SerializeField] private TextMeshProUGUI _infoTitleText;
        [InspectorLabel("Описание предмета")]
        [Tooltip("Текст описания выбранного предмета.")]
        [SerializeField] private TextMeshProUGUI _infoBodyText;
        [InspectorLabel("Префаб слота")]
        [Tooltip("UI-префаб одной ячейки предмета.")]
        [SerializeField] private InventorySlotUI _slotPrefab;

        private LootContainer _currentContainer;
        private InventorySlotUI _selectedSlotUI;
        private bool _isSelectionFromPlayer;
        private bool _isDropMode;
        private bool _hasChanges;
        private readonly List<Transaction> _sessionTransactions = new();

        private float PlayerMaxWeight => _playerShipInventory != null ? _playerShipInventory.MaxWeight : 100f;

        private struct Transaction {
            public bool FromPlayerToContainer;
            public string ItemID;
            public int Amount;
        }

        private void Start()
        {
            _database?.Initialize();
            LootContainer.OnContainerInteracted += OpenForLooting;
            BindButtons();

            if (_windowRoot != null) {
                _windowRoot.SetActive(false);
            }

            ClosePopups();
        }

        private void OnDestroy()
        {
            LootContainer.OnContainerInteracted -= OpenForLooting;
        }

        public void OpenForDropping()
        {
            _isDropMode = true;
            _currentContainer = null;
            InitializeWindow();
        }

        private void OpenForLooting(LootContainer container)
        {
            _isDropMode = false;
            _currentContainer = container;
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            _playerShipInventory = _playerShipInventory != null ? _playerShipInventory : FindPlayerInventory();
            if (_playerShipInventory == null || _windowRoot == null) {
                Debug.LogError("[LootWindowManager] Player ShipInventory or WindowRoot is missing.", this);
                return;
            }

            _windowRoot.SetActive(true);
            _hasChanges = false;
            _sessionTransactions.Clear();
            ClearSelection();
            RefreshUI();
            UpdateWindowButtons();
        }

        private void BindButtons()
        {
            if (_xButton != null) _xButton.onClick.AddListener(OnCancelOrCloseClicked);
            if (_okButton != null) _okButton.onClick.AddListener(OnOkClicked);
            if (_takeButton != null) _takeButton.onClick.AddListener(OnTakeClicked);
            if (_takeAllButton != null) _takeAllButton.onClick.AddListener(OnTakeAllClicked);
            if (_dropButton != null) _dropButton.onClick.AddListener(OnDropClicked);
            if (_dropAllButton != null) _dropAllButton.onClick.AddListener(OnDropAllClicked);
        }

        private void ClosePopups()
        {
            if (_quantityPopup != null) _quantityPopup.gameObject.SetActive(false);
            if (_universalPopup != null) _universalPopup.gameObject.SetActive(false);
        }

        private void CreateContainerIfNeeded()
        {
            if (_currentContainer != null) {
                return;
            }

            if (LootSpawner.Instance == null) {
                Debug.LogError("[LootWindowManager] LootSpawner not found in scene.", this);
                return;
            }

            Vector3 fallbackPosition = _playerShipInventory != null ? _playerShipInventory.transform.position : transform.position;
            Vector3 spawnPos = PlayerInventorySystem.GetDropPosition(fallbackPosition);
            _currentContainer = LootSpawner.Instance.SpawnPlayerDrop(spawnPos);
        }

        private void OnDropClicked()
        {
            if (_selectedSlotUI == null || !_isSelectionFromPlayer) {
                return;
            }

            CreateContainerIfNeeded();
            if (_currentContainer == null) {
                return;
            }

            TransferWithPopup(_selectedSlotUI.SlotData, TransactionType.Drop, (item, amount) => TransferToContainer(item, amount), 9999f);
        }

        private void OnDropAllClicked()
        {
            if (_playerShipInventory == null || _playerShipInventory.CurrentWeight <= 0f || _universalPopup == null) {
                return;
            }

            _universalPopup.ShowQuestion("Сбросить все?", "Все содержимое трюма будет перемещено в контейнер.", () => {
                CreateContainerIfNeeded();
                if (_currentContainer == null) return;

                List<InventorySlot> toDrop = new();
                for (int i = 0; i < _playerShipInventory.SlotCount; i++) {
                    InventorySlot slot = _playerShipInventory.GetSlot(i);
                    if (slot != null && !slot.IsEmpty) {
                        toDrop.Add(new InventorySlot(slot.Item, slot.Quantity));
                    }
                }

                foreach (InventorySlot slot in toDrop) {
                    TransferToContainer(slot.Item, slot.Quantity, false);
                }

                RefreshUI();
                UpdateWindowButtons();
                ClearSelection();
            }, confirmLabel: "Сбросить", cancelLabel: "Нет");
        }

        private void OnTakeClicked()
        {
            if (_selectedSlotUI == null || _isSelectionFromPlayer || _currentContainer == null) {
                return;
            }

            TransferWithPopup(_selectedSlotUI.SlotData, TransactionType.Take, (item, amount) => TransferToPlayer(item, amount));
        }

        private void OnTakeAllClicked()
        {
            if (_currentContainer == null || _currentContainer.Inventory.GetTotalCount() == 0 || _universalPopup == null) {
                return;
            }

            float itemsWeight = CalculateContainerWeight(_currentContainer.Inventory);
            float finalWeight = _playerShipInventory.CurrentWeight + itemsWeight;
            string message = $"Вес: {itemsWeight:F1} кг.\nИтог: {finalWeight:F1}/{PlayerMaxWeight:F1} кг";
            if (finalWeight > PlayerMaxWeight) {
                message += "\n<color=red>Перегруз!</color>";
            }

            _universalPopup.ShowQuestion("Взять все?", message, () => {
                Dictionary<string, int> grouped = _currentContainer.Inventory.GetGroupedItems();
                foreach (KeyValuePair<string, int> pair in grouped) {
                    ItemDataSO item = _database.GetItem(pair.Key);
                    if (item != null) {
                        TransferToPlayer(item, pair.Value, false);
                    }
                }

                RefreshUI();
                UpdateWindowButtons();
                ClearSelection();

                if (!_isDropMode) {
                    CloseWindowAndCleanup();
                }
            }, confirmLabel: "Забрать", cancelLabel: "Нет");
        }

        private void TransferToPlayer(ItemDataSO item, int amount, bool updateUI = true)
        {
            if (!CanPlayerFitItem(item, amount)) {
                if (updateUI) {
                    _universalPopup?.ShowNotification("Нет места", "Недостаточно места или грузоподъемности.");
                }
                return;
            }

            if (_currentContainer.Inventory.RemoveItem(item.ID, amount)) {
                if (!_playerShipInventory.AddItem(item, amount)) {
                    _currentContainer.Inventory.AddItem(item.ID, amount);
                    return;
                }

                _hasChanges = true;
                _sessionTransactions.Add(new Transaction { FromPlayerToContainer = false, ItemID = item.ID, Amount = amount });

                if (updateUI) {
                    RefreshUI();
                    UpdateWindowButtons();
                    ClearSelection();
                }
            }
        }

        private void TransferToContainer(ItemDataSO item, int amount, bool updateUI = true)
        {
            if (_currentContainer == null || item == null || amount <= 0) {
                return;
            }

            _playerShipInventory.RemoveItem(item, amount);
            _currentContainer.Inventory.AddItem(item.ID, amount);
            _hasChanges = true;
            _sessionTransactions.Add(new Transaction { FromPlayerToContainer = true, ItemID = item.ID, Amount = amount });

            if (updateUI) {
                RefreshUI();
                UpdateWindowButtons();
                ClearSelection();
            }
        }

        private void TransferWithPopup(InventorySlot slotData, TransactionType type, System.Action<ItemDataSO, int> onConfirm, float maxWeight = -1f)
        {
            if (slotData == null || slotData.IsEmpty) {
                return;
            }

            if (slotData.Quantity == 1 || _quantityPopup == null) {
                onConfirm(slotData.Item, 1);
                return;
            }

            float currentLoad = type == TransactionType.Take ? _playerShipInventory.CurrentWeight : CalculateContainerWeight(_currentContainer.Inventory);
            float max = maxWeight > 0f ? maxWeight : PlayerMaxWeight;
            _quantityPopup.Open(slotData.Item, slotData.Quantity, currentLoad, max, type, amount => onConfirm(slotData.Item, amount));
        }

        private void RefreshUI()
        {
            DrawPlayerGrid();
            DrawContainerGrid();
        }

        private void DrawPlayerGrid()
        {
            ClearGrid(_playerGridContent);
            if (_playerShipInventory == null || _slotPrefab == null) {
                return;
            }

            for (int i = 0; i < _playerShipInventory.SlotCount; i++) {
                InventorySlot slot = _playerShipInventory.GetSlot(i);
                if (slot == null || slot.IsEmpty) {
                    continue;
                }

                InventorySlotUI ui = Instantiate(_slotPrefab, _playerGridContent);
                ui.Setup(slot);
                ui.OnClick += clicked => OnSlotClicked(clicked, true);
            }

            if (_playerTitleText != null) {
                _playerTitleText.text = $"Трюм ({_playerShipInventory.CurrentWeight:F1}/{PlayerMaxWeight:F1} кг)";
            }
        }

        private void DrawContainerGrid()
        {
            ClearGrid(_containerGridContent);
            if (_currentContainer == null || _slotPrefab == null || _database == null) {
                if (_containerTitleText != null) {
                    _containerTitleText.text = _isDropMode ? "Сброс (пусто)" : "Пусто";
                }
                return;
            }

            foreach (KeyValuePair<string, int> pair in _currentContainer.Inventory.GetGroupedItems()) {
                ItemDataSO item = _database.GetItem(pair.Key);
                if (item == null) {
                    continue;
                }

                InventorySlotUI ui = Instantiate(_slotPrefab, _containerGridContent);
                ui.Setup(new InventorySlot(item, pair.Value));
                ui.OnClick += clicked => OnSlotClicked(clicked, false);
            }

            if (_containerTitleText != null) {
                float weight = CalculateContainerWeight(_currentContainer.Inventory);
                _containerTitleText.text = $"{_currentContainer.DisplayName} [{_currentContainer.ContainerID}] | {weight:F1} кг";
            }
        }

        private void OnCancelOrCloseClicked()
        {
            if (!_hasChanges) {
                CloseWindowAndCleanup();
                return;
            }

            _universalPopup?.ShowQuestion("Отменить?", "Отменить все изменения?", () => {
                RollbackChanges();
                CloseWindowAndCleanup();
            }, confirmLabel: "Да", cancelLabel: "Нет");
        }

        private void OnOkClicked()
        {
            if (!_hasChanges) {
                CloseWindowAndCleanup();
                return;
            }

            _universalPopup?.ShowQuestion("Подтвердить?", "Завершить операции?", CloseWindowAndCleanup, confirmLabel: "ОК", cancelLabel: "Нет");
        }

        private void CloseWindowAndCleanup()
        {
            _currentContainer?.CheckIfEmpty();
            if (_windowRoot != null) {
                _windowRoot.SetActive(false);
            }

            _currentContainer = null;
            _hasChanges = false;
            _sessionTransactions.Clear();
            _isDropMode = false;
            ClosePopups();
        }

        private void RollbackChanges()
        {
            if (_currentContainer == null || _database == null || _playerShipInventory == null) {
                return;
            }

            for (int i = _sessionTransactions.Count - 1; i >= 0; i--) {
                Transaction transaction = _sessionTransactions[i];
                ItemDataSO item = _database.GetItem(transaction.ItemID);
                if (item == null) {
                    continue;
                }

                if (transaction.FromPlayerToContainer) {
                    _currentContainer.Inventory.RemoveItem(transaction.ItemID, transaction.Amount);
                    _playerShipInventory.AddItem(item, transaction.Amount);
                }
                else {
                    _playerShipInventory.RemoveItem(item, transaction.Amount);
                    _currentContainer.Inventory.AddItem(transaction.ItemID, transaction.Amount);
                }
            }

            RefreshUI();
        }

        private bool CanPlayerFitItem(ItemDataSO item, int amount)
        {
            return _playerShipInventory != null && _playerShipInventory.CanAddItem(item, amount);
        }

        private float CalculateContainerWeight(SimpleInventory inventory)
        {
            if (inventory == null || _database == null) {
                return 0f;
            }

            float total = 0f;
            foreach (KeyValuePair<string, int> pair in inventory.GetGroupedItems()) {
                ItemDataSO item = _database.GetItem(pair.Key);
                if (item != null) {
                    total += item.Weight * pair.Value;
                }
            }

            return total;
        }

        private void OnSlotClicked(InventorySlotUI ui, bool isPlayer)
        {
            if (_selectedSlotUI != null) {
                _selectedSlotUI.SetSelected(false);
            }

            _selectedSlotUI = ui;
            _isSelectionFromPlayer = isPlayer;
            _selectedSlotUI.SetSelected(true);

            if (_infoTitleText != null) _infoTitleText.text = ui.SlotData.Item.Name;
            if (_infoBodyText != null) _infoBodyText.text = ui.SlotData.Item.Description;
            UpdateWindowButtons();
        }

        private void UpdateWindowButtons()
        {
            if (_dropButton != null) _dropButton.interactable = _selectedSlotUI != null && _isSelectionFromPlayer;
            if (_takeButton != null) _takeButton.interactable = _selectedSlotUI != null && !_isSelectionFromPlayer && _currentContainer != null;
            if (_takeAllButton != null) _takeAllButton.interactable = _currentContainer != null && _currentContainer.Inventory.GetTotalCount() > 0;
            if (_dropAllButton != null) _dropAllButton.interactable = _playerShipInventory != null && _playerShipInventory.CurrentWeight > 0f;

            if (_okButton != null && _okButton.TryGetComponent(out Image image)) {
                image.color = _hasChanges ? Color.green : Color.white;
            }
        }

        private void ClearSelection()
        {
            _selectedSlotUI = null;
            _isSelectionFromPlayer = false;
            if (_infoTitleText != null) _infoTitleText.text = "...";
            if (_infoBodyText != null) _infoBodyText.text = string.Empty;
            UpdateWindowButtons();
        }

        private void ClearGrid(Transform root)
        {
            if (root == null) {
                return;
            }

            foreach (Transform child in root) {
                Destroy(child.gameObject);
            }
        }

        private ShipInventory FindPlayerInventory()
        {
            return PlayerInventorySystem.FindCargoInventory();
        }
    }
}
