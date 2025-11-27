using UnityEngine;

namespace NPC.Characters.Player.Energy {
    /// <summary>
    /// Базовый класс для потребителей энергии (OCP)
    /// </summary>
    public abstract class EnergyConsumerBase : IEnergyConsumer {
        protected float energyAccumulator;

        public abstract string ActionName { get; }

        public virtual bool CanConsume(IEnergyReadable energy)
        {
            return !energy.IsEnergyEmpty;
        }

        public abstract float CalculateCost(float deltaTime);

        public virtual void OnConsumed()
        {
            // Override in derived classes if needed
        }

        public virtual void Reset()
        {
            energyAccumulator = 0f;
        }
    }
}
