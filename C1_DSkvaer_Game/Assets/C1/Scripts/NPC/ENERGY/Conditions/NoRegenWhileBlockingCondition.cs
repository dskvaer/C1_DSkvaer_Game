namespace NPC.Characters.Player.Energy {
    /// <summary>
    /// ”словие: нельз€ регенерировать во врем€ блока
    /// </summary>
    public class NoRegenWhileBlockingCondition : IRegenerationCondition {
        private readonly System.Func<bool> isBlockingCheck;

        public string ConditionName => "No Regen While Blocking";

        public NoRegenWhileBlockingCondition(System.Func<bool> isBlockingCheck)
        {
            this.isBlockingCheck = isBlockingCheck ?? throw new System.ArgumentNullException(nameof(isBlockingCheck));
        }

        public bool CanRegenerate()
        {
            return !isBlockingCheck();
        }
    }
}