namespace NPC.Characters.Player.Energy {
    /// <summary>
    /// Потребитель для одноразовых действий (атака, прыжок)
    /// </summary>
    public class SingleActionConsumer : EnergyConsumerBase {
        private readonly string actionName;
        private readonly float cost;

        public override string ActionName => actionName;

        public SingleActionConsumer(string actionName, float cost)
        {
            this.actionName = actionName;
            this.cost = cost;
        }

        public override float CalculateCost(float deltaTime)
        {
            // Для одноразовых действий deltaTime не используется
            return cost;
        }
    }
}