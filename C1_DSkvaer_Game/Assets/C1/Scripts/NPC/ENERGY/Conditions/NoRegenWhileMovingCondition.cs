namespace NPC.Characters.Player.Energy {
    /// <summary>
    /// ”словие: нельз€ регенерировать во врем€ движени€
    /// </summary>
    public class NoRegenWhileMovingCondition : IRegenerationCondition {
        private readonly System.Func<bool> isMovingCheck;

        public string ConditionName => "No Regen While Moving";

        public NoRegenWhileMovingCondition(System.Func<bool> isMovingCheck)
        {
            this.isMovingCheck = isMovingCheck ?? throw new System.ArgumentNullException(nameof(isMovingCheck));
        }

        public bool CanRegenerate()
        {
            return !isMovingCheck();
        }
    }
}