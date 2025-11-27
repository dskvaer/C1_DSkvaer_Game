using UnityEngine;
using UnityEngine.Events;

namespace NPC.Characters.Player.Energy {
    /// <summary>
    /// Ядро системы энергии - отвечает только за хранение и изменение значений (SRP)
    /// </summary>
    public class EnergyCore : IEnergy {
        private readonly EnergyConfigSO config;
        private float currentEnergy;
        private bool wasLowEnergy;

        public int CurrentEnergy => Mathf.RoundToInt(currentEnergy);
        public int MaxEnergy => config?.MaxEnergy ?? 100;
        public bool IsEnergyEmpty => currentEnergy <= 0;
        public bool IsLowEnergy => currentEnergy <= (config?.LowEnergyThreshold ?? 20f);

        public UnityEvent OnEnergyChanged { get; } = new UnityEvent();
        public UnityEvent OnEnergyEmpty { get; } = new UnityEvent();
        public UnityEvent OnLowEnergy { get; } = new UnityEvent();

        public EnergyCore(EnergyConfigSO config)
        {
            this.config = config ?? throw new System.ArgumentNullException(nameof(config));
            currentEnergy = MaxEnergy;
        }

        public void Initialize()
        {
            currentEnergy = MaxEnergy;
            wasLowEnergy = false;
            OnEnergyChanged.Invoke();
        }

        public void ConsumeEnergy(float amount)
        {
            if (amount <= 0f) return;

            float oldEnergy = currentEnergy;
            currentEnergy = Mathf.Max(0f, currentEnergy - amount);

            // Проверка на пустую энергию
            if (currentEnergy <= 0f && oldEnergy > 0f)
            {
                OnEnergyEmpty.Invoke();
            }

            // Проверка на низкую энергию
            if (IsLowEnergy && !wasLowEnergy)
            {
                wasLowEnergy = true;
                OnLowEnergy.Invoke();
            }

            // Уведомление об изменении только при целочисленном изменении
            if (Mathf.RoundToInt(currentEnergy) != Mathf.RoundToInt(oldEnergy))
            {
                OnEnergyChanged.Invoke();
            }
        }

        public void RestoreEnergy(float amount)
        {
            if (amount <= 0f) return;

            float oldEnergy = currentEnergy;
            currentEnergy = Mathf.Min(MaxEnergy, currentEnergy + amount);

            // Сброс флага низкой энергии
            if (!IsLowEnergy && wasLowEnergy)
            {
                wasLowEnergy = false;
            }

            // Уведомление об изменении только при целочисленном изменении
            if (Mathf.RoundToInt(currentEnergy) != Mathf.RoundToInt(oldEnergy))
            {
                OnEnergyChanged.Invoke();
            }
        }

        // Методы для обратной совместимости
        public void ConsumeForSingleAction(float cost) => ConsumeEnergy(cost);

        public void ConsumeForHeldAction(float costPerSecond, float deltaTime)
        {
            if (costPerSecond > 0f && deltaTime > 0f)
                ConsumeEnergy(costPerSecond * deltaTime);
        }
    }
}