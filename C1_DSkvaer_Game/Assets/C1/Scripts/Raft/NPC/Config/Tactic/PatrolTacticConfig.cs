using UnityEngine;

namespace Ship {
    /// <summary>
    /// Настройки тактики патрулирования NPC.
    /// </summary>
    [CreateAssetMenu(fileName = "PatrolTacticConfig", menuName = "ShipConfigs/TacticConfigs/PatrolTacticConfig", order = 8)]
    public class PatrolTacticConfig : ScriptableObject {
        [Header("Патрулирование")]
        [InspectorLabel("Скорость патруля")]
        [Tooltip("Скорость движения NPC во время патрулирования.")]
        [SerializeField, Min(0f)] private float patrolSpeed = 0.5f;
        public float PatrolSpeed => patrolSpeed;

        [InspectorLabel("Мин. время патруля")]
        [Tooltip("Минимальное время движения к одной патрульной точке перед выбором новой.")]
        [SerializeField, Min(0.1f)] private float minPatrolTime = 3f;
        public float MinPatrolTime => minPatrolTime;

        [InspectorLabel("Макс. время патруля")]
        [Tooltip("Максимальное время движения к одной патрульной точке перед выбором новой.")]
        [SerializeField, Min(0.1f)] private float maxPatrolTime = 7f;
        public float MaxPatrolTime => maxPatrolTime;

        [InspectorLabel("Дистанция достижения точки")]
        [Tooltip("Если NPC подошел к цели ближе этой дистанции, выбирается новая точка патруля.")]
        [SerializeField, Min(0.05f)] private float pointReachDistance = 0.5f;
        public float PointReachDistance => pointReachDistance;

        [InspectorLabel("Плавность поворота")]
        [Tooltip("Плавность разворота к патрульной точке. Значение от 0 до 1.")]
        [SerializeField, Range(0f, 1f)] private float smoothTurnSpeed = 0.3f;
        public float SmoothTurnSpeed => smoothTurnSpeed;

        private void OnValidate()
        {
            if (maxPatrolTime < minPatrolTime) {
                maxPatrolTime = minPatrolTime;
            }
        }
    }
}
