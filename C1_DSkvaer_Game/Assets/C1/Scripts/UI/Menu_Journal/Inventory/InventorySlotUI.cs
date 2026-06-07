using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu_Journal {
    public class InventorySlotUI : MonoBehaviour {
        [Header("UI слота")]
        [InspectorLabel("Иконка предмета")]
        [Tooltip("Image, в котором показывается иконка предмета.")]
        [SerializeField] private Image _iconImage;

        [InspectorLabel("Количество")]
        [Tooltip("TMP-текст с количеством предметов в стаке.")]
        [SerializeField] private TextMeshProUGUI _amountText;

        [InspectorLabel("Кнопка слота")]
        [Tooltip("Button, по нажатию на который выбирается этот слот.")]
        [SerializeField] private Button _button;

        [InspectorLabel("Рамка выбора")]
        [Tooltip("Image рамки, которая включается для выбранного слота.")]
        [SerializeField] private Image _selectionFrame;

        private InventorySlot _currentSlot;

        public event Action<InventorySlotUI> OnClick;
        public InventorySlot SlotData => _currentSlot;

        private void Start()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(HandleClick);
            }

            if (_selectionFrame) _selectionFrame.enabled = false;
        }

        public void Setup(InventorySlot slot)
        {
            _currentSlot = slot;

            if (slot == null || slot.IsEmpty)
            {
                ClearSlot();
            }
            else
            {
                DrawSlot(slot);
            }
        }

        private void DrawSlot(InventorySlot slot)
        {
            _iconImage.gameObject.SetActive(true);
            _iconImage.sprite = slot.Item.Icon;
            _button.interactable = true;
            _amountText.text = slot.Quantity > 1 ? slot.Quantity.ToString() : string.Empty;
        }

        private void ClearSlot()
        {
            _iconImage.gameObject.SetActive(false);
            _amountText.text = string.Empty;
            _button.interactable = false;
            if (_selectionFrame) _selectionFrame.enabled = false;
        }

        private void HandleClick()
        {
            OnClick?.Invoke(this);
        }

        public void SetSelected(bool isSelected)
        {
            if (_selectionFrame) _selectionFrame.enabled = isSelected;
        }
    }
}
