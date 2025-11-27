using UnityEngine;

namespace Ship {
    /// <summary>
    /// Настройки эффекта ряби на воде вокруг неподвижного корабля.
    /// </summary>
    /// <remarks>
    /// Привязка в Unity Inspector:
    /// - Привязать к компоненту ShipRippleEffect на объекте с ParticleSystem.
    /// Настройка сцены:
    /// - Создайте ScriptableObject через меню (File > Create > ShipConfigs/EffectsConfigs > ShipRippleConfig).
    /// - Убедитесь, что значения RippleParticleSpeed, RippleParticleSize, RippleEmissionRate, RippleParticleLifetime, RippleFadeSpeed, RippleMaxSpeed, RippleEmissionRadius сбалансированы для реалистичного эффекта.
    /// </remarks>
    [CreateAssetMenu(fileName = "ShipRippleConfig", menuName = "ShipConfigs/EffectsConfigs/ShipRippleConfig", order = 3)]
    public class ShipRippleConfig : ScriptableObject {
        [SerializeField] private float rippleParticleSpeed = 0.5f; // Скорость частиц
        /// <summary>
        /// Скорость частиц ряби (в единицах Unity/сек).
        /// Используется в ShipRippleEffect для настройки ParticleSystem.
        /// </summary>
        public float RippleParticleSpeed => rippleParticleSpeed;

        [SerializeField] private float rippleParticleSize = 1f; // Размер частиц
        /// <summary>
        /// Начальный размер частиц ряби (в единицах Unity).
        /// Используется в ShipRippleEffect для настройки ParticleSystem.
        /// </summary>
        public float RippleParticleSize => rippleParticleSize;

        [SerializeField] private float rippleEmissionRate = 0.5f; // Частота эмиссии
        /// <summary>
        /// Количество частиц в секунду (частота появления, низкая для одиночных частиц).
        /// Используется в ShipRippleEffect для настройки ParticleSystem.
        /// </summary>
        public float RippleEmissionRate => rippleEmissionRate;

        [SerializeField] private Color rippleParticleColor = new Color(0.8f, 0.9f, 1f, 0.5f); // Цвет частиц
        /// <summary>
        /// Начальный цвет частиц ряби (должен соответствовать начальному цвету градиента в Color over Lifetime).
        /// Используется в ShipRippleEffect для настройки ParticleSystem.
        /// </summary>
        public Color RippleParticleColor => rippleParticleColor;

        [SerializeField] private float rippleParticleLifetime = 6f; // Время жизни частиц
        /// <summary>
        /// Базовая длительность жизни частиц ряби (в секундах).
        /// Используется в ShipRippleEffect для настройки ParticleSystem.
        /// </summary>
        public float RippleParticleLifetime => rippleParticleLifetime;

        [SerializeField] private float rippleFadeSpeed = 1f; // Скорость затухания
        /// <summary>
        /// Скорость затухания ряби (1 = нормальная, <1 = быстрее, >1 = медленнее).
        /// Используется в ShipRippleEffect для настройки ParticleSystem.
        /// </summary>
        public float RippleFadeSpeed => rippleFadeSpeed;

        [SerializeField] private float rippleMaxSpeed = 0.2f; // Максимальная скорость
        /// <summary>
        /// Максимальная скорость корабля для активации ряби (в морских узлах).
        /// Используется в ShipRippleEffect для проверки скорости.
        /// </summary>
        public float RippleMaxSpeed => rippleMaxSpeed;

        [SerializeField] private float rippleEmissionRadius = 0f; // Радиус эмиссии
        /// <summary>
        /// Радиус эмиссии ряби (0 для старта из центра).
        /// Используется в ShipRippleEffect для настройки ParticleSystem.
        /// </summary>
        public float RippleEmissionRadius => rippleEmissionRadius;
    }
}