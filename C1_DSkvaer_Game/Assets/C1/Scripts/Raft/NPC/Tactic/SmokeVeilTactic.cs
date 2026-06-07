using UnityEngine;

namespace Ship {
    public class SmokeVeilTactic : MonoBehaviour, IEnemyTactic {
        [Header("Дымовая завеса")]
        [InspectorLabel("Префаб дыма")]
        [Tooltip("Префаб облака дыма, которое скрывает корабль.")]
        [SerializeField] private GameObject smokePrefab;
        [InspectorLabel("Скрываемые спрайты")]
        [Tooltip("Спрайты корабля, которые выключаются, пока NPC находится в дыму.")]
        [SerializeField] private SpriteRenderer[] renderersToFade;
        [InspectorLabel("Перезарядка дыма")]
        [Tooltip("Пауза между постановками дымовой завесы.")]
        [SerializeField, Min(0.5f)] private float smokeCooldown = 7f;
        [InspectorLabel("Длительность дыма")]
        [Tooltip("Сколько секунд NPC считается скрытым после выпуска дыма.")]
        [SerializeField, Min(0.5f)] private float smokeDuration = 3f;
        [InspectorLabel("Скорость смены позиции")]
        [Tooltip("Скорость ухода в сторону внутри дымовой завесы.")]
        [SerializeField, Min(0.1f)] private float repositionSpeed = 1.35f;
        [InspectorLabel("Дистанция смены позиции")]
        [Tooltip("Насколько далеко NPC смещается в сторону после выпуска дыма.")]
        [SerializeField, Min(1f)] private float repositionDistance = 6f;

        private NPCAI npcAI;
        private float nextSmokeTime;
        private float hiddenUntil;
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
            if (Time.time >= nextSmokeTime) {
                SpawnSmoke(context);
                hiddenUntil = Time.time + smokeDuration;
                nextSmokeTime = Time.time + smokeCooldown;
                side *= -1;
            }

            bool hidden = Time.time < hiddenUntil;
            SetRenderersVisible(!hidden);

            Vector2 toPlayer = BanditTacticUtility.DirectionToPlayer(context);
            Vector2 reposition = context.Rigidbody.position + BanditTacticUtility.Perpendicular(toPlayer, side) * repositionDistance;
            npcAI.MoveToSmooth(context, BanditTacticUtility.ClampToWater(context, reposition), repositionSpeed, 0.45f);

            if (!hidden) {
                BanditTacticUtility.FireBroadside(context);
            }
        }

        private void SpawnSmoke(EnemyAIContext context)
        {
            if (smokePrefab != null) {
                Instantiate(smokePrefab, context.Rigidbody.position, Quaternion.identity);
            }
        }

        private void SetRenderersVisible(bool visible)
        {
            if (renderersToFade == null) {
                return;
            }

            foreach (SpriteRenderer renderer in renderersToFade) {
                if (renderer != null) {
                    renderer.enabled = visible;
                }
            }
        }
    }
}
