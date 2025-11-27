using UnityEngine.Events;

namespace NPC.Characters.Player {
    /// <summary>
    /// Интерфейс для обратной совместимости со старым кодом
    /// Используйте Energy.IEnergy для нового кода
    /// </summary>
    public interface IEnergy {
        int CurrentEnergy { get; }
        int MaxEnergy { get; }
        bool IsEnergyEmpty { get; }
        void ConsumeForSingleAction(float cost);
        void ConsumeForHeldAction(float costPerSecond, float deltaTime);
        void RestoreEnergy(float amount);
        UnityEvent OnEnergyChanged { get; }
        UnityEvent OnEnergyEmpty { get; }
    }
}