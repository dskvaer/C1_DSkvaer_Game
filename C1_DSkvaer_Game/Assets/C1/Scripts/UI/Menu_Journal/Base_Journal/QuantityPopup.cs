using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Menu_Journal.UI {
    // Тип действия, чтобы менять заголовки
    public enum TransactionType { Drop, Sell, Buy, Transfer, Take }

    // =================================================================================================
    // ИНСТРУКЦИЯ ПО НАСТРОЙКЕ:
    // 1. Создайте панель Popup_Quantity.
    // 2. Добавьте слайдер, кнопки +/-, Max, Confirm, Cancel.
    // 3. Добавьте тексты для Заголовка, Количества и Инфо (вес/цена).
    // 4. Повесьте этот скрипт и заполните ссылки.
    // =================================================================================================
    public class QuantityPopup : MonoBehaviour {
        [Header("--- ЭЛЕМЕНТЫ УПРАВЛЕНИЯ ---")]
        [SerializeField] private Slider _amountSlider;
        [SerializeField] private Button _btnMinus;
        [SerializeField] private Button _btnPlus;
        [SerializeField] private Button _btnMax;     // Кнопка "Всё"
        [SerializeField] private Button _btnConfirm; // Кнопка "Принять"
        [SerializeField] private Button _btnCancel;  // Кнопка "Отмена"

        [Header("--- ОТОБРАЖЕНИЕ ---")]
        [SerializeField] private TextMeshProUGUI _titleText;   // "Взять: Ром"
        [SerializeField] private TextMeshProUGUI _amountText;  // "5 шт"
        [SerializeField] private TextMeshProUGUI _infoText;    // "Вес: 10кг" (тут будет краснеть при перегрузе)
        [SerializeField] private Image _itemIcon;              // Иконка предмета

        // Внутренние переменные
        private ItemDataSO _currentItem;
        private int _currentAmount;
        private float _currentInventoryWeight; // Сколько веса УЖЕ в инвентаре
        private float _maxInventoryWeight;     // Максимальный вес корабля

        private Action<int> _onConfirmCallback;
        private TransactionType _currentType;

        private void Awake()
        {
            // Подписываемся на события кнопок и слайдера
            _btnMinus.onClick.AddListener(OnMinusClicked);
            _btnPlus.onClick.AddListener(OnPlusClicked);
            _btnMax.onClick.AddListener(OnMaxClicked);
            _btnConfirm.onClick.AddListener(OnConfirmClicked);
            _btnCancel.onClick.AddListener(Close);

            _amountSlider.onValueChanged.AddListener(OnSliderChanged);

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Открывает окно выбора количества.
        /// </summary>
        /// <param name="item">Предмет, с которым работаем</param>
        /// <param name="maxAvailable">Сколько всего таких предметов есть в пачке</param>
        /// <param name="currentLoad">Текущая загрузка того инвентаря, КУДА кладем</param>
        /// <param name="maxLoad">Максимальная грузоподъемность того инвентаря, КУДА кладем</param>
        /// <param name="type">Тип действия (Взять, Сбросить...)</param>
        /// <param name="onConfirm">Функция, которую вызвать при нажатии ОК</param>
        public void Open(ItemDataSO item, int maxAvailable, float currentLoad, float maxLoad, TransactionType type, Action<int> onConfirm)
        {
            _currentItem = item;
            _currentInventoryWeight = currentLoad;
            _maxInventoryWeight = maxLoad;
            _currentType = type;
            _onConfirmCallback = onConfirm;

            // Настройка слайдера
            _amountSlider.minValue = 1;
            _amountSlider.maxValue = maxAvailable;
            _amountSlider.value = 1;
            _currentAmount = 1;

            // Визуал
            if (_itemIcon) _itemIcon.sprite = item.Icon;
            _titleText.text = GetTitlePrefix(type) + item.Name;

            UpdateUI(); // Обновляем текст и проверки веса
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        // --- ЛОГИКА ИНТЕРФЕЙСА ---

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
            _onConfirmCallback?.Invoke(_currentAmount);
            Close();
        }

        /// <summary>
        /// Главный метод обновления текста. Тут проверяем перегруз.
        /// </summary>
        private void UpdateUI()
        {
            _amountText.text = $"{_currentAmount} шт";

            // Рассчитываем вес выбранного кол-ва
            float selectedWeight = _currentItem.Weight * _currentAmount;

            // Предсказываем финальный вес
            float predictedTotalWeight = _currentInventoryWeight + selectedWeight;

            // Формируем строку информации
            string infoString = "";
            bool isOverweight = false;

            if (_currentType == TransactionType.Take || _currentType == TransactionType.Transfer)
            {
                // Если мы берем предмет, нам важно, влезет ли он
                infoString = $"Вес груза: {selectedWeight:F1} кг\n";
                infoString += $"Итог: {predictedTotalWeight:F1} / {_maxInventoryWeight:F1} кг";

                // Если итог больше макс веса - помечаем как перегруз
                if (predictedTotalWeight > _maxInventoryWeight)
                {
                    isOverweight = true;
                }
            }
            else // Для продажи или сброса перегруз не так важен (мы освобождаем место)
            {
                infoString = $"Вес: {selectedWeight:F1} кг";
            }

            // Применяем текст и цвет
            _infoText.text = infoString;

            if (isOverweight)
            {
                _infoText.color = Color.red; // КРАСНЫЙ если перегруз
            }
            else
            {
                _infoText.color = Color.white; // Или стандартный цвет
            }
        }

        private string GetTitlePrefix(TransactionType type)
        {
            switch (type)
            {
                case TransactionType.Drop: return "Сбросить: ";
                case TransactionType.Sell: return "Продать: ";
                case TransactionType.Buy: return "Купить: ";
                case TransactionType.Take: return "Взять: ";
                default: return "";
            }
        }
    }
}