using TMPro; // Обязательно для работы с текстом
using UnityEngine;
using UnityEngine.UI;

namespace Menu_Journal {
    public class InventorySlotUI : MonoBehaviour {
        [Header("UI Components")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private Button _button; // Чтобы можно было нажать на ячейку

        private InventorySlot _currentSlot;

        // Метод, который вызовет "Вкладка", чтобы настроить эту ячейку
        public void Setup(InventorySlot slot)
        {
            _currentSlot = slot;

            if (slot == null || slot.IsEmpty)
            {
                // Если слот пустой - скрываем иконку и текст
                _iconImage.gameObject.SetActive(false);
                _amountText.text = "";
                _button.interactable = false;
            }
            else
            {
                // Если есть предмет - показываем
                _iconImage.gameObject.SetActive(true);
                _iconImage.sprite = slot.Item.Icon;
                _button.interactable = true;

                // Показываем количество только если больше 1
                if (slot.Quantity > 1)
                    _amountText.text = slot.Quantity.ToString();
                else
                    _amountText.text = "";
            }
        }
    }
}