using UnityEngine;

namespace Ship {
    public class DeadRockAmbushTactic : MonoBehaviour, IEnemyTactic {
        [Header("Засада Мертвая скала")]
        [InspectorLabel("Дистанция срабатывания")]
        [Tooltip("На какой дистанции от игрока корабль выходит из засады.")]
        [SerializeField, Min(0.5f)] private float triggerDistance = 7f;
        [InspectorLabel("Длительность атаки")]
        [Tooltip("Сколько секунд бандит остается в режиме внезапной атаки после срабатывания.")]
        [SerializeField, Min(0.5f)] private float ambushDuration = 5f;
        [InspectorLabel("Скорость рывка")]
        [Tooltip("Скорость сближения после раскрытия засады.")]
        [SerializeField, Min(0.1f)] private float burstSpeed = 1.5f;
        [InspectorLabel("Плавность поворота")]
        [Tooltip("Плавность разворота к цели во время атаки.")]
        [SerializeField, Range(0f, 1f)] private float turnSmoothness = 0.45f;
        [InspectorLabel("Скрываемые спрайты")]
        [Tooltip("SpriteRenderer корабля или частей корабля, которые отключаются до начала засады.")]
        [SerializeField] private SpriteRenderer[] hiddenRenderers;

        private NPCAI npcAI;
        private bool triggered;
        private float triggeredUntil;

        private void Awake()
        {
            npcAI = GetComponentInParent<NPCAI>();
            SetHidden(true);
        }

        public bool CanExecute(EnemyAIContext context)
        {
            if (npcAI == null || context.Player == null) {
                return false;
            }

            if (triggered) {
                return Time.time <= triggeredUntil;
            }

            if (Vector2.Distance(context.Rigidbody.position, context.Player.position) <= triggerDistance) {
                triggered = true;
                triggeredUntil = Time.time + ambushDuration;
                SetHidden(false);
                return true;
            }

            return true;
        }

        public void Execute(EnemyAIContext context, float deltaTime)
        {
            if (!triggered) {
                context.ShipMovement.ShipMove(Vector2.zero, 0f);
                context.ShipMovement.ShipRotate(Vector2.zero, 0f);
                return;
            }

            Vector2 target = BanditTacticUtility.PlayerPosition(context);
            npcAI.MoveToSmooth(context, target, burstSpeed, turnSmoothness);
            BanditTacticUtility.FireBroadside(context);
        }

        private void SetHidden(bool hidden)
        {
            if (hiddenRenderers == null) {
                return;
            }

            foreach (SpriteRenderer renderer in hiddenRenderers) {
                if (renderer != null) {
                    renderer.enabled = !hidden;
                }
            }
        }
    }
}
