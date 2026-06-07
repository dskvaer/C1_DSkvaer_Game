using UnityEngine;

namespace Ship {
    /// <summary>
    /// Тактика тарана NPC: сближение, удар, короткий отход и повторная атака.
    /// </summary>
    public class RamAttackTactic : MonoBehaviour, IEnemyTactic {
        [Header("Настройки тарана")]
        [InspectorLabel("Конфиг тарана")]
        [Tooltip("ScriptableObject с настройками скорости, дистанций, отхода и перезарядки урона тараном.")]
        [SerializeField] private RamAttackTacticConfig config;

        private enum State { Approach, Ram, Retreat }

        private State currentState = State.Approach;
        private float retreatTimer;
        private float lastDamageTime;
        private NPCAI npcAI;

        private void Awake()
        {
            if (config == null)
            {
                Debug.LogError($"RamAttackTacticConfig не назначен для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"})!", this);
                enabled = false;
                return;
            }

            npcAI = GetComponentInParent<NPCAI>();
            if (npcAI == null)
            {
                Debug.LogError($"NPCAI не найден для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"})!", this);
                enabled = false;
            }
        }

        public bool CanExecute(EnemyAIContext context)
        {
            return context.Player != null
                && context.ShipHealth.GetCurrentShipHealth() > context.ShipHealth.GetMaxShipHealth() * config.HealthThreshold;
        }

        public void Execute(EnemyAIContext context, float deltaTime)
        {
            switch (currentState)
            {
                case State.Approach:
                    Approach(context);
                    break;
                case State.Ram:
                    Ram(context);
                    break;
                case State.Retreat:
                    Retreat(context, deltaTime);
                    break;
            }
        }

        private void Approach(EnemyAIContext context)
        {
            if (context.Player == null)
            {
                return;
            }

            float distanceToPlayer = Vector2.Distance(context.Rigidbody.position, (Vector2)context.Player.position);
            if (distanceToPlayer <= config.RamDistance)
            {
                currentState = State.Ram;
                return;
            }

            Vector2 targetPos = (Vector2)context.Player.position;
            targetPos = npcAI.ClampToTilemapBounds(context, targetPos);
            npcAI.MoveToSmooth(context, targetPos, config.AttackSpeed, config.SmoothTurnSpeed);
        }

        private void Ram(EnemyAIContext context)
        {
            if (context.Player == null)
            {
                currentState = State.Retreat;
                retreatTimer = config.RetreatTime;
                return;
            }

            float distanceToPlayer = Vector2.Distance(context.Rigidbody.position, (Vector2)context.Player.position);
            float currentSpeed = context.Rigidbody.linearVelocity.magnitude;

            if (distanceToPlayer <= config.ContactDistance && currentSpeed >= config.MinRamSpeed)
            {
                if (Time.time - lastDamageTime >= config.DamageCooldown)
                {
                    DealRamDamage(context);
                    ApplyKnockbackToSelf(context);
                    lastDamageTime = Time.time;
                    currentState = State.Retreat;
                    retreatTimer = config.RetreatTime;
                    return;
                }
            }

            Vector2 targetPos = (Vector2)context.Player.position;
            targetPos = npcAI.ClampToTilemapBounds(context, targetPos);
            Vector2 worldDir = (targetPos - context.Rigidbody.position).normalized;
            Vector2 localDir = context.Rigidbody.transform.InverseTransformDirection(worldDir);
            float smoothRotation = Mathf.Lerp(0f, localDir.x, config.SmoothTurnSpeed);
            context.ShipMovement.ShipRotate(new Vector2(Mathf.Clamp(smoothRotation, -1f, 1f), 0f), config.AttackSpeed);
            context.ShipMovement.ShipMove(new Vector2(0f, 1f), config.AttackSpeed);
        }

        private void Retreat(EnemyAIContext context, float deltaTime)
        {
            retreatTimer -= deltaTime;
            if (retreatTimer <= 0)
            {
                currentState = State.Approach;
                return;
            }

            context.ShipMovement.ShipMove(new Vector2(0f, -1f), config.AttackSpeed * 0.6f);
            if (context.Player != null)
            {
                Vector2 directionToPlayer = ((Vector2)context.Player.position - context.Rigidbody.position).normalized;
                Vector2 localDir = context.Rigidbody.transform.InverseTransformDirection(directionToPlayer);
                float smoothRotation = Mathf.Lerp(0f, localDir.x, config.SmoothTurnSpeed);
                context.ShipMovement.ShipRotate(new Vector2(Mathf.Clamp(smoothRotation, -1f, 1f), 0f), config.AttackSpeed);
            }
        }

        private void DealRamDamage(EnemyAIContext context)
        {
            if (context.PlayerHealth == null || context.RamConfig == null)
            {
                return;
            }

            float currentSpeed = context.Rigidbody.linearVelocity.magnitude;
            int damage = Mathf.RoundToInt(context.RamConfig.BaseDamage * (currentSpeed >= context.RamConfig.MinRamSpeed ? context.RamConfig.DamageBoost : 1f) * context.RamConfig.DamageMultiplier);
            context.PlayerHealth.TakeShipDamage(damage, isRam: true);
        }

        private void ApplyKnockbackToSelf(EnemyAIContext context)
        {
            if (context.Rigidbody == null || context.Player == null)
            {
                return;
            }

            Vector2 knockbackDir = (context.Rigidbody.position - (Vector2)context.Player.position).normalized;
            float knockbackForce = context.RamConfig.BaseKnockbackForce * context.RamConfig.RamKnockbackMultiplier * 0.5f;
            IShipDamageable damageable = context.Rigidbody.GetComponent<IShipDamageable>();
            if (damageable != null)
            {
                damageable.ApplyShipKnockback(knockbackDir * knockbackForce);
            }
        }
    }
}
