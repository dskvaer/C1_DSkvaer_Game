using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Menu_Journal {
    // Тип транзакции определяет, какую информацию показывать игроку
    public enum TransactionType {
        Drop,       // Сброс (Показываем Вес)
        Sell,       // Продажа (Показываем Прибыль)
        Buy,        // Покупка (Показываем Затраты)
        Transfer    // Просто перекладывание (Показываем Вес)
    }

    public class QuantityPopup : MonoBehaviour {
        [Header("Controls")]
        [SerializeField] private Slider _amountSlider;
        [SerializeField] private Button _btnMinus;
        [SerializeField] private Button _btnPlus;
        [SerializeField] private Button _btnConfirm;
        [SerializeField] private Button _btnCancel;
        [SerializeField] private Button _btnMax; // Кнопка "Всё"

        [Header("Display")]
        [SerializeField] private TextMeshProUGUI _titleText;      // "Сбросить: Ром"
        [SerializeField] private TextMeshProUGUI _amountText;     // "5 шт"
        [SerializeField] private TextMeshProUGUI _infoText;       // "Вес: 10кг" или "Цена: 500G"
        [SerializeField] private Image _itemIcon;

        private ItemDataSO _currentItem;
        private int _currentAmount;
        private Action<int> _onConfirmCallback;
        private TransactionType _currentType;

        private void Awake()
        {
            // Подписка на кнопки
            _btnMinus.onClick.AddListener(OnMinusClicked);
            _btnPlus.onClick.AddListener(OnPlusClicked);
            _btnConfirm.onClick.AddListener(OnConfirmClicked);
            _btnCancel.onClick.AddListener(Close);
            _btnMax.onClick.AddListener(OnMaxClicked);

            _amountSlider.onValueChanged.AddListener(OnSliderChanged);

            // Скрываем при старте
            gameObject.SetActive(false);
        }

        // ГЛАВНЫЙ МЕТОД ВЫЗОВА ОКНА
        // maxAvailable - сколько всего есть у игрока
        // onConfirm - функция, которая выполнится, когда игрок нажмет "ОК"
        public void Open(ItemDataSO item, int maxAvailable, TransactionType type, Action<int> onConfirm)
        {
            _currentItem = item;
            _onConfirmCallback = onConfirm;
            _currentType = type;

            // Настройка слайдера
            _amountSlider.minValue = 1;
            _amountSlider.maxValue = maxAvailable;
            _amountSlider.value = 1; // Сброс на 1
            _currentAmount = 1;

            // Настройка визуала
            _itemIcon.sprite = item.Icon;
            _titleText.text = GetTitlePrefix(type) + item.Name;

            UpdateUI();
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private string GetTitlePrefix(TransactionType type)
        {
            switch (type)
            {
                case TransactionType.Drop: return "Сбросить: ";
                case TransactionType.Sell: return "Продать: ";
                case TransactionType.Buy: return "Купить: ";
                case TransactionType.Transfer: return "Переложить: ";
                default: return "";
            }
        }

        // --- ЛОГИКА КНОПОК ---

        private void OnSliderChanged(float value)
        {
            _currentAmount = Mathf.RoundToInt(value);
            UpdateUI();
        }

        private void OnMinusClicked()
        {
            if (_currentAmount > 1)
            {
                _currentAmount--;
                _amountSlider.value = _currentAmount;
            }
        }

        private void OnPlusClicked()
        {
            if (_currentAmount < _amountSlider.maxValue)
            {
                _currentAmount++;
                _amountSlider.value = _currentAmount;
            }
        }

        private void OnMaxClicked()
        {
            _currentAmount = (int)_amountSlider.maxValue;
            _amountSlider.value = _currentAmount;
        }

        private void OnConfirmClicked()
        {
            // Вызываем колбэк и передаем выбранное число тому, кто открыл окно
            _onConfirmCallback?.Invoke(_currentAmount);
            Close();
        }

        // --- ОБНОВЛЕНИЕ ТЕКСТА ---

        private void UpdateUI()
        {
            _amountText.text = $"{_currentAmount}";

            // Динамическая информация внизу
            switch (_currentType)
            {
                case TransactionType.Drop:
                case TransactionType.Transfer:
                    float totalWeight = _currentItem.Weight * _currentAmount;
                    _infoText.text = $"Вес: {totalWeight:F1} кг";
                    break;

                case TransactionType.Sell:
                case TransactionType.Buy:
                    int totalPrice = _currentItem.BasePrice * _currentAmount;
                    _infoText.text = $"Сумма: {FormatMoney(totalPrice)}";
                    break;
            }
        }

        // Утилита для форматирования денег (можно вынести в отдельный Helper класс)
        private string FormatMoney(int totalCopper)
        {
            int gold = totalCopper / 10000;
            int silver = (totalCopper % 10000) / 100;
            int copper = totalCopper % 100;

            string result = "";
            if (gold > 0) result += $"{gold}<color=yellow>G</color> ";
            if (silver > 0) result += $"{silver}<color=grey>S</color> ";
            if (copper > 0 || result == "") result += $"{copper}<color=orange>C</color>";
            return result;
        }
    }
}