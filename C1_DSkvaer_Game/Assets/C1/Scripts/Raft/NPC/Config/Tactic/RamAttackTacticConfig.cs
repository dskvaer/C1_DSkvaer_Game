using UnityEngine;

namespace Ship {
    /// <summary>
    /// Настройки тактики тарана NPC.
    /// </summary>
    [CreateAssetMenu(fileName = "RamAttackTacticConfig", menuName = "ShipConfigs/TacticConfigs/RamAttackTacticConfig", order = 9)]
    public class RamAttackTacticConfig : ScriptableObject {
        [Header("Таран")]
        [InspectorLabel("Скорость атаки")]
        [Tooltip("Скорость сближения с игроком во время атаки тараном.")]
        [SerializeField] private float attackSpeed = 1.0f;
        public float AttackSpeed => attackSpeed;

        [InspectorLabel("Дистанция начала тарана")]
        [Tooltip("На какой дистанции NPC переходит от сближения к прямому тарану.")]
        [SerializeField] private float ramDistance = 4f;
        public float RamDistance => ramDistance;

        [InspectorLabel("Дистанция контакта")]
        [Tooltip("Дистанция, на которой считается, что таран попал по цели.")]
        [SerializeField] private float contactDistance = 0.8f;
        public float ContactDistance => contactDistance;

        [InspectorLabel("Время отхода")]
        [Tooltip("Сколько секунд NPC отходит назад после попытки тарана.")]
        [SerializeField] private float retreatTime = 1.2f;
        public float RetreatTime => retreatTime;

        [InspectorLabel("Мин. скорость тарана")]
        [Tooltip("Минимальная скорость, при которой удар тараном считается достаточно сильным.")]
        [SerializeField] private float minRamSpeed = 2f;
        public float MinRamSpeed => minRamSpeed;

        [InspectorLabel("Плавность поворота")]
        [Tooltip("Плавность разворота во время атаки. Значение от 0 до 1.")]
        [SerializeField] private float smoothTurnSpeed = 0.3f;
        public float SmoothTurnSpeed => smoothTurnSpeed;

        [InspectorLabel("Перезарядка урона")]
        [Tooltip("Пауза между повторными нанесениями урона тараном.")]
        [SerializeField] private float damageCooldown = 1f;
        public float DamageCooldown => damageCooldown;

        [InspectorLabel("Порог здоровья")]
        [Tooltip("Процент здоровья от 0 до 1, выше которого NPC может использовать таран.")]
        [SerializeField] private float healthThreshold = 0.2f;
        public float HealthThreshold => healthThreshold;
    }
}
