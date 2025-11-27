namespace NPC.Characters.Player.Energy 
    {
    /// <summary>
    /// Интерфейс для изменения энергии (ISP)
    /// </summary>
    public interface IEnergyModifiable {
        void ConsumeEnergy(float amount);
        void RestoreEnergy(float amount);
    }
}