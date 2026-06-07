using UnityEngine;

namespace Ship {
    /// <summary>
    /// Настройки визуальных эффектов движения корабля: волны и след на воде.
    /// </summary>
    [CreateAssetMenu(fileName = "ShipEffectsConfig", menuName = "ShipConfigs/EffectsConfigs/ShipEffectsConfig", order = 2)]
    public class ShipEffectsConfig : ScriptableObject {
        [Header("Волны")]
        [InspectorLabel("Скорость частиц волн")]
        [Tooltip("Скорость движения частиц волны в единицах Unity в секунду.")]
        [SerializeField] private float waveParticleSpeed = 2f;
        public float WaveParticleSpeed => waveParticleSpeed;

        [InspectorLabel("Размер частиц волн")]
        [Tooltip("Размер отдельных частиц волны.")]
        [SerializeField] private float waveParticleSize = 0.2f;
        public float WaveParticleSize => waveParticleSize;

        [InspectorLabel("Частота волн")]
        [Tooltip("Сколько частиц волны испускается в секунду при активном движении.")]
        [SerializeField] private float waveEmissionRate = 20f;
        public float WaveEmissionRate => waveEmissionRate;

        [InspectorLabel("Цвет волн")]
        [Tooltip("Цвет частиц волны, включая прозрачность.")]
        [SerializeField] private Color waveParticleColor = new Color(1f, 1f, 1f, 0.8f);
        public Color WaveParticleColor => waveParticleColor;

        [InspectorLabel("Время жизни волн")]
        [Tooltip("Сколько секунд живет частица волны после создания.")]
        [SerializeField] private float waveParticleLifetime = 1f;
        public float WaveParticleLifetime => waveParticleLifetime;

        [InspectorLabel("Минимальная скорость для волн")]
        [Tooltip("Корабль должен двигаться быстрее этого значения, чтобы включались частицы волн.")]
        [SerializeField] private float waveMinSpeed = 2f;
        public float WaveMinSpeed => waveMinSpeed;

        [Header("След на воде")]
        [InspectorLabel("Длительность следа")]
        [Tooltip("Сколько секунд виден след за кораблем.")]
        [SerializeField] private float trailTime = 2f;
        public float TrailTime => trailTime;

        [InspectorLabel("Начальная ширина следа")]
        [Tooltip("Ширина следа у кормы корабля.")]
        [SerializeField] private float trailStartWidth = 0.5f;
        public float TrailStartWidth => trailStartWidth;

        [InspectorLabel("Конечная ширина следа")]
        [Tooltip("Ширина следа в конце линии затухания.")]
        [SerializeField] private float trailEndWidth = 0.1f;
        public float TrailEndWidth => trailEndWidth;

        [InspectorLabel("Начальный цвет следа")]
        [Tooltip("Цвет следа рядом с кораблем.")]
        [SerializeField] private Color trailStartColor = new Color(1f, 1f, 1f, 0.8f);
        public Color TrailStartColor => trailStartColor;

        [InspectorLabel("Конечный цвет следа")]
        [Tooltip("Цвет следа в конце линии. Обычно прозрачный.")]
        [SerializeField] private Color trailEndColor = new Color(1f, 1f, 1f, 0f);
        public Color TrailEndColor => trailEndColor;

        [InspectorLabel("Минимальная скорость для следа")]
        [Tooltip("Корабль должен двигаться быстрее этого значения, чтобы включался след.")]
        [SerializeField] private float trailMinSpeed = 2f;
        public float TrailMinSpeed => trailMinSpeed;
    }
}
