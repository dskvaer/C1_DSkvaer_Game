using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System; // Нужно для Action

namespace Menu_Journal {
    public class InventorySlotUI : MonoBehaviour {
        [Header("UI Components")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private Button _button;
        [SerializeField] private Image _selectionFrame; // Рамка выделения (опционально)

        private InventorySlot _currentSlot;

        // Событие: "Меня кликнули". Передает сам слот.
        public event Action<InventorySlotUI> OnClick;

        // Публичное свойство, чтобы менеджер мог прочитать, какой это слот
        public InventorySlot SlotData => _currentSlot;

        private void Start()
        {
            // Подписываем кнопку на наш метод
            if (_button != null)
            {
                _button.onClick.AddListener(HandleClick);
            }

            // Скрываем рамку выделения по умолчанию
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

            if (slot.Quantity > 1)
                _amountText.text = slot.Quantity.ToString();
            else
                _amountText.text = "";
        }

        private void ClearSlot()
        {
            _iconImage.gameObject.SetActive(false);
            _amountText.text = "";
            _button.interactable = false;
            if (_selectionFrame) _selectionFrame.enabled = false;
        }

        // Вызывается при нажатии на кнопку Unity Button
        private void HandleClick()
        {
            // Вызываем событие, чтобы менеджер (Лут или Журнал) узнал о клике
            OnClick?.Invoke(this);
        }

        // Методы для выделения (визуал)
        public void SetSelected(bool isSelected)
        {
            if (_selectionFrame) _selectionFrame.enabled = isSelected;
        }
    }
}