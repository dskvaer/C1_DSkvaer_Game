namespace NPC.Characters.Player.Energy 
    {
    /// <summary>
    /// Интерфейс для условий регенерации (OCP)
    /// </summary>
    public interface IRegenerationCondition {
        string ConditionName { get; }
        bool CanRegenerate();
    }
}