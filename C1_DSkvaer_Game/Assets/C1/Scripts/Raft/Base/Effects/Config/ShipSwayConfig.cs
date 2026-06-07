using UnityEngine;

namespace Ship {
    /// <summary>
    /// Настройки легкого покачивания визуала корабля.
    /// </summary>
    [CreateAssetMenu(fileName = "ShipSwayConfig", menuName = "ShipConfigs/EffectsConfigs/ShipSwayConfig", order = 3)]
    public class ShipSwayConfig : ScriptableObject {
        [Header("Покачивание")]
        [InspectorLabel("Амплитуда")]
        [Tooltip("Насколько сильно визуал корабля смещается вверх и вниз.")]
        [SerializeField] private float swayAmplitude = 0.2f;
        public float SwayAmplitude => swayAmplitude;

        [InspectorLabel("Период")]
        [Tooltip("Сколько секунд занимает полный цикл покачивания.")]
        [SerializeField] private float swayPeriod = 1f;
        public float SwayPeriod => swayPeriod;
    }
}
