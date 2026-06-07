using UnityEngine;

namespace Ship {
    public class HookAndRamTactic : MonoBehaviour, IEnemyTactic {
        [Header("Крюк и таран")]
        [InspectorLabel("Скорость тарана")]
        [Tooltip("Скорость диагонального сближения для тарана или абордажного крюка.")]
        [SerializeField, Min(0.1f)] private float ramSpeed = 1.6f;
        [InspectorLabel("Боковая точка захода")]
        [Tooltip("На каком расстоянии сбоку от игрока NPC пытается зайти перед ударом.")]
        [SerializeField, Min(0.5f)] private float sideAimDistance = 4f;
        [InspectorLabel("Дистанция крюка")]
        [Tooltip("Дистанция, на которой отправляется сообщение оглушения/зацепа игроку.")]
        [SerializeField, Min(0.5f)] private float hookRange = 3f;
        [InspectorLabel("Перезарядка крюка")]
        [Tooltip("Пауза между попытками зацепить игрока.")]
        [SerializeField, Min(0f)] private float hookCooldown = 4f;
        [InspectorLabel("Сообщение крюка")]
        [Tooltip("Имя метода, который будет вызван на игроке через SendMessage.")]
        [SerializeField] private string hookMessage = "ApplyStun";

        private NPCAI npcAI;
        private float nextHookTime;
        private int side = 1;

        private void Awake()
        {
            npcAI = GetComponentInParent<NPCAI>();
            side = Random.value < 0.5f ? -1 : 1;
        }

        public bool CanExecute(EnemyAIContext context)
        {
            return npcAI != null && context.Player != null;
        }

        public void Execute(EnemyAIContext context, float deltaTime)
        {
            Vector2 toPlayer = BanditTacticUtility.DirectionToPlayer(context);
            Vector2 sidePoint = BanditTacticUtility.PlayerPosition(context) + BanditTacticUtility.Perpendicular(toPlayer, side) * sideAimDistance;
            npcAI.MoveToSmooth(context, BanditTacticUtility.ClampToWater(context, sidePoint), ramSpeed, 0.55f);

            float distance = Vector2.Distance(context.Rigidbody.position, context.Player.position);
            if (distance <= hookRange && Time.time >= nextHookTime) {
                context.Player.SendMessage(hookMessage, SendMessageOptions.DontRequireReceiver);
                nextHookTime = Time.time + hookCooldown;
                side *= -1;
            }
        }
    }
}
