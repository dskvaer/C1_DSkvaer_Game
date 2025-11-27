namespace NPC.Characters.Player.Energy 
    {
    /// <summary>
    /// »нтерфейс дл€ действий, потребл€ющих энергию (OCP)
    /// </summary>
    public interface IEnergyConsumer {
        string ActionName { get; }
        bool CanConsume(IEnergyReadable energy);
        float CalculateCost(float deltaTime);
        void OnConsumed();
        void Reset();
    }
}