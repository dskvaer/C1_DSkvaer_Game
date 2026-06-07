using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Menu_Journal.UI {
    // =================================================================================================
    // ИНСТРУКЦИЯ ПО НАСТРОЙКЕ В ИНСПЕКТОРЕ:
    // 1. Создайте в Canvas панель (Panel), назовите UniversalPopup.
    // 2. Внутри создайте Title (Text), Body (Text) и 3 кнопки: Confirm, Cancel, Alt.
    // 3. Повесьте этот скрипт на корневую панель.
    // 4. Перетащите ссылки в соответствующие поля скрипта.
    // 5. Сделайте из этого ПРЕФАБ или оставьте на сцене выключенным (галочка Active выкл).
    // =================================================================================================

    public class UniversalPopup : MonoBehaviour {
        [Header("--- ТЕКСТЫ ---")]
        [SerializeField] private TextMeshProUGUI _titleText; // Заголовок (например "Внимание")
        [SerializeField] private TextMeshProUGUI _bodyText;  // Основной текст (например "Вы уверены?")

        [Header("--- КНОПКИ ---")]
        [SerializeField] private Button _btnConfirm; // Кнопка "Да" / "Подтвердить"
        [SerializeField] private TextMeshProUGUI _btnConfirmText; // Текст на кнопке (чтобы менять "Да" на "ОК")

        [SerializeField] private Button _btnCancel;  // Кнопка "Нет" / "Отмена"
        [SerializeField] private TextMeshProUGUI _btnCancelText;

        [SerializeField] private Button _btnAlt;     // Дополнительная кнопка (редко нужна, но пусть будет)

        // Сюда мы запомним действие, которое нужно выполнить при нажатии "Да"
        private Action _onConfirmAction;
        private Action _onCancelAction;

        private void Awake()
        {
            // Подписываем кнопки на методы
            if (_btnConfirm) _btnConfirm.onClick.AddListener(OnConfirmClicked);
            if (_btnCancel) _btnCancel.onClick.AddListener(OnCancelClicked);

            // Скрываем окно при старте игры
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Главный метод вызова окна.
        /// </summary>
        /// <param name="title">Заголовок окна</param>
        /// <param name="message">Текст сообщения</param>
        /// <param name="onConfirm">Функция, которая сработает при нажатии "Да"</param>
        /// <param name="onCancel">Функция, которая сработает при нажатии "Нет" (необязательно)</param>
        /// <param name="confirmLabel">Надпись на кнопке подтверждения (по умолчанию "ОК")</param>
        public void ShowQuestion(string title, string message, Action onConfirm, Action onCancel = null, string confirmLabel = "ОК", string cancelLabel = "Отмена")
        {
            _titleText.text = title;
            _bodyText.text = message;

            _onConfirmAction = onConfirm;
            _onCancelAction = onCancel;

            // Настройка кнопок
            if (_btnConfirmText) _btnConfirmText.text = confirmLabel;
            if (_btnCancelText) _btnCancelText.text = cancelLabel;

            _btnConfirm.gameObject.SetActive(true);
            _btnCancel.gameObject.SetActive(true);
            if (_btnAlt) _btnAlt.gameObject.SetActive(false); // Третью кнопку пока скрываем

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Показать просто уведомление (только кнопка ОК)
        /// </summary>
        public void ShowNotification(string title, string message, Action onClose = null)
        {
            _titleText.text = title;
            _bodyText.text = message;
            _onConfirmAction = onClose;

            if (_btnConfirmText) _btnConfirmText.text = "Понятно";

            _btnConfirm.gameObject.SetActive(true);
            _btnCancel.gameObject.SetActive(false); // Скрываем кнопку отмены
            if (_btnAlt) _btnAlt.gameObject.SetActive(false);

            gameObject.SetActive(true);
        }

        private void OnConfirmClicked()
        {
            // Выполняем действие
            _onConfirmAction?.Invoke();
            Close();
        }

        private void OnCancelClicked()
        {
            _onCancelAction?.Invoke();
            Close();
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}