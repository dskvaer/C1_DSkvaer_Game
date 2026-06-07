using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Menu_Journal.Data;
using Menu_Journal.Systems;

namespace Menu_Journal.UI {
    public class InventoryPageController : MonoBehaviour {
        [Header("Ссылки на системы")]
        [InspectorLabel("База предметов")]
        [Tooltip("ItemDatabaseSO, из которой окно берет данные предметов по ID.")]
        [SerializeField] private ItemDatabaseSO _database;

        [InspectorLabel("Окно лута")]
        [Tooltip("Менеджер окна лута, который используется для режима выброса груза.")]
        [SerializeField] private LootWindowManager _lootWindowManager;

        [Header("UI элементы")]
        [InspectorLabel("Контейнер сетки")]
        [Tooltip("Transform, внутрь которого создаются UI-слоты предметов.")]
        [SerializeField] private Transform _gridContent;

        [InspectorLabel("Префаб слота")]
        [Tooltip("Префаб UI-слота, который отображает один предмет или стак.")]
        [SerializeField] private InventorySlotUI _slotPrefab;

        [InspectorLabel("Кнопка выброса")]
        [Tooltip("Кнопка, которая закрывает инвентарь и открывает окно выброса груза.")]
        [SerializeField] private Button _openDropModeButton;

        [Header("Информация о выбранном предмете")]
        [InspectorLabel("Текст названия")]
        [Tooltip("TMP-текст, куда выводится имя выбранного предмета.")]
        [SerializeField] private TextMeshProUGUI _itemNameText;

        [InspectorLabel("Текст описания")]
        [Tooltip("TMP-текст, куда выводится описание выбранного предмета.")]
        [SerializeField] private TextMeshProUGUI _itemDescriptionText;

        [InspectorLabel("Текст общего веса")]
        [Tooltip("TMP-текст, куда выводится суммарный вес груза игрока.")]
        [SerializeField] private TextMeshProUGUI _totalWeightText;

        private SimpleInventory _playerInventory;

        private void Start()
        {
            _playerInventory = PlayerInventorySystem.FindPersonalInventory();

            if (_openDropModeButton)
            {
                _openDropModeButton.onClick.AddListener(OnDropModeClicked);
            }
        }

        private void OnEnable()
        {
            RefreshInventory();
        }

        private void RefreshInventory()
        {
            if (_playerInventory == null) {
                _playerInventory = PlayerInventorySystem.FindPersonalInventory();
            }

            if (_playerInventory == null || _database == null) return;

            foreach (Transform child in _gridContent) Destroy(child.gameObject);

            Dictionary<string, int> grouped = _playerInventory.GetGroupedItems();
            float totalWeight = 0f;

            foreach (var kvp in grouped)
            {
                string id = kvp.Key;
                int count = kvp.Value;

                ItemDataSO data = _database.GetItem(id);
                if (data == null) continue;

                totalWeight += data.Weight * count;

                InventorySlotUI newSlot = Instantiate(_slotPrefab, _gridContent);
                newSlot.Setup(new InventorySlot(data, count));
                newSlot.OnClick += OnSlotClicked;
            }

            if (_totalWeightText) _totalWeightText.text = $"Вес груза: {totalWeight} кг";
        }

        private void OnSlotClicked(InventorySlotUI slotUI)
        {
            if (_itemNameText) _itemNameText.text = slotUI.SlotData.Item.Name;
            if (_itemDescriptionText) _itemDescriptionText.text = slotUI.SlotData.Item.Description;
        }

        private void OnDropModeClicked()
        {
            gameObject.SetActive(false);

            if (_lootWindowManager != null)
            {
                _lootWindowManager.OpenForDropping();
            }
            else
            {
                Debug.LogError("LootWindowManager не назначен в InventoryPageController!", this);
            }
        }
    }
}
