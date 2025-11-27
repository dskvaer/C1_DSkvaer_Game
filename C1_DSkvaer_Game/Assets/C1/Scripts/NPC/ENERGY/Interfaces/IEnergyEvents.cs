using UnityEngine.Events;

namespace NPC.Characters.Player.Energy 
    {
    /// <summary>
    /// Интерфейс для событий энергии (ISP)
    /// </summary>
    public interface IEnergyEvents {
        UnityEvent OnEnergyChanged { get; }
        UnityEvent OnEnergyEmpty { get; }
        UnityEvent OnLowEnergy { get; }
    }
}