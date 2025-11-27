using UnityEngine; // Подключаем базовую библиотеку Unity
using UnityEngine.UI; // Подключаем UI для работы с Slider

namespace C1.Scripts.UI.Game_play_UI // Пространство имён для UI игрового процесса
{
    /// <summary>
    /// Компонент для отображения полосы здоровья корабля через UI Slider.
    /// </summary>
    /// <remarks>
    /// Привязка в Unity Inspector:
    /// - HealthSlider: UI Slider для отображения процента здоровья.
    /// - HealthComponent: Компонент, реализующий IHealth (например, ShipHealth).
    /// Настройка сцены:
    /// - Убедитесь, что HealthSlider привязан к UI Slider на объекте (Canvas > HealthBar).
    /// - HealthComponent должен ссылаться на ShipHealth на объекте корабля (Player_Ship, Enemy_Ship, Trader_Ship).
    /// - Canvas должен быть настроен как Screen Space - Camera, с привязкой к Main Camera и Scale With Screen Size.
    /// Логика работы:
    /// - Awake: Проверяет наличие Slider, получает IHealth.
    /// - Start: Инициализирует Slider и обновляет полосу.
    /// - UpdateBar: Обновляет Slider.value на основе текущего и максимального здоровья.
    /// - OnDeath: Плавно уменьшает полосу до 0 при смерти.
    /// </remarks>
    [RequireComponent(typeof(Slider))] // Требуем UI Slider на объекте
    public class HealthBar : MonoBehaviour {
        [SerializeField] private Slider healthSlider; // UI Slider для отображения здоровья
        [SerializeField] private MonoBehaviour healthComponent; // Компонент, реализующий IHealth (например, ShipHealth)
        private IHealth health; // Интерфейс для доступа к данным здоровья

        // Инициализация при старте сцены
        private void Awake()
        {
            if (healthSlider == null) // Проверяем наличие Slider
            {
                healthSlider = GetComponent<Slider>(); // Пытаемся получить Slider с объекта
                if (healthSlider == null) // Проверяем успешность получения
                {
                    Debug.LogError($"Slider не привязан для {gameObject.name}!", this); // Логируем ошибку, если Slider отсутствует
                    enabled = false; // Отключаем компонент
                    return;
                }
            }

            if (healthComponent != null) // Проверяем наличие HealthComponent
            {
                SetHealthComponent(healthComponent as IHealth); // Привязываем компонент здоровья
            }
            else
            {
                Debug.LogWarning($"HealthComponent не привязан для {gameObject.name}. Ожидается вызов SetHealthComponent.", this); // Логируем предупреждение
            }
        }

        // Инициализация Slider в начале сцены
        private void Start()
        {
            if (health != null && healthSlider != null) // Проверяем наличие health и Slider
            {
                healthSlider.maxValue = 1f; // Устанавливаем максимальное значение Slider (0-1)
                UpdateBar(); // Обновляем полосу здоровья
                ShipID shipID = healthComponent?.GetComponent<ShipID>(); // Получаем ShipID для логов
                Debug.Log($"Полоса здоровья инициализирована для {healthComponent?.name ?? "Unknown"} (ID={shipID?.ID ?? "Unknown"}): {health.CurrentHealth}/{health.MaxHealth} ({healthSlider.value * 100f}%)"); // Логируем инициализацию
            }
        }

        // Привязывает компонент здоровья
        public void SetHealthComponent(IHealth healthComp)
        {
            if (healthComp == null && healthComponent != null) // Проверяем, можно ли получить IHealth из healthComponent
            {
                healthComp = healthComponent.GetComponent<IHealth>(); // Пытаемся получить IHealth
            }

            if (healthComp == null) // Проверяем успешность получения IHealth
            {
                Debug.LogError($"IHealth не найден для {gameObject.name}!", this); // Логируем ошибку
                enabled = false; // Отключаем компонент
                return;
            }

            if (health != null) // Проверяем, был ли ранее привязан IHealth
            {
                health.OnHealthChanged.RemoveListener(UpdateBar); // Отписываемся от события изменения здоровья
                health.OnDeath.RemoveListener(OnDeath); // Отписываемся от события смерти
            }

            health = healthComp; // Устанавливаем новый компонент здоровья
            ShipID shipID = healthComponent?.GetComponent<ShipID>(); // Получаем ShipID для логов
            health.OnHealthChanged.AddListener(UpdateBar); // Подписываемся на событие изменения здоровья
            health.OnDeath.AddListener(OnDeath); // Подписываемся на событие смерти
            Debug.Log($"Полоса здоровья привязана для {healthComponent?.name ?? "Unknown"} (ID={shipID?.ID ?? "Unknown"}), Здоровье={health.CurrentHealth}/{health.MaxHealth}"); // Логируем привязку
        }

        // Обновляет полосу здоровья
        private void UpdateBar()
        {
            if (health == null || healthSlider == null) return; // Пропускаем, если health или Slider отсутствуют

            float maxHealth = health.MaxHealth; // Получаем максимальное здоровье
            if (maxHealth > 0) // Проверяем, что максимальное здоровье больше 0
            {
                healthSlider.value = (float)health.CurrentHealth / maxHealth; // Устанавливаем значение Slider (0-1)
                ShipID shipID = healthComponent?.GetComponent<ShipID>(); // Получаем ShipID для логов
                Debug.Log($"Полоса здоровья обновлена для {healthComponent?.name ?? "Unknown"} (ID={shipID?.ID ?? "Unknown"}): {health.CurrentHealth}/{health.MaxHealth} ({healthSlider.value * 100f}%)"); // Логируем обновление
            }
            else
            {
                Debug.LogWarning($"Максимальное здоровье=0 для {healthComponent?.name ?? "Unknown"}. Полоса установлена в 0.", this); // Логируем предупреждение
                healthSlider.value = 0; // Устанавливаем Slider в 0
            }
        }

        // Обрабатывает смерть объекта
        private void OnDeath()
        {
            if (healthSlider != null) // Проверяем наличие Slider
            {
                LeanTween.value(gameObject, v => healthSlider.value = v, healthSlider.value, 0f, 0.5f); // Плавно уменьшаем полосу до 0 за 0.5 секунды
                Debug.Log($"Полоса здоровья: Объект умер, полоса уменьшается до 0% для {healthComponent?.name ?? "Unknown"}"); // Логируем смерть
            }
        }

        // Очищает подписки при уничтожении
        private void OnDestroy()
        {
            if (health != null) // Проверяем наличие health
            {
                health.OnHealthChanged.RemoveListener(UpdateBar); // Отписываемся от события изменения здоровья
                health.OnDeath.RemoveListener(OnDeath); // Отписываемся от события смерти
            }
        }
    }
}