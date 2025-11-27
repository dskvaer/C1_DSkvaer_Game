using Ship;
using UnityEngine;

// Компонент для управления визуальными эффектами корабля (волны, след)
[RequireComponent(typeof(ParticleSystem), typeof(TrailRenderer))]
public class ShipEffects : MonoBehaviour {
    [SerializeField] private ShipEffectsConfig config; // Настройки эффектов
    [SerializeField] private ShipMovement shipMovement; // Компонент движения корабля
    [SerializeField] private Material waveParticleMaterial; // Материал для частиц волн
    private ParticleSystem waveParticles; // Система частиц для волн
    private TrailRenderer trailRenderer; // Компонент для следа
    private ParticleSystem.EmissionModule emissionModule; // Модуль эмиссии частиц

    // Инициализация при старте
    private void Awake()
    {
        waveParticles = GetComponent<ParticleSystem>();
        trailRenderer = GetComponent<TrailRenderer>();
        emissionModule = waveParticles.emission;

        // Проверка привязки компонентов
        if (config == null)
        {
            Debug.LogError("ShipEffectsConfig не привязан в Inspector!");
            return;
        }
        if (shipMovement == null)
        {
            Debug.LogError("ShipMovement не привязан в Inspector!");
            return;
        }
        if (waveParticleMaterial == null)
        {
            Debug.LogError("Материал для частиц волн не привязан в Inspector!");
            return;
        }

        // Настройка ParticleSystem
        var main = waveParticles.main;
        main.startSpeed = config.WaveParticleSpeed; // Скорость частиц
        main.startSize = config.WaveParticleSize; // Размер частиц
        main.startColor = config.WaveParticleColor; // Цвет частиц
        main.startLifetime = config.WaveParticleLifetime; // Время жизни частиц
        emissionModule.rateOverTime = 0f; // Выключаем эмиссию по умолчанию

        // Применяем материал для частиц
        var particleRenderer = waveParticles.GetComponent<ParticleSystemRenderer>();
        particleRenderer.material = waveParticleMaterial;
        particleRenderer.sortingLayerName = "Default";
        particleRenderer.sortingOrder = 5; // Выше тайлмапа, ниже корабля

        // Настройка TrailRenderer
        trailRenderer.time = config.TrailTime; // Длительность следа
        trailRenderer.startWidth = config.TrailStartWidth; // Начальная ширина
        trailRenderer.endWidth = config.TrailEndWidth; // Конечная ширина
        trailRenderer.startColor = config.TrailStartColor; // Цвет начала
        trailRenderer.endColor = config.TrailEndColor; // Цвет конца
        trailRenderer.emitting = false; // Выключаем след по умолчанию
        trailRenderer.sortingLayerName = "Default";
        trailRenderer.sortingOrder = 5; // Выше тайлмапа, ниже корабля

        Debug.Log("Эффекты корабля (волны, след) инициализированы");
    }

    // Обновление эффектов каждый кадр
    private void Update()
    {
        if (shipMovement == null || config == null) return;

        float speed = shipMovement.GetShipVelocity().magnitude; // Текущая скорость корабля
        bool isMoving = speed >= config.WaveMinSpeed;

        // Включение/выключение волн
        emissionModule.rateOverTime = isMoving ? config.WaveEmissionRate : 0f;

        // Включение/выключение следа
        trailRenderer.emitting = speed >= config.TrailMinSpeed;

        Debug.Log($"Эффекты: Скорость={speed}, Волны={(isMoving ? "вкл" : "выкл")}, След={(trailRenderer.emitting ? "вкл" : "выкл")}");
    }
}