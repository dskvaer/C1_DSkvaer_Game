using UnityEngine;

namespace NPC.Characters.Player.Energy 
    {
    /// <summary>
    /// Интерфейс для чтения состояния энергии (ISP)
    /// </summary>
    public interface IEnergyReadable {
        int CurrentEnergy { get; }
        int MaxEnergy { get; }
        bool IsEnergyEmpty { get; }
        bool IsLowEnergy { get; }
    }
}