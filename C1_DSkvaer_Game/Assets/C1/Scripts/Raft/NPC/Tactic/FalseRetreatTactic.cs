using UnityEngine;

namespace Ship {
    public class FalseRetreatTactic : MonoBehaviour, IEnemyTactic {
        [Header("Ложное бегство")]
        [InspectorLabel("Порог здоровья")]
        [Tooltip("При каком проценте здоровья NPC начинает притворное отступление.")]
        [SerializeField, Range(0.05f, 0.95f)] private float healthThreshold = 0.35f;
        [InspectorLabel("Точка засады")]
        [Tooltip("Куда NPC отступает, чтобы завести игрока в ловушку.")]
        [SerializeField] private Transform ambushPoint;
        [InspectorLabel("Скорость отступления")]
        [Tooltip("Скорость движения к точке засады.")]
        [SerializeField, Min(0.1f)] private float retreatSpeed = 1.25f;
        [InspectorLabel("Дистанция срабатывания ловушки")]
        [Tooltip("Когда игрок подойдет к точке засады ближе этой дистанции, скрытые объекты включатся.")]
        [SerializeField, Min(0.5f)] private float springTrapDistance = 5f;
        [InspectorLabel("Объекты засады")]
        [Tooltip("Корабли, орудия или другие объекты, которые включаются при срабатывании ловушки.")]
        [SerializeField] private GameObject[] ambushObjectsToActivate;

        private NPCAI npcAI;
        private bool trapSprung;

        private void Awake()
        {
            npcAI = GetComponentInParent<NPCAI>();
            SetAmbushActive(false);
        }

        public bool CanExecute(EnemyAIContext context)
        {
            if (npcAI == null || context.Player == null || context.ShipHealth == null || context.ShipHealth.GetMaxShipHealth() <= 0) {
                return false;
            }

            return context.ShipHealth.GetCurrentShipHealth() <= context.ShipHealth.GetMaxShipHealth() * healthThreshold;
        }

        public void Execute(EnemyAIContext context, float deltaTime)
        {
            Vector2 target = ambushPoint != null
                ? (Vector2)ambushPoint.position
                : context.Rigidbody.position + BanditTacticUtility.DirectionFromPlayer(context) * 8f;

            npcAI.MoveToSmooth(context, BanditTacticUtility.ClampToWater(context, target), retreatSpeed, 0.35f);

            if (!trapSprung && Vector2.Distance(context.Player.position, target) <= springTrapDistance) {
                trapSprung = true;
                SetAmbushActive(true);
            }
        }

        private void SetAmbushActive(bool value)
        {
            if (ambushObjectsToActivate == null) {
                return;
            }

            foreach (GameObject obj in ambushObjectsToActivate) {
                if (obj != null) {
                    obj.SetActive(value);
                }
            }
        }
    }
}
