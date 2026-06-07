using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Ship {
    /// <summary>
    /// Central enemy AI controller. It gathers ship components, keeps target context
    /// up to date, and runs the first tactic that can act in the current situation.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class NPCAI : MonoBehaviour {
        [Header("Обнаружение")]
        [InspectorLabel("Зона обнаружения")]
        [Tooltip("Триггер, который фиксирует вход цели в радиус обзора NPC.")]
        [SerializeField] private Collider2D detectionArea;
        [InspectorLabel("Радиус обзора")]
        [Tooltip("Радиус круговой зоны обнаружения, если используется CircleCollider2D.")]
        [SerializeField, Min(0.1f)] private float detectionRadius = 10f;
        [InspectorLabel("Слой игрока")]
        [Tooltip("Слой, по которому AI определяет корабль игрока.")]
        [SerializeField] private LayerMask playerLayer = -1;

        [Header("Мир")]
        [InspectorLabel("Карта воды")]
        [Tooltip("Tilemap воды, по границам которого NPC ограничивает движение.")]
        [SerializeField] private Tilemap waterTilemap;

        [Header("Бой")]
        [InspectorLabel("Настройки тарана")]
        [Tooltip("Конфиг урона и параметров тарана для тактик ближнего боя.")]
        [SerializeField] private ShipRamConfig ramConfig;
        [InspectorLabel("Система оружия")]
        [Tooltip("Система бортовых пушек корабля, если NPC использует стрельбу.")]
        [SerializeField] private ShipWeaponSystem weaponSystem;

        [Header("Тактики")]
        [InspectorLabel("Список тактик")]
        [Tooltip("Порядок приоритета. Если список пустой, тактики собираются с этого объекта и дочерних объектов.")]
        [SerializeField] private List<MonoBehaviour> tactics = new();
        [InspectorLabel("Интервал решений")]
        [Tooltip("Как часто AI пересматривает активную тактику.")]
        [SerializeField, Min(0.02f)] private float decisionInterval = 0.1f;
        [InspectorLabel("Профиль поведения")]
        [Tooltip("ScriptableObject с весами, приоритетами и задержками тактик.")]
        [SerializeField] private EnemyBehaviorProfileConfig behaviorProfile;

        private IShipMovable shipMovement;
        private IShipHealth shipHealth;
        private Rigidbody2D rb;
        private Transform player;
        private IShipHealth playerHealth;
        private EnemyAIContext context;
        private readonly List<IEnemyTactic> validTactics = new();
        private readonly Dictionary<EnemyTacticKind, float> tacticCooldownUntil = new();
        private IEnemyTactic activeTactic;
        private float decisionTimer;

        private void Awake()
        {
            if (!TryInitializeRequiredComponents()) {
                enabled = false;
                return;
            }

            ConfigureDetectionArea();
            CollectTactics();

            context = new EnemyAIContext {
                ShipMovement = shipMovement,
                ShipHealth = shipHealth,
                Rigidbody = rb,
                WaterTilemap = waterTilemap,
                RamConfig = ramConfig,
                WeaponSystem = weaponSystem
            };

            ;
        }

        private void Update()
        {
            if (shipHealth == null || shipHealth.GetCurrentShipHealth() <= 0f) {
                StopShip();
                return;
            }

            RefreshTargetIfNeeded();

            context.Player = player;
            context.PlayerHealth = playerHealth;

            decisionTimer -= Time.deltaTime;
            IEnemyTactic nextTactic = activeTactic;

            if (decisionTimer <= 0f || nextTactic == null || !nextTactic.CanExecute(context)) {
                nextTactic = SelectNextTactic();
                decisionTimer = decisionInterval;
            }

            if (nextTactic == null) {
                if (activeTactic != null) {
                    activeTactic = null;
                    StopShip();
                }
                return;
            }

            if (activeTactic != nextTactic) {
                activeTactic = nextTactic;
                ;
            }

            activeTactic.Execute(context, Time.deltaTime);
        }

        public void SetTactics(IEnumerable<MonoBehaviour> newTactics)
        {
            tactics = newTactics?.Where(tactic => tactic != null).ToList() ?? new List<MonoBehaviour>();
            CollectTactics();
            activeTactic = null;
            decisionTimer = 0f;
        }

        public void SetDecisionInterval(float interval)
        {
            decisionInterval = Mathf.Max(0.02f, interval);
        }

        public void SetBehaviorProfile(EnemyBehaviorProfileConfig profile)
        {
            behaviorProfile = profile;
            tacticCooldownUntil.Clear();
            activeTactic = null;
            decisionTimer = 0f;
        }

        private IEnemyTactic SelectNextTactic()
        {
            if (behaviorProfile != null) {
                IEnemyTactic profiledTactic = SelectProfiledTactic();
                if (profiledTactic != null) {
                    return profiledTactic;
                }
            }

            for (int i = 0; i < validTactics.Count; i++) {
                IEnemyTactic tactic = validTactics[i];
                if (tactic.CanExecute(context)) {
                    return tactic;
                }
            }

            return null;
        }

        private IEnemyTactic SelectProfiledTactic()
        {
            var candidates = new List<(IEnemyTactic tactic, EnemyBehaviorTacticSlot slot)>();

            for (int i = 0; i < validTactics.Count; i++) {
                IEnemyTactic tactic = validTactics[i];
                EnemyTacticKind kind = GetTacticKind(tactic);
                EnemyBehaviorTacticSlot slot = GetSlot(kind);

                if (slot == null || !slot.Enabled || slot.Weight <= 0f) {
                    continue;
                }

                if (tacticCooldownUntil.TryGetValue(kind, out float cooldownEndTime) && Time.time < cooldownEndTime) {
                    continue;
                }

                if (tactic.CanExecute(context)) {
                    candidates.Add((tactic, slot));
                }
            }

            if (candidates.Count == 0) {
                return null;
            }

            int bestPriority = candidates.Min(candidate => candidate.slot.Priority);
            candidates = candidates.Where(candidate => candidate.slot.Priority == bestPriority).ToList();

            float totalWeight = candidates.Sum(candidate => candidate.slot.Weight);
            float roll = Random.Range(0f, totalWeight);

            foreach ((IEnemyTactic tactic, EnemyBehaviorTacticSlot slot) in candidates) {
                roll -= slot.Weight;
                if (roll <= 0f) {
                    SetTacticCooldown(tactic, slot);
                    return tactic;
                }
            }

            SetTacticCooldown(candidates[0].tactic, candidates[0].slot);
            return candidates[0].tactic;
        }

        private EnemyBehaviorTacticSlot GetSlot(EnemyTacticKind kind)
        {
            if (behaviorProfile == null) {
                return null;
            }

            foreach (EnemyBehaviorTacticSlot slot in behaviorProfile.Tactics) {
                if (slot != null && slot.Kind == kind) {
                    return slot;
                }
            }

            return null;
        }

        private void SetTacticCooldown(IEnemyTactic tactic, EnemyBehaviorTacticSlot slot)
        {
            if (slot.Cooldown <= 0f) {
                return;
            }

            tacticCooldownUntil[GetTacticKind(tactic)] = Time.time + slot.Cooldown;
        }

        private static EnemyTacticKind GetTacticKind(IEnemyTactic tactic)
        {
            return tactic switch {
                PatrolTactic => EnemyTacticKind.Patrol,
                RamAttackTactic => EnemyTacticKind.RamAttack,
                NPCShootingTactic => EnemyTacticKind.Shooting,
                FleeTactic => EnemyTacticKind.Flee,
                WolfHuntTactic => EnemyTacticKind.WolfHunt,
                DeadRockAmbushTactic => EnemyTacticKind.DeadRockAmbush,
                FireShipBlockadeTactic => EnemyTacticKind.FireShipBlockade,
                HookAndRamTactic => EnemyTacticKind.HookAndRam,
                HazardDroppingTactic => EnemyTacticKind.HazardDropping,
                SniperHarassTactic => EnemyTacticKind.SniperHarass,
                ShallowsLureTactic => EnemyTacticKind.ShallowsLure,
                FalseRetreatTactic => EnemyTacticKind.FalseRetreat,
                SmokeVeilTactic => EnemyTacticKind.SmokeVeil,
                SuicideExplosionTactic => EnemyTacticKind.SuicideExplosion,
                _ => EnemyTacticKind.Custom
            };
        }

        private bool TryInitializeRequiredComponents()
        {
            shipMovement = GetComponent<IShipMovable>();
            if (shipMovement == null) {
                Debug.LogError($"[NPCAI] IShipMovable not found on {name}.", this);
                return false;
            }

            shipHealth = GetComponent<IShipHealth>();
            if (shipHealth == null) {
                Debug.LogError($"[NPCAI] IShipHealth not found on {name}.", this);
                return false;
            }

            rb = GetComponent<Rigidbody2D>();
            if (rb == null) {
                Debug.LogError($"[NPCAI] Rigidbody2D not found on {name}.", this);
                return false;
            }

            if (waterTilemap == null) {
                waterTilemap = FindAnyObjectByType<Tilemap>();
                if (waterTilemap == null) {
                    Debug.LogError($"[NPCAI] Water Tilemap is not assigned for {name}.", this);
                    return false;
                }
            }

            if (weaponSystem == null) {
                weaponSystem = GetComponent<ShipWeaponSystem>();
            }

            if (ramConfig == null) {
                ;
            }

            return true;
        }

        private void ConfigureDetectionArea()
        {
            if (detectionArea == null) {
                var detectionObject = new GameObject("DetectionArea");
                detectionObject.transform.SetParent(transform, false);
                detectionArea = detectionObject.AddComponent<CircleCollider2D>();
            }

            detectionArea.isTrigger = true;

            if (detectionArea is CircleCollider2D circleCollider) {
                circleCollider.radius = detectionRadius;
            }
            else {
                ;
            }
        }

        private void CollectTactics()
        {
            validTactics.Clear();

            if (tactics == null) {
                tactics = new List<MonoBehaviour>();
            }

            if (tactics.Count == 0) {
                MonoBehaviour[] behaviours = GetComponentsInChildren<MonoBehaviour>(true);
                foreach (MonoBehaviour behaviour in behaviours) {
                    if (behaviour != this && behaviour is IEnemyTactic) {
                        tactics.Add(behaviour);
                    }
                }
            }

            foreach (MonoBehaviour tacticBehaviour in tactics) {
                if (tacticBehaviour == null) {
                    continue;
                }

                if (tacticBehaviour is IEnemyTactic enemyTactic) {
                    validTactics.Add(enemyTactic);
                }
                else {
                    ;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TrySetPlayer(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (player == null) {
                TrySetPlayer(other);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (player != null && (other.transform == player || other.transform.IsChildOf(player))) {
                ClearPlayer();
            }
        }

        private void TrySetPlayer(Collider2D other)
        {
            if (!IsInPlayerLayer(other.gameObject.layer) && !other.CompareTag("Player")) {
                return;
            }

            Transform targetTransform = ResolvePlayerRoot(other);
            if (targetTransform == null || !targetTransform.CompareTag("Player")) {
                return;
            }

            player = targetTransform;
            playerHealth = player.GetComponent<IShipHealth>() ?? player.GetComponentInChildren<IShipHealth>();
            ;
        }

        private Transform ResolvePlayerRoot(Collider2D other)
        {
            if (other.CompareTag("Player")) {
                return other.transform;
            }

            IShipHealth health = other.GetComponentInParent<IShipHealth>();
            if (health is Component healthComponent && healthComponent.CompareTag("Player")) {
                return healthComponent.transform;
            }

            Transform root = other.transform.root;
            return root.CompareTag("Player") ? root : null;
        }

        private void RefreshTargetIfNeeded()
        {
            if (player == null || player.gameObject.activeInHierarchy) {
                return;
            }

            ClearPlayer();
        }

        private void ClearPlayer()
        {
            player = null;
            playerHealth = null;
            activeTactic = null;
            StopShip();
            ;
        }

        private bool IsInPlayerLayer(int layer)
        {
            return (playerLayer.value & (1 << layer)) != 0;
        }

        private void StopShip()
        {
            shipMovement?.ShipMove(Vector2.zero, 0f);
            shipMovement?.ShipRotate(Vector2.zero, 0f);
        }

        public void MoveToSmooth(EnemyAIContext aiContext, Vector2 target, float speed, float smoothTurnSpeed = 0.3f)
        {
            if (aiContext.Rigidbody == null || aiContext.ShipMovement == null) {
                return;
            }

            Vector2 toTarget = target - aiContext.Rigidbody.position;
            if (toTarget.sqrMagnitude < 0.04f) {
                StopShip();
                return;
            }

            Vector2 direction = toTarget.normalized;
            Vector2 localDirection = aiContext.Rigidbody.transform.InverseTransformDirection(direction);
            float turnInput = Mathf.Lerp(0f, localDirection.x, smoothTurnSpeed);

            aiContext.ShipMovement.ShipRotate(new Vector2(Mathf.Clamp(turnInput, -1f, 1f), 0f), speed);
            aiContext.ShipMovement.ShipMove(new Vector2(0f, Mathf.Clamp(localDirection.y, -1f, 1f)), speed);
        }

        public Vector2 ClampToTilemapBounds(EnemyAIContext aiContext, Vector2 position)
        {
            if (aiContext.WaterTilemap == null) {
                return position;
            }

            Bounds bounds = aiContext.WaterTilemap.localBounds;
            Vector3 min = aiContext.WaterTilemap.transform.TransformPoint(bounds.min);
            Vector3 max = aiContext.WaterTilemap.transform.TransformPoint(bounds.max);

            return new Vector2(
                Mathf.Clamp(position.x, min.x, max.x),
                Mathf.Clamp(position.y, min.y, max.y)
            );
        }
    }
}
