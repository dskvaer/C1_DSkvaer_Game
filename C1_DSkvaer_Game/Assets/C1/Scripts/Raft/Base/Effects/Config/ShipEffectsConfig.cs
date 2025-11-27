using UnityEngine;

namespace Ship {
    /// <summary>
    /// Настройки визуальных эффектов корабля (волны, след).
    /// </summary>
    /// <remarks>
    /// Привязка в Unity Inspector:
    /// - Привязать к компоненту ShipEffects на объекте с ParticleSystem и TrailRenderer.
    /// Настройка сцены:
    /// - Создайте ScriptableObject через меню (File > Create > ShipConfigs/EffectsConfigs > ShipEffectsConfig).
    /// - Убедитесь, что значения WaveParticleSpeed, WaveParticleSize, WaveEmissionRate, WaveParticleLifetime, WaveMinSpeed, TrailTime, TrailStartWidth, TrailEndWidth, TrailStartColor, TrailEndColor, TrailMinSpeed сбалансированы для реалистичного эффекта.
    /// </remarks>
    [CreateAssetMenu(fileName = "ShipEffectsConfig", menuName = "ShipConfigs/EffectsConfigs/ShipEffectsConfig", order = 2)]
    public class ShipEffectsConfig : ScriptableObject {
        [SerializeField] private float waveParticleSpeed = 2f; // Скорость частиц волн
        /// <summary>
        /// Скорость частиц волн (в единицах Unity/сек).
        /// Используется в ShipEffects для настройки ParticleSystem.
        /// </summary>
        public float WaveParticleSpeed => waveParticleSpeed;

        [SerializeField] private float waveParticleSize = 0.2f; // Размер частиц волн
        /// <summary>
        /// Размер частиц волн (в единицах Unity).
        /// Используется в ShipEffects для настройки ParticleSystem.
        /// </summary>
        public float WaveParticleSize => waveParticleSize;

        [SerializeField] private float waveEmissionRate = 20f; // Частота эмиссии волн
        /// <summary>
        /// Количество частиц в секунду (частота появления).
        /// Используется в ShipEffects для настройки ParticleSystem.
        /// </summary>
        public float WaveEmissionRate => waveEmissionRate;

        [SerializeField] private Color waveParticleColor = new Color(1f, 1f, 1f, 0.8f); // Цвет частиц волн
        /// <summary>
        /// Цвет частиц волн.
        /// Используется в ShipEffects для настройки ParticleSystem.
        /// </summary>
        public Color WaveParticleColor => waveParticleColor;

        [SerializeField] private float waveParticleLifetime = 1f; // Время жизни частиц волн
        /// <summary>
        /// Длительность жизни частиц волн (в секундах).
        /// Используется в ShipEffects для настройки ParticleSystem.
        /// </summary>
        public float WaveParticleLifetime => waveParticleLifetime;

        [SerializeField] private float waveMinSpeed = 2f; // Минимальная скорость для волн
        /// <summary>
        /// Минимальная скорость корабля для активации волн (в морских узлах).
        /// Используется в ShipEffects для проверки скорости.
        /// </summary>
        public float WaveMinSpeed => waveMinSpeed;

        [SerializeField] private float trailTime = 2f; // Длительность следа
        /// <summary>
        /// Время затухания следа (в секундах).
        /// Используется в ShipEffects для настройки TrailRenderer.
        /// </summary>
        public float TrailTime => trailTime;

        [SerializeField] private float trailStartWidth = 0.5f; // Начальная ширина следа
        /// <summary>
        /// Ширина следа в начале (в единицах Unity).
        /// Используется в ShipEffects для настройки TrailRenderer.
        /// </summary>
        public float TrailStartWidth => trailStartWidth;

        [SerializeField] private float trailEndWidth = 0.1f; // Конечная ширина следа
        /// <summary>
        /// Ширина следа в конце (в единицах Unity).
        /// Используется в ShipEffects для настройки TrailRenderer.
        /// </summary>
        public float TrailEndWidth => trailEndWidth;

        [SerializeField] private Color trailStartColor = new Color(1f, 1f, 1f, 0.8f); // Начальный цвет следа
        /// <summary>
        /// Цвет следа в начале.
        /// Используется в ShipEffects для настройки TrailRenderer.
        /// </summary>
        public Color TrailStartColor => trailStartColor;

        [SerializeField] private Color trailEndColor = new Color(1f, 1f, 1f, 0f); // Конечный цвет следа
        /// <summary>
        /// Цвет следа в конце.
        /// Используется в ShipEffects для настройки TrailRenderer.
        /// </summary>
        public Color TrailEndColor => trailEndColor;

        [SerializeField] private float trailMinSpeed = 2f; // Минимальная скорость для следа
        /// <summary>
        /// Минимальная скорость корабля для активации следа (в морских узлах).
        /// Используется в ShipEffects для проверки скорости.
        /// </summary>
        public float TrailMinSpeed => trailMinSpeed;
    }
}