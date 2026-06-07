using UnityEngine;

namespace Ship {
    public class FireShipBlockadeTactic : MonoBehaviour, IEnemyTactic {
        [Header("Блокировка брандером")]
        [InspectorLabel("Префаб брандера")]
        [Tooltip("Префаб старого взрывающегося судна, которое выпускается в сторону игрока.")]
        [SerializeField] private GameObject fireShipPrefab;
        [InspectorLabel("Дистанция запуска")]
        [Tooltip("Максимальная дистанция до игрока, на которой тактика может сработать.")]
        [SerializeField, Min(1f)] private float launchDistance = 12f;
        [InspectorLabel("Перезарядка запуска")]
        [Tooltip("Пауза между выпусками брандеров.")]
        [SerializeField, Min(0.5f)] private float launchCooldown = 8f;
        [InspectorLabel("Боковое смещение")]
        [Tooltip("На какое расстояние основной корабль уходит в сторону для залпа бортом.")]
        [SerializeField, Min(1f)] private float broadsideOffset = 7f;
        [InspectorLabel("Скорость занятия позиции")]
        [Tooltip("Скорость движения основного корабля к выгодной боковой позиции.")]
        [SerializeField, Min(0.1f)] private float positioningSpeed = 0.9f;

        private NPCAI npcAI;
        private float nextLaunchTime;
        private int side = 1;

        private void Awake()
        {
            npcAI = GetComponentInParent<NPCAI>();
            side = Random.value < 0.5f ? -1 : 1;
        }

        public bool CanExecute(EnemyAIContext context)
        {
            return npcAI != null && context.Player != null && Vector2.Distance(context.Rigidbody.position, context.Player.position) <= launchDistance;
        }

        public void Execute(EnemyAIContext context, float deltaTime)
        {
            Vector2 toPlayer = BanditTacticUtility.DirectionToPlayer(context);
            Vector2 flank = BanditTacticUtility.PlayerPosition(context) - toPlayer * launchDistance * 0.5f + BanditTacticUtility.Perpendicular(toPlayer, side) * broadsideOffset;
            npcAI.MoveToSmooth(context, BanditTacticUtility.ClampToWater(context, flank), positioningSpeed, 0.35f);

            if (Time.time >= nextLaunchTime) {
                LaunchFireShip(context, toPlayer);
                nextLaunchTime = Time.time + launchCooldown;
                side *= -1;
            }
        }

        private void LaunchFireShip(EnemyAIContext context, Vector2 direction)
        {
            if (fireShipPrefab == null) {
                return;
            }

            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);
            Instantiate(fireShipPrefab, context.Rigidbody.position + direction * 1.5f, rotation);
        }
    }
}
