using UnityEngine;

namespace Ship {
    /// <summary>
    /// Компонент для управления покачиванием визуального объекта корабля.
    /// </summary>
    /// <remarks>
    /// Привязка в Unity Inspector:
    /// - Config: Настройки покачивания (ShipSwayConfig, задает амплитуду и период).
    /// Настройка сцены:
    /// - Привяжите компонент к визуальному объекту корабля (например, SpriteRenderer корабля).
    /// - Убедитесь, что LeanTween установлен в проекте (через Package Manager или Asset Store).
    /// - Создайте ShipSwayConfig через меню (File > Create > ShipConfigs/EffectsConfigs > ShipSwayConfig).
    /// Логика работы:
    /// - Awake: Проверяет наличие конфигурации ShipSwayConfig.
    /// - Start: Запускает анимацию покачивания с использованием LeanTween (смещение по локальной оси X).
    /// </remarks>
    public class ShipSway : MonoBehaviour {
        [SerializeField] private ShipSwayConfig config; // Настройки покачивания

        // Инициализация при старте
        private void Awake()
        {
            if (config == null) // Проверяем наличие конфигурации
            {
                Debug.LogError($"ShipSwayConfig не привязан для {gameObject.name}!", this); // Логируем ошибку
                enabled = false; // Отключаем компонент
                return;
            }
            Debug.Log($"ShipSway инициализирован для {gameObject.name} с амплитудой {config.SwayAmplitude} и периодом {config.SwayPeriod}"); // Логируем инициализацию
        }

        // Запуск анимации покачивания
        private void Start()
        {
            if (config != null) // Проверяем наличие конфигурации
            {
                // Запускаем покачивание влево-вправо по локальной оси X
                LeanTween.moveLocalX(gameObject, config.SwayAmplitude, config.SwayPeriod / 2f)
                    .setLoopPingPong() // Устанавливаем циклическую анимацию
                    .setEase(LeanTweenType.easeInOutSine); // Устанавливаем тип сглаживания
                Debug.Log($"Покачивание запущено для {gameObject.name}: Амплитуда={config.SwayAmplitude}, Период={config.SwayPeriod}"); // Логируем запуск
            }
        }
    }
}