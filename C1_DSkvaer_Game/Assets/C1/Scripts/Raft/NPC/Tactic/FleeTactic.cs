using UnityEngine;

namespace Ship {
    /// <summary>
    /// Побег NPC при низком здоровье: к союзникам, либо с постепенной потерей скорости и последним ударом.
    /// </summary>
    public class FleeTactic : MonoBehaviour, IEnemyTactic {
        [Header("Настройки бегства")]
        [InspectorLabel("Конфиг бегства")]
        [Tooltip("ScriptableObject с порогом здоровья, поиском союзников, усталостью и последним ударом.")]
        [SerializeField] private FleeTacticConfig config;

        private readonly Collider2D[] allyResults = new Collider2D[24];
        private NPCAI npcAI;
        private float fleeTime;
        private bool lastStandStarted;
        private bool lastStandFinished;
        private bool lastStandDamageApplied;
        private float lastStandTimer;
        private Vector2 lastStandDirection;

        private void Awake()
        {
            if (config == null) {
                Debug.LogError($"FleeTacticConfig не назначен для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"})!", this);
                enabled = false;
                return;
            }

            npcAI = GetComponentInParent<NPCAI>();
            if (npcAI == null) {
                Debug.LogError($"NPCAI не найден для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"})!", this);
                enabled = false;
            }
        }

        public bool CanExecute(EnemyAIContext context)
        {
            return !lastStandFinished
                && context.Player != null
                && context.ShipHealth.GetCurrentShipHealth() <= context.ShipHealth.GetMaxShipHealth() * config.HealthThreshold;
        }

        public void Execute(EnemyAIContext context, float deltaTime)
        {
            if (context.Player == null) {
                return;
            }

            if (lastStandStarted) {
                ExecuteLastStandRam(context, deltaTime);
                return;
            }

            fleeTime += deltaTime;
            Transform ally = FindNearestAlly(context);
            bool hasAlly = ally != null;
            float currentSpeed = hasAlly ? config.FleeSpeed : GetExhaustedSpeed();
            Vector2 targetPosition = hasAlly
                ? (Vector2)ally.position
                : GetPointAwayFromPlayer(context);

            npcAI.MoveToSmooth(context, npcAI.ClampToTilemapBounds(context, targetPosition), currentSpeed, config.SmoothTurnSpeed);

            if (CanStartLastStandRam(context, currentSpeed)) {
                StartLastStandRam(context);
            }
        }

        private float GetExhaustedSpeed()
        {
            float t = Mathf.Clamp01(fleeTime / Mathf.Max(0.1f, config.ExhaustionTime));
            float multiplier = Mathf.Lerp(1f, config.ExhaustedSpeedMultiplier, t);
            return config.FleeSpeed * multiplier;
        }

        private Vector2 GetPointAwayFromPlayer(EnemyAIContext context)
        {
            Vector2 fromPlayer = ((Vector2)context.Rigidbody.position - (Vector2)context.Player.position).normalized;
            if (fromPlayer.sqrMagnitude <= 0.001f) {
                fromPlayer = context.Rigidbody.transform.up;
            }

            return context.Rigidbody.position + fromPlayer * config.FleeDistance;
        }

        private Transform FindNearestAlly(EnemyAIContext context)
        {
            var filter = new ContactFilter2D {
                useLayerMask = true,
                layerMask = config.AllyLayer,
                useTriggers = true
            };

            int count = Physics2D.OverlapCircle(context.Rigidbody.position, config.AllySearchRadius, filter, allyResults);
            Transform best = null;
            float bestDistance = float.MaxValue;
            Transform selfRoot = transform.root;

            for (int i = 0; i < count; i++) {
                Collider2D hit = allyResults[i];
                if (hit == null || hit.transform == selfRoot || hit.transform.IsChildOf(selfRoot)) {
                    continue;
                }

                Transform candidate = ResolveAllyRoot(hit);
                if (candidate == null || candidate == selfRoot) {
                    continue;
                }

                float distance = ((Vector2)candidate.position - context.Rigidbody.position).sqrMagnitude;
                if (distance < bestDistance) {
                    bestDistance = distance;
                    best = candidate;
                }
            }

            return best;
        }

        private static Transform ResolveAllyRoot(Collider2D hit)
        {
            IShipHealth health = hit.GetComponentInParent<IShipHealth>();
            if (health is Component healthComponent && healthComponent.CompareTag("Enemy")) {
                return healthComponent.transform;
            }

            Transform root = hit.transform.root;
            return root.CompareTag("Enemy") ? root : null;
        }

        private bool CanStartLastStandRam(EnemyAIContext context, float currentSpeed)
        {
            if (!config.AllowLastStandRam || lastStandStarted || lastStandFinished || context.PlayerHealth == null) {
                return false;
            }

            bool exhausted = currentSpeed <= config.FleeSpeed * (config.ExhaustedSpeedMultiplier + 0.01f);
            bool playerWeak = context.PlayerHealth.GetCurrentShipHealth() <= context.PlayerHealth.GetMaxShipHealth() * config.PlayerLastStandHealthThreshold;
            return exhausted && playerWeak;
        }

        private void StartLastStandRam(EnemyAIContext context)
        {
            lastStandStarted = true;
            lastStandTimer = config.LastStandRamDuration;
            lastStandDirection = ((Vector2)context.Player.position - context.Rigidbody.position).normalized;
            if (lastStandDirection.sqrMagnitude <= 0.001f) {
                lastStandDirection = context.Rigidbody.transform.up;
            }
        }

        private void ExecuteLastStandRam(EnemyAIContext context, float deltaTime)
        {
            lastStandTimer -= deltaTime;
            context.Rigidbody.transform.up = lastStandDirection;
            context.ShipMovement.ShipRotate(Vector2.zero, 0f);
            context.ShipMovement.ShipMove(new Vector2(0f, 1f), config.LastStandRamSpeed);

            if (!lastStandDamageApplied && Vector2.Distance(context.Rigidbody.position, context.Player.position) <= config.LastStandContactDistance) {
                context.PlayerHealth?.TakeShipDamage(config.LastStandDamage, isRam: true);
                lastStandDamageApplied = true;
            }

            if (lastStandTimer <= 0f) {
                lastStandFinished = true;
            }
        }
    }
}
