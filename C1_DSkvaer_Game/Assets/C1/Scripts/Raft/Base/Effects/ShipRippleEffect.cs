using Ship;
using UnityEngine;

// Компонент для управления эффектом ряби на воде вокруг неподвижного корабля
[RequireComponent(typeof(ParticleSystem))]
public class ShipRippleEffect : MonoBehaviour {
    [SerializeField] private ShipRippleConfig config; // Настройки ряби
    [SerializeField] private ShipMovement shipMovement; // Компонент движения корабля
    [SerializeField] private Material rippleParticleMaterial; // Материал для частиц ряби
    private ParticleSystem rippleParticles; // Система частиц для ряби
    private ParticleSystem.EmissionModule emissionModule; // Модуль эмиссии частиц
    private float stopTimer; // Таймер для задержки активации ряби
    private bool isMoveInputActive; // Текущее состояние ввода движения
    private const float STOP_DELAY = 2f; // Задержка 2 секунды перед включением ряби

    // Инициализация при старте
    private void Awake()
    {
        rippleParticles = GetComponent<ParticleSystem>();
        emissionModule = rippleParticles.emission;

        // Проверка привязки компонентов
        if (config == null)
        {
            Debug.LogError("ShipRippleConfig не привязан в Inspector!");
        }
        if (shipMovement == null)
        {
            Debug.LogError("ShipMovement не привязан в Inspector!");
        }
        if (rippleParticleMaterial == null)
        {
            Debug.LogError("Материал для частиц ряби не привязан в Inspector!");
        }

        // Настройка ParticleSystem (базовые параметры)
        var main = rippleParticles.main;
        main.startSpeed = config.RippleParticleSpeed; // Скорость частиц
        main.startSize = config.RippleParticleSize; // Размер частиц
        main.startColor = config.RippleParticleColor; // Начальный цвет частиц (голубой оттенок воды)
        main.startLifetime = config.RippleParticleLifetime * config.RippleFadeSpeed; // Время жизни с учетом скорости затухания
        emissionModule.rateOverTime = 0f; // Выключаем эмиссию по умолчанию

        // Настройка формы эмиссии (точка в центре)
        var shape = rippleParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = config.RippleEmissionRadius; // Радиус 0 для старта из центра

        // Проверка модулей (настраиваются в Inspector)
        var sizeOverLifetime = rippleParticles.sizeOverLifetime;
        if (!sizeOverLifetime.enabled)
        {
            Debug.LogWarning("Size over Lifetime не включен в ParticleSystem! Включите и настройте кривую в Inspector.");
        }

        var colorOverLifetime = rippleParticles.colorOverLifetime;
        if (!colorOverLifetime.enabled)
        {
            Debug.LogWarning("Color over Lifetime не включен в ParticleSystem! Включите и настройте градиент (Alpha от 0.5 до 0, начальный цвет должен соответствовать RippleParticleColor) в Inspector.");
        }

        var velocityOverLifetime = rippleParticles.velocityOverLifetime;
        if (!velocityOverLifetime.enabled)
        {
            Debug.LogWarning("Velocity over Lifetime не включен в ParticleSystem! Включите и настройте скорость в Inspector.");
        }

        // Применяем материал для частиц
        var renderer = rippleParticles.GetComponent<ParticleSystemRenderer>();
        renderer.material = rippleParticleMaterial;
        renderer.sortingLayerName = "Default";
        renderer.sortingOrder = 4; // Выше тайлмапа, ниже волн и следа

        stopTimer = 0f;
        isMoveInputActive = false;
        Debug.Log("Эффект ряби инициализирован");
    }

    // Подписка на события при включении компонента
    private void OnEnable()
    {
        if (shipMovement != null)
        {
            shipMovement.OnMoveInputChanged += HandleMoveInputChanged;
            // Инициализируем начальное состояние (предполагаем, что ввод неактивен)
            HandleMoveInputChanged(false);
        }
    }

    // Отписка от событий при выключении компонента
    private void OnDisable()
    {
        if (shipMovement != null)
        {
            shipMovement.OnMoveInputChanged -= HandleMoveInputChanged;
        }
    }

    // Обновление таймера для задержки
    private void Update()
    {
        if (!isMoveInputActive && stopTimer > 0f)
        {
            stopTimer -= Time.deltaTime;
            if (stopTimer <= 0f)
            {
                emissionModule.rateOverTime = config.RippleEmissionRate;
                Debug.Log("Рябь включена после задержки");
            }
        }
    }

    // Обработка изменения ввода движения
    private void HandleMoveInputChanged(bool isInputActive)
    {
        isMoveInputActive = isInputActive;
        if (isMoveInputActive)
        {
            // Немедленно выключаем рябь при активном вводе
            emissionModule.rateOverTime = 0f;
            stopTimer = STOP_DELAY; // Сбрасываем таймер
            Debug.Log("Рябь выключена: Ввод движения активен");
        }
        else
        {
            // Запускаем таймер задержки перед включением ряби
            stopTimer = STOP_DELAY;
            emissionModule.rateOverTime = 0f; // Отключаем рябь до истечения таймера
            Debug.Log($"Рябь: Ввод движения неактивен, ждём {STOP_DELAY} сек");
        }
    }
}