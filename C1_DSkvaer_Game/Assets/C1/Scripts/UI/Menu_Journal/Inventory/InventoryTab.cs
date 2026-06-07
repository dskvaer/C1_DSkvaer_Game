using System.Collections.Generic;
using System.Text;
using Menu_Journal.Systems;
using Menu_Journal.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu_Journal {
    public class InventoryTab : JournalTabBase {
        [Header("Связи")]
        [InspectorLabel("Окно дропа")]
        [Tooltip("Окно, которое открывается для сброса выбранного груза из инвентаря.")]
        [SerializeField] private LootWindowManager _lootWindowManager;

        [Header("Сетка")]
        [InspectorLabel("Контейнер слотов")]
        [Tooltip("Transform, внутрь которого создаются UI-слоты инвентаря.")]
        [SerializeField] private Transform _slotsContainer;
        [InspectorLabel("Префаб слота")]
        [Tooltip("UI-префаб одной ячейки инвентаря.")]
        [SerializeField] private InventorySlotUI _slotPrefab;

        [Header("Информация")]
        [InspectorLabel("Заголовок информации")]
        [Tooltip("Текст названия выбранного предмета или общей информации трюма.")]
        [SerializeField] private TextMeshProUGUI _infoTitleText;
        [InspectorLabel("Описание информации")]
        [Tooltip("Текст описания выбранного предмета или списка груза.")]
        [SerializeField] private TextMeshProUGUI _infoDescriptionText;

        [Header("Кнопки")]
        [InspectorLabel("Кнопка сброса")]
        [Tooltip("Открывает окно сброса груза.")]
        [SerializeField] private Button _dropButton;

        private IItemContainer _connectedContainer;
        private InventorySlot _selectedSlot;
        private readonly List<InventorySlotUI> _spawnedSlots = new();

        private void Start()
        {
            ShipInventory playerInventory = FindPlayerInventory();
            if (playerInventory != null) {
                ConnectToContainer(playerInventory);
            }
            else {
                Debug.LogError("[InventoryTab] Player cargo inventory was not found.", this);
            }

            if (_dropButton != null) {
                _dropButton.onClick.AddListener(OnDropButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (_connectedContainer != null) {
                _connectedContainer.OnInventoryUpdated -= RefreshUI;
            }
        }

        public void ConnectToContainer(IItemContainer container)
        {
            if (_connectedContainer != null) {
                _connectedContainer.OnInventoryUpdated -= RefreshUI;
            }

            _connectedContainer = container;
            if (_connectedContainer != null) {
                _connectedContainer.OnInventoryUpdated += RefreshUI;
            }
        }

        public override void OnOpen()
        {
            base.OnOpen();
            _selectedSlot = null;
            RefreshUI();
        }

        private void OnSlotClicked(InventorySlot slot)
        {
            if (slot == null || slot.IsEmpty) {
                return;
            }

            _selectedSlot = slot;
            UpdateInfoPanel();
        }

        private void OnDropButtonClicked()
        {
            if (_lootWindowManager == null) {
                Debug.LogError("[InventoryTab] LootWindowManager is not assigned.", this);
                return;
            }

            _lootWindowManager.OpenForDropping();
        }

        private void RefreshUI()
        {
            if (_connectedContainer == null || _slotsContainer == null || _slotPrefab == null) {
                return;
            }

            foreach (Transform child in _slotsContainer) {
                Destroy(child.gameObject);
            }

            _spawnedSlots.Clear();

            for (int i = 0; i < _connectedContainer.SlotCount; i++) {
                InventorySlot slotData = _connectedContainer.GetSlot(i);
                InventorySlotUI slotUI = Instantiate(_slotPrefab, _slotsContainer);
                slotUI.Setup(slotData);

                Button button = slotUI.GetComponent<Button>();
                if (button != null) {
                    button.onClick.AddListener(() => OnSlotClicked(slotData));
                }

                _spawnedSlots.Add(slotUI);
            }

            UpdateInfoPanel();
        }

        private void UpdateInfoPanel()
        {
            if (_infoTitleText == null || _infoDescriptionText == null) {
                return;
            }

            if (_selectedSlot != null && !_selectedSlot.IsEmpty) {
                ShowItemDetails(_selectedSlot);
            }
            else {
                ShowGeneralShipInfo();
            }
        }

        private void ShowItemDetails(InventorySlot slot)
        {
            _infoTitleText.text = slot.Item.Name;
            var builder = new StringBuilder();
            string rarityColor = slot.Item.IsCurrency ? "<color=yellow>" : "";
            string rarityEnd = slot.Item.IsCurrency ? "</color>" : "";

            builder.AppendLine($"<i>{slot.Item.Category.CategoryName} | {rarityColor}{slot.Item.Rarity}{rarityEnd}</i>");
            builder.AppendLine();
            builder.AppendLine(slot.Item.Description);
            builder.AppendLine();
            builder.AppendLine($"<b>Вес всего:</b> {slot.Item.Weight * slot.Quantity:F1} кг");
            builder.AppendLine($"<b>Кол-во:</b> {slot.Quantity} шт");

            if (!slot.Item.IsCurrency) {
                builder.AppendLine($"<b>Цена за шт:</b> {slot.Item.BasePrice}");
            }

            _infoDescriptionText.text = builder.ToString();
        }

        private void ShowGeneralShipInfo()
        {
            if (_connectedContainer == null) {
                return;
            }

            _infoTitleText.text = "Трюм корабля";
            var builder = new StringBuilder();
            builder.AppendLine($"<b>Загрузка:</b> {_connectedContainer.CurrentWeight:F1} / {_connectedContainer.MaxWeight:F1} кг");
            builder.AppendLine("----------------");
            builder.AppendLine("<b>Кошелек:</b>");

            var wallet = new Dictionary<ItemDataSO, int>();
            for (int i = 0; i < _connectedContainer.SlotCount; i++) {
                InventorySlot slot = _connectedContainer.GetSlot(i);
                if (slot != null && !slot.IsEmpty && slot.Item.IsCurrency) {
                    wallet[slot.Item] = wallet.TryGetValue(slot.Item, out int count) ? count + slot.Quantity : slot.Quantity;
                }
            }

            if (wallet.Count == 0) {
                builder.AppendLine("<i>Пусто</i>");
            }
            else {
                foreach (KeyValuePair<ItemDataSO, int> pair in wallet) {
                    builder.AppendLine($"{pair.Key.Name}: {pair.Value}");
                }
            }

            builder.AppendLine("----------------");
            builder.AppendLine("<b>Груз:</b>");
            bool hasGoods = false;
            for (int i = 0; i < _connectedContainer.SlotCount; i++) {
                InventorySlot slot = _connectedContainer.GetSlot(i);
                if (slot != null && !slot.IsEmpty && !slot.Item.IsCurrency) {
                    builder.AppendLine($"- {slot.Item.Name}: {slot.Quantity}");
                    hasGoods = true;
                }
            }

            if (!hasGoods) {
                builder.AppendLine("<i>Нет товаров</i>");
            }

            _infoDescriptionText.text = builder.ToString();
        }

        private ShipInventory FindPlayerInventory()
        {
            return PlayerInventorySystem.FindCargoInventory();
        }
    }
}
