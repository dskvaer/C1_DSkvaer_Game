using UnityEngine;

namespace NPC.Characters.Player.Energy {
    /// <summary>
    /// Потребитель для удерживаемых действий (блок, движение, толкание)
    /// </summary>
    public class HeldActionConsumer : EnergyConsumerBase {
        private readonly string actionName;
        private readonly float costPerSecond;

        public override string ActionName => actionName;

        public HeldActionConsumer(string actionName, float costPerSecond)
        {
            this.actionName = actionName;
            this.costPerSecond = costPerSecond;
        }

        public override float CalculateCost(float deltaTime)
        {
            energyAccumulator += costPerSecond * deltaTime;

            if (energyAccumulator >= 1f)
            {
                int cost = Mathf.FloorToInt(energyAccumulator);
                energyAccumulator -= cost;
                return cost;
            }

            return 0f;
        }
    }
}
