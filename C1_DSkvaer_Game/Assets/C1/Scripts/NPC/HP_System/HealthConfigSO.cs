using UnityEngine;

namespace NPC.Characters.Player {
    [CreateAssetMenu(fileName = "HealthConfig", menuName = "UI/HealthConfig", order = 1)]
    public class HealthConfigSO : ScriptableObject {
        [Header("Здоровье")]
        [InspectorLabel("Максимальное здоровье")]
        [Tooltip("Максимальный запас здоровья персонажа.")]
        [SerializeField] private int maxHealth = 100;

        [InspectorLabel("Регенерация")]
        [Tooltip("Сколько здоровья восстанавливается за секунду. 0 отключает регенерацию.")]
        [SerializeField] private float regenRate = 0f;

        [InspectorLabel("Порог урона")]
        [Tooltip("Минимальный урон, который будет применен. Урон ниже этого значения игнорируется.")]
        [SerializeField] private float damageThreshold = 0f;

        public int MaxHealth => maxHealth;
        public float RegenRate => regenRate;
        public float DamageThreshold => damageThreshold;
    }
}
