namespace NPC.Characters.Player.Energy {
    public interface IEnergy : IEnergyReadable, IEnergyModifiable, IEnergyEvents {
        void ConsumeForSingleAction(float cost);
        void ConsumeForHeldAction(float costPerSecond, float deltaTime);
    }
}