namespace NPC.Characters.Player.Energy {
    /// <summary>
    /// Условие: нельзя регенерировать в бою (пример расширения)
    /// </summary>
    public class NoRegenInCombatCondition : IRegenerationCondition {
        private readonly System.Func<bool> isInCombatCheck;

        public string ConditionName => "No Regen In Combat";

        public NoRegenInCombatCondition(System.Func<bool> isInCombatCheck)
        {
            this.isInCombatCheck = isInCombatCheck ?? throw new System.ArgumentNullException(nameof(isInCombatCheck));
        }

        public bool CanRegenerate()
        {
            return !isInCombatCheck();
        }
    }
}