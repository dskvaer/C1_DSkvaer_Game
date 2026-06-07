using UnityEngine;

namespace Ship {
    public class WolfHuntTactic : MonoBehaviour, IEnemyTactic {
        [Header("Волчья охота")]
        [InspectorLabel("Радиус окружения")]
        [Tooltip("На какой дистанции бандит держится вокруг цели во время загона.")]
        [SerializeField, Min(1f)] private float orbitRadius = 12f;
        [InspectorLabel("Скорость кружения")]
        [Tooltip("Скорость движения по окружности вокруг игрока.")]
        [SerializeField, Min(0.1f)] private float orbitSpeed = 1.1f;
        [InspectorLabel("Скорость атаки")]
        [Tooltip("Скорость короткого рывка, когда бандит заходит в корму.")]
        [SerializeField, Min(0.1f)] private float attackSpeed = 1.35f;
        [InspectorLabel("Дистанция удара в корму")]
        [Tooltip("На какой дистанции от игрока бандит начинает атаковать сзади.")]
        [SerializeField, Min(0f)] private float rearAttackDistance = 7f;
        [InspectorLabel("Плавность поворота")]
        [Tooltip("Насколько мягко корабль доворачивает к выбранной точке. 0 - резче, 1 - плавнее.")]
        [SerializeField, Range(0f, 1f)] private float turnSmoothness = 0.35f;

        private NPCAI npcAI;
        private float orbitOffset;

        private void Awake()
        {
            npcAI = GetComponentInParent<NPCAI>();
            orbitOffset = Random.Range(0f, 360f);
        }

        public bool CanExecute(EnemyAIContext context)
        {
            return npcAI != null && context.Player != null;
        }

        public void Execute(EnemyAIContext context, float deltaTime)
        {
            Vector2 playerPos = BanditTacticUtility.PlayerPosition(context);
            float angle = orbitOffset + Time.time * orbitSpeed * 30f;
            Vector2 orbitPoint = playerPos + new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * orbitRadius;
            orbitPoint = BanditTacticUtility.ClampToWater(context, orbitPoint);

            bool behindPlayer = Vector2.Dot(context.Player.up, context.Rigidbody.position - playerPos) < -0.35f;
            float distance = Vector2.Distance(context.Rigidbody.position, playerPos);
            if (behindPlayer && distance <= rearAttackDistance) {
                BanditTacticUtility.FireBroadside(context);
                npcAI.MoveToSmooth(context, context.Rigidbody.position + BanditTacticUtility.DirectionFromPlayer(context) * 2f, attackSpeed, turnSmoothness);
                return;
            }

            npcAI.MoveToSmooth(context, orbitPoint, orbitSpeed, turnSmoothness);
        }
    }
}
