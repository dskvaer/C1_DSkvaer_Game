using UnityEngine;

namespace Ship {
    public class SniperHarassTactic : MonoBehaviour, IEnemyTactic {
        [Header("Снайперский измор")]
        [InspectorLabel("Желаемая дистанция")]
        [Tooltip("Дистанция, которую снайпер старается держать от игрока.")]
        [SerializeField, Min(1f)] private float preferredRange = 15f;
        [InspectorLabel("Дистанция бегства")]
        [Tooltip("Если игрок подходит ближе, снайпер отступает.")]
        [SerializeField, Min(0.5f)] private float fleeRange = 8f;
        [InspectorLabel("Скорость кайта")]
        [Tooltip("Скорость удержания дистанции и отхода.")]
        [SerializeField, Min(0.1f)] private float kiteSpeed = 1.25f;
        [InspectorLabel("Перезарядка выстрела по мачтам")]
        [Tooltip("Пауза между попытками замедлить игрока через сообщение по мачтам.")]
        [SerializeField, Min(0f)] private float mastShotCooldown = 5f;
        [InspectorLabel("Сообщение замедления")]
        [Tooltip("Имя метода, который вызывается на игроке для замедления мачт.")]
        [SerializeField] private string mastSlowMessage = "ApplyMastSlow";

        private NPCAI npcAI;
        private float nextMastShotTime;

        private void Awake()
        {
            npcAI = GetComponentInParent<NPCAI>();
        }

        public bool CanExecute(EnemyAIContext context)
        {
            return npcAI != null && context.Player != null;
        }

        public void Execute(EnemyAIContext context, float deltaTime)
        {
            Vector2 fromPlayer = BanditTacticUtility.DirectionFromPlayer(context);
            float distance = Vector2.Distance(context.Rigidbody.position, context.Player.position);
            Vector2 target = distance < fleeRange
                ? context.Rigidbody.position + fromPlayer * preferredRange
                : BanditTacticUtility.PlayerPosition(context) + fromPlayer * preferredRange;

            npcAI.MoveToSmooth(context, BanditTacticUtility.ClampToWater(context, target), kiteSpeed, 0.3f);
            BanditTacticUtility.FireBroadside(context);

            if (Time.time >= nextMastShotTime) {
                context.Player.SendMessage(mastSlowMessage, SendMessageOptions.DontRequireReceiver);
                nextMastShotTime = Time.time + mastShotCooldown;
            }
        }
    }
}
