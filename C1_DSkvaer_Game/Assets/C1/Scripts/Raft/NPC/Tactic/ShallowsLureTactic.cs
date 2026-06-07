using UnityEngine;

namespace Ship {
    public class ShallowsLureTactic : MonoBehaviour, IEnemyTactic {
        [Header("Туман и отмели")]
        [InspectorLabel("Точки заманивания")]
        [Tooltip("Маршрут через рифы, проливы или отмели, куда бандит уводит игрока.")]
        [SerializeField] private Transform[] lurePoints;
        [InspectorLabel("Скорость")]
        [Tooltip("Скорость движения между точками заманивания.")]
        [SerializeField, Min(0.1f)] private float speed = 1.1f;
        [InspectorLabel("Дистанция достижения")]
        [Tooltip("На каком расстоянии от точки NPC переключается на следующую.")]
        [SerializeField, Min(0.5f)] private float pointReachDistance = 1.2f;

        private NPCAI npcAI;
        private int currentPoint;

        private void Awake()
        {
            npcAI = GetComponentInParent<NPCAI>();
        }

        public bool CanExecute(EnemyAIContext context)
        {
            return npcAI != null && context.Player != null && lurePoints != null && lurePoints.Length > 0;
        }

        public void Execute(EnemyAIContext context, float deltaTime)
        {
            Transform point = lurePoints[currentPoint];
            if (point == null) {
                currentPoint = (currentPoint + 1) % lurePoints.Length;
                return;
            }

            Vector2 target = BanditTacticUtility.ClampToWater(context, point.position);
            npcAI.MoveToSmooth(context, target, speed, 0.45f);

            if (Vector2.Distance(context.Rigidbody.position, target) <= pointReachDistance) {
                currentPoint = (currentPoint + 1) % lurePoints.Length;
            }
        }
    }
}
