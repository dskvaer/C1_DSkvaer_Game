using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Menu_Journal {
    public class InventoryTab : JournalTabBase {
        [Header("Inventory Grid")]
        [SerializeField] private Transform _slotsContainer;   // Контейнер для ячеек (Scroll View Content)
        [SerializeField] private InventorySlotUI _slotPrefab; // Префаб кнопки-ячейки

        [Header("Info Panel")]
        [SerializeField] private TextMeshProUGUI _infoTitleText;       // Заголовок (Название предмета или "Трюм")
        [SerializeField] private TextMeshProUGUI _infoDescriptionText; // Основной текст (внутри Scroll View)

        [Header("Actions")]
        [SerializeField] private Button _dropButton; // Кнопка "Сброс"

        private IItemContainer _connectedContainer;
        private InventorySlot _selectedSlot; // Текущая выбранная ячейка (null, если ничего не выбрано)
        private List<InventorySlotUI> _spawnedSlots = new List<InventorySlotUI>();

        private void Start()
        {
            // Исправлено предупреждение CS0618: используем FindFirstObjectByType
            var playerInventory = FindFirstObjectByType<ShipInventory>();

            if (playerInventory != null)
            {
                ConnectToContainer(playerInventory);
            }
            else
            {
                Debug.LogError("InventoryTab не нашла ShipInventory на сцене!");
            }

            // Настройка кнопки дропа (логику добавим позже)
            if (_dropButton != null)
            {
                _dropButton.onClick.AddListener(OnDropButtonClicked);
            }
        }

        public void ConnectToContainer(IItemContainer container)
        {
            _connectedContainer = container;
            _connectedContainer.OnInventoryUpdated += RefreshUI;
        }

        private void OnDestroy()
        {
            if (_connectedContainer != null)
            {
                _connectedContainer.OnInventoryUpdated -= RefreshUI;
            }
        }

        public override void OnOpen()
        {
            base.OnOpen();
            // При открытии сбрасываем выделение, чтобы показать общую сводку
            _selectedSlot = null;
            RefreshUI();
        }

        // Этот метод вызывается при клике на любую ячейку в сетке
        private void OnSlotClicked(InventorySlot slot)
        {
            if (slot == null || slot.IsEmpty) return;

            _selectedSlot = slot; // Запоминаем выбор
            UpdateInfoPanel();    // Обновляем только правую панель
        }

        private void OnDropButtonClicked()
        {
            Debug.Log("Открыть окно сброса (Drop Window)");
            // Тут позже будет вызов JournalController.SwitchTab(JournalTabType.Drop);
        }

        private void RefreshUI()
        {
            if (_connectedContainer == null) return;

            // 1. Очистка сетки
            foreach (Transform child in _slotsContainer)
            {
                Destroy(child.gameObject);
            }
            _spawnedSlots.Clear();

            // 2. Генерация ячеек
            // Мы проходим по всем слотам в "бэкенде" инвентаря
            for (int i = 0; i < _connectedContainer.SlotCount; i++)
            {
                InventorySlot slotData = _connectedContainer.GetSlot(i);

                // ВАЖНО: Если это валюта, мы не показываем её в сетке как предмет (она в кошельке)
                // Но если слот пустой - мы его показываем (чтобы видеть свободное место)
                if (!slotData.IsEmpty && slotData.Item.IsCurrency)
                {
                    continue; // Пропускаем визуализацию монет в сетке
                }

                // Создаем кнопку ячейки
                InventorySlotUI newSlotUI = Instantiate(_slotPrefab, _slotsContainer);
                newSlotUI.Setup(slotData);

                // Добавляем слушатель клика. 
                // Важно: создаем локальную копию переменной для замыкания, если нужно, но здесь slotData ссылочный.
                Button btn = newSlotUI.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => OnSlotClicked(slotData));
                }

                _spawnedSlots.Add(newSlotUI);
            }

            // 3. Обновляем правую панель (Инфо)
            UpdateInfoPanel();
        }

        // Логика отображения информации (Правый столбец)
        private void UpdateInfoPanel()
        {
            if (_infoTitleText == null || _infoDescriptionText == null) return;

            if (_selectedSlot != null && !_selectedSlot.IsEmpty)
            {
                // ВАРИАНТ А: ВЫБРАН ПРЕДМЕТ
                ShowItemDetails(_selectedSlot);
            }
            else
            {
                // ВАРИАНТ Б: НИЧЕГО НЕ ВЫБРАНО (ОБЩАЯ СВОДКА)
                ShowGeneralShipInfo();
            }
        }

        private void ShowItemDetails(InventorySlot slot)
        {
            _infoTitleText.text = slot.Item.Name;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<i>{slot.Item.Category.CategoryName} | {slot.Item.Rarity}</i>");
            sb.AppendLine();
            sb.AppendLine(slot.Item.Description);
            sb.AppendLine();
            sb.AppendLine($"<b>Вес:</b> {slot.Item.Weight} кг");
            sb.AppendLine($"<b>Кол-во:</b> {slot.Quantity} шт");
            sb.AppendLine($"<b>Цена за шт:</b> {FormatMoney(slot.Item.BasePrice)}");

            _infoDescriptionText.text = sb.ToString();
        }

        private void ShowGeneralShipInfo()
        {
            _infoTitleText.text = "Статус Трюма";

            StringBuilder sb = new StringBuilder();

            // 1. Вес
            float currentWeight = _connectedContainer.CurrentWeight;
            float maxWeight = _connectedContainer.MaxWeight;
            sb.AppendLine($"<b>Загрузка:</b> {currentWeight:F1} / {maxWeight:F1} кг");

            // 2. Деньги (Считаем сумму всех валют в инвентаре)
            int totalMoney = CalculateTotalMoney();
            sb.AppendLine($"<b>Кошелек:</b> {FormatMoney(totalMoney)}");
            sb.AppendLine("----------------");
            sb.AppendLine("<b>Груз на борту:</b>");

            // 3. Список товаров списком
            bool hasGoods = false;
            for (int i = 0; i < _connectedContainer.SlotCount; i++)
            {
                var slot = _connectedContainer.GetSlot(i);
                if (!slot.IsEmpty && !slot.Item.IsCurrency)
                {
                    sb.AppendLine($"- {slot.Item.Name}: {slot.Quantity} шт.");
                    hasGoods = true;
                }
            }

            if (!hasGoods)
            {
                sb.AppendLine("<i>Трюм пуст</i>");
            }

            _infoDescriptionText.text = sb.ToString();
        }

        private int CalculateTotalMoney()
        {
            int total = 0;
            for (int i = 0; i < _connectedContainer.SlotCount; i++)
            {
                var slot = _connectedContainer.GetSlot(i);
                // Если предмет помечен как Валюта, считаем его цену * количество
                if (!slot.IsEmpty && slot.Item.IsCurrency)
                {
                    total += slot.Item.BasePrice * slot.Quantity;
                }
            }
            return total;
        }

        // Конвертер цены (10050 -> 1 Gold, 0 Silver, 50 Copper)
        private string FormatMoney(int totalCopper)
        {
            int gold = totalCopper / 10000;
            int remainder = totalCopper % 10000;
            int silver = remainder / 100;
            int copper = remainder % 100;

            string result = "";
            if (gold > 0) result += $"{gold} <color=yellow>G</color> ";
            if (silver > 0) result += $"{silver} <color=grey>S</color> ";
            if (copper > 0 || result == "") result += $"{copper} <color=orange>C</color>";

            return result;
        }
    }
}