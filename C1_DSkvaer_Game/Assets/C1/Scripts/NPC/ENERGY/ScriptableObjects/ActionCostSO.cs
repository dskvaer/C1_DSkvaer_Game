using UnityEngine;

namespace NPC.Characters.Player.Energy {
    [CreateAssetMenu(fileName = "ActionCost", menuName = "Character/ActionCost", order = 3)]
    public class ActionCostSO : ScriptableObject {
        [Header("Удерживаемые действия")]
        [InspectorLabel("Блок в секунду")]
        [Tooltip("Расход энергии за каждую секунду удержания блока.")]
        [SerializeField] private float blockCostPerSecond = 3f;

        [InspectorLabel("Движение в секунду")]
        [Tooltip("Расход энергии за каждую секунду активного движения.")]
        [SerializeField] private float moveCostPerSecond = 2f;

        [InspectorLabel("Толкание в секунду")]
        [Tooltip("Расход энергии за каждую секунду толкания объекта.")]
        [SerializeField] private float pushCostPerSecond = 4f;

        [Header("Разовые действия")]
        [InspectorLabel("Атака")]
        [Tooltip("Разовый расход энергии при атаке.")]
        [SerializeField] private float attackSingleCost = 10f;

        [InspectorLabel("Прыжок")]
        [Tooltip("Разовый расход энергии при прыжке.")]
        [SerializeField] private float jumpSingleCost = 10f;

        [Header("Особые расходы")]
        [InspectorLabel("Удар по блоку")]
        [Tooltip("Дополнительный расход энергии, когда персонаж получает удар во время блока.")]
        [SerializeField] private float blockHitCost = 5f;

        public float BlockCostPerSecond => blockCostPerSecond;
        public float BlockHitCost => blockHitCost;
        public float AttackSingleCost => attackSingleCost;
        public float MoveCostPerSecond => moveCostPerSecond;
        public float JumpSingleCost => jumpSingleCost;
        public float PushCostPerSecond => pushCostPerSecond;

        private void OnValidate()
        {
            blockCostPerSecond = Mathf.Max(0f, blockCostPerSecond);
            moveCostPerSecond = Mathf.Max(0f, moveCostPerSecond);
            pushCostPerSecond = Mathf.Max(0f, pushCostPerSecond);
            attackSingleCost = Mathf.Max(0f, attackSingleCost);
            jumpSingleCost = Mathf.Max(0f, jumpSingleCost);
            blockHitCost = Mathf.Max(0f, blockHitCost);
        }
    }
}
