using UnityEngine;

namespace Ship {
    /// <summary>
    /// Патрулирование NPC: выбирает случайные точки внутри назначенной зоны или water tilemap.
    /// </summary>
    public class PatrolTactic : MonoBehaviour, IEnemyTactic {
        [Header("Настройки патруля")]
        [InspectorLabel("Конфиг патруля")]
        [Tooltip("ScriptableObject с настройками скорости, времени патруля и плавности поворота.")]
        [SerializeField] private PatrolTacticConfig config;

        [InspectorLabel("Зона патруля")]
        [Tooltip("Зона, внутри которой NPC выбирает точки патруля. Если пусто, используется зона спавна или весь water tilemap.")]
        [SerializeField] private NPCPatrolZone patrolZone;

        private Vector2 targetPosition;
        private float patrolTimer;
        private NPCAI npcAI;
        private bool isInitialized;

        private void Awake()
        {
            if (config == null) {
                Debug.LogError($"PatrolTacticConfig не назначен для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"}).", this);
                enabled = false;
                return;
            }

            npcAI = GetComponentInParent<NPCAI>();
            if (npcAI == null) {
                Debug.LogError($"NPCAI не найден для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"}).", this);
                enabled = false;
            }
        }

        public bool CanExecute(EnemyAIContext context)
        {
            return context.Player == null;
        }

        public void SetPatrolZone(NPCPatrolZone zone)
        {
            patrolZone = zone;
            isInitialized = false;
            patrolTimer = 0f;
        }

        public void Execute(EnemyAIContext context, float deltaTime)
        {
            patrolTimer -= deltaTime;
            if (!isInitialized || patrolTimer <= 0f || Vector2.Distance(context.Rigidbody.position, targetPosition) < config.PointReachDistance) {
                SetPatrolTarget(context);
                isInitialized = true;
            }

            npcAI.MoveToSmooth(context, targetPosition, config.PatrolSpeed, config.SmoothTurnSpeed);
        }

        private void SetPatrolTarget(EnemyAIContext context)
        {
            if (patrolZone != null) {
                targetPosition = patrolZone.GetRandomPoint(context.WaterTilemap);
                patrolTimer = Random.Range(config.MinPatrolTime, config.MaxPatrolTime);
                return;
            }

            if (context.WaterTilemap == null) {
                Debug.LogError($"WaterTilemap не назначен для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"}).", this);
                targetPosition = context.Rigidbody.position;
                return;
            }

            Bounds bounds = context.WaterTilemap.localBounds;
            Vector2 randomPoint = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );

            targetPosition = context.WaterTilemap.transform.TransformPoint(randomPoint);
            patrolTimer = Random.Range(config.MinPatrolTime, config.MaxPatrolTime);
        }
    }
}
