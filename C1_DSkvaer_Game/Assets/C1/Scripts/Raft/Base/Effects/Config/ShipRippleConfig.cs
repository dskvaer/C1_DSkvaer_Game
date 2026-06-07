using UnityEngine;

namespace Ship {
    /// <summary>
    /// Настройки кругов воды вокруг корабля при движении.
    /// </summary>
    [CreateAssetMenu(fileName = "ShipRippleConfig", menuName = "ShipConfigs/EffectsConfigs/ShipRippleConfig", order = 3)]
    public class ShipRippleConfig : ScriptableObject {
        [Header("Круги на воде")]
        [InspectorLabel("Скорость частиц")]
        [Tooltip("Скорость движения частиц кругов воды.")]
        [SerializeField] private float rippleParticleSpeed = 0.5f;
        public float RippleParticleSpeed => rippleParticleSpeed;

        [InspectorLabel("Размер частиц")]
        [Tooltip("Размер частиц кругов воды.")]
        [SerializeField] private float rippleParticleSize = 1f;
        public float RippleParticleSize => rippleParticleSize;

        [InspectorLabel("Частота испускания")]
        [Tooltip("Сколько частиц создается в секунду. Низкое значение дает редкие спокойные круги.")]
        [SerializeField] private float rippleEmissionRate = 0.5f;
        public float RippleEmissionRate => rippleEmissionRate;

        [InspectorLabel("Цвет частиц")]
        [Tooltip("Цвет кругов воды, включая прозрачность.")]
        [SerializeField] private Color rippleParticleColor = new Color(0.8f, 0.9f, 1f, 0.5f);
        public Color RippleParticleColor => rippleParticleColor;

        [InspectorLabel("Время жизни")]
        [Tooltip("Сколько секунд живет частица круга воды.")]
        [SerializeField] private float rippleParticleLifetime = 6f;
        public float RippleParticleLifetime => rippleParticleLifetime;

        [InspectorLabel("Скорость затухания")]
        [Tooltip("Как быстро круги становятся прозрачными. 1 - стандартно, больше 1 - быстрее.")]
        [SerializeField] private float rippleFadeSpeed = 1f;
        public float RippleFadeSpeed => rippleFadeSpeed;

        [InspectorLabel("Максимальная скорость для кругов")]
        [Tooltip("Круги показываются только когда корабль движется не быстрее этого значения.")]
        [SerializeField] private float rippleMaxSpeed = 0.2f;
        public float RippleMaxSpeed => rippleMaxSpeed;

        [InspectorLabel("Радиус испускания")]
        [Tooltip("Радиус области, из которой появляются круги. 0 означает точку в центре эффекта.")]
        [SerializeField] private float rippleEmissionRadius = 0f;
        public float RippleEmissionRadius => rippleEmissionRadius;
    }
}
