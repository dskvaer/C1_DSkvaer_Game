using UnityEngine;

namespace NPC.Characters.Player {
    [CreateAssetMenu(fileName = "HealthConfig", menuName = "Player/HealthConfig", order = 1)]
    public class HealthConfigSO : ScriptableObject {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private float regenRate = 0f; // HP per second, 0 if no regen
        [SerializeField] private float damageThreshold = 0f; // Minimum damage to apply

        public int MaxHealth => maxHealth;
        public float RegenRate => regenRate;
        public float DamageThreshold => damageThreshold;
    }
}