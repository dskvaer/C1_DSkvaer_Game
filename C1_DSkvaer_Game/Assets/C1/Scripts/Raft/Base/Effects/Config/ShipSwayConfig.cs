using UnityEngine;

namespace Ship {
    /// <summary>
    /// Настройки покачивания корабля.
    /// </summary>
    /// <remarks>
    /// Привязка в Unity Inspector:
    /// - Привязать к компоненту ShipSway на визуальном объекте корабля.
    /// Настройка сцены:
    /// - Создайте ScriptableObject через меню (File > Create > ShipConfigs/EffectsConfigs > ShipSwayConfig).
    /// - Убедитесь, что значения SwayAmplitude и SwayPeriod сбалансированы для реалистичного эффекта.
    /// </remarks>
    [CreateAssetMenu(fileName = "ShipSwayConfig", menuName = "ShipConfigs/EffectsConfigs/ShipSwayConfig", order = 3)]
    public class ShipSwayConfig : ScriptableObject {
        [SerializeField] private float swayAmplitude = 0.2f; // Амплитуда смещения
        /// <summary>
        /// Амплитуда смещения для покачивания (в единицах Unity, влево-вправо).
        /// Используется в ShipSway для анимации.
        /// </summary>
        public float SwayAmplitude => swayAmplitude;

        [SerializeField] private float swayPeriod = 1f; // Период покачивания
        /// <summary>
        /// Период покачивания (в секундах).
        /// Используется в ShipSway для анимации.
        /// </summary>
        public float SwayPeriod => swayPeriod;
    }
}