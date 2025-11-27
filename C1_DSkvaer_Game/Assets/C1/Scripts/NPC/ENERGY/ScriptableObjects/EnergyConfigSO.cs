using UnityEngine;

namespace NPC.Characters.Player.Energy {
    [CreateAssetMenu(fileName = "EnergyConfig", menuName = "Player/EnergyConfig", order = 2)]
    public class EnergyConfigSO : ScriptableObject {
        [Header("Energy Settings")]
        [SerializeField] private int maxEnergy = 100;
        [SerializeField] private float regenRate = 5f; // Energy per second

        [Header("Thresholds")]
        [SerializeField] private float lowEnergyThreshold = 20f; // For events

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