using System.Collections.Generic;
using UnityEngine;

namespace Ship {
    [CreateAssetMenu(fileName = "EnemyBehaviorProfile", menuName = "ShipConfigs/Enemy Behavior Profile", order = 21)]
    public class EnemyBehaviorProfileConfig : ScriptableObject {
        [Header("Выбор тактик")]
        [InspectorLabel("Интервал решений")]
        [Tooltip("Как часто NPC пересматривает активную тактику из профиля.")]
        [SerializeField, Min(0.05f)] private float decisionInterval = 0.35f;
        [InspectorLabel("Тактики")]
        [Tooltip("Список доступных тактик с приоритетом, весом и перезарядкой.")]
        [SerializeField] private List<EnemyBehaviorTacticSlot> tactics = new();

        public float DecisionInterval => decisionInterval;
        public IReadOnlyList<EnemyBehaviorTacticSlot> Tactics => tactics;
    }
}
