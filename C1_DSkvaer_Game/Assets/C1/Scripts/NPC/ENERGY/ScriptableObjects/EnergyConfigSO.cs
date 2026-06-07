using UnityEngine;

namespace NPC.Characters.Player.Energy {
    [CreateAssetMenu(fileName = "EnergyConfig", menuName = "UI/EnergyConfig", order = 2)]
    public class EnergyConfigSO : ScriptableObject {
        [Header("Энергия")]
        [InspectorLabel("Максимальная энергия")]
        [Tooltip("Максимальный запас энергии персонажа.")]
        [SerializeField] private int maxEnergy = 100;

        [InspectorLabel("Регенерация")]
        [Tooltip("Сколько энергии восстанавливается за секунду.")]
        [SerializeField] private float regenRate = 5f;

        [Header("Пороги")]
        [InspectorLabel("Порог низкой энергии")]
        [Tooltip("Если энергия опускается до этого значения или ниже, срабатывают события низкой энергии.")]
        [SerializeField] private float lowEnergyThreshold = 20f;

        public int MaxEnergy => maxEnergy;
        public float RegenRate => regenRate;
        public float LowEnergyThreshold => lowEnergyThreshold;

        private void OnValidate()
        {
            maxEnergy = Mathf.Max(1, maxEnergy);
            regenRate = Mathf.Max(0f, regenRate);
            lowEnergyThreshold = Mathf.Clamp(lowEnergyThreshold, 0f, maxEnergy);
        }
    }
}
