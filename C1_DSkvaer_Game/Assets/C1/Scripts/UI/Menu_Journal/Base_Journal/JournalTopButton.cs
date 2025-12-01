using UnityEngine;
using UnityEngine.UI;

namespace Menu_Journal {
    // Этот скрипт вешается на кнопки "Инфо", "Инвентарь" в шапке меню.
    [RequireComponent(typeof(Button))]
    public class JournalTopButton : MonoBehaviour {
        [SerializeField] private JournalTabType _targetTab; // Какую вкладку открывать?

        // Контроллер можно перетащить вручную, или он найдется сам, если кнопка внутри меню
        [SerializeField] private JournalController _controller;

        private Button _button;

        private void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClicked);

            // Если забыли привязать контроллер, пытаемся найти его в родителях
            if (_controller == null)
            {
                _controller = GetComponentInParent<JournalController>();
            }
        }

        private void OnButtonClicked()
        {
            if (_controller != null)
            {
                _controller.SwitchTab(_targetTab);
            }
            else
            {
                Debug.LogError("Кнопка меню не знает, где находится JournalController!");
            }
        }
    }
}