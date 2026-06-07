using UnityEngine;

namespace Ship {
    public class HazardDroppingTactic : MonoBehaviour, IEnemyTactic {
        [Header("Минометание")]
        [InspectorLabel("Префаб заграждения")]
        [Tooltip("Префаб бочки, масла или другой временной опасной зоны.")]
        [SerializeField] private GameObject hazardPrefab;
        [InspectorLabel("Интервал сброса")]
        [Tooltip("Как часто NPC оставляет за собой опасную зону.")]
        [SerializeField, Min(0.5f)] private float dropInterval = 1.5f;
        [InspectorLabel("Дистанция впереди игрока")]
        [Tooltip("Насколько далеко впереди курса игрока бандит пытается идти.")]
        [SerializeField, Min(1f)] private float leadDistance = 7f;
        [InspectorLabel("Скорость")]
        [Tooltip("Скорость движения при создании заграждения.")]
        [SerializeField, Min(0.1f)] private float speed = 1f;

        private NPCAI npcAI;
        private float nextDropTime;

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
            Vector2 playerForward = context.Player.up;
            Vector2 leadPoint = BanditTacticUtility.PlayerPosition(context) + playerForward * leadDistance;
            npcAI.MoveToSmooth(context, BanditTacticUtility.ClampToWater(context, leadPoint), speed, 0.3f);

            if (hazardPrefab != null && Time.time >= nextDropTime) {
                Instantiate(hazardPrefab, context.Rigidbody.position - (Vector2)context.Rigidbody.transform.up, Quaternion.identity);
                nextDropTime = Time.time + dropInterval;
            }
        }
    }
}
