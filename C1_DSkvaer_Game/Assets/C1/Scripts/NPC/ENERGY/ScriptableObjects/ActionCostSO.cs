using UnityEngine;

namespace NPC.Characters.Player.Energy {
    [CreateAssetMenu(fileName = "ActionCost", menuName = "Player/ActionCost", order = 3)]
    public class ActionCostSO : ScriptableObject {
        [Header("Held Actions (per second)")]
        [SerializeField] private float blockCostPerSecond = 3f;
        [SerializeField] private float moveCostPerSecond = 2f;
        [SerializeField] private float pushCostPerSecond = 4f;

        [Header("Single Actions")]
        [SerializeField] private float attackSingleCost = 10f;
        [SerializeField] private float jumpSingleCost = 10f;

        [Header("Special Costs")]
        [SerializeField] private float blockHitCost = 5f; // Additional cost when hit during block

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