using UnityEngine;
using System.Collections.Generic;

namespace NPC.Characters.Player.Energy {
    /// <summary>
    /// Система регенерации энергии (SRP)
    /// </summary>
    public class EnergyRegenerator {
        private readonly EnergyConfigSO config;
        private readonly List<IRegenerationCondition> conditions = new List<IRegenerationCondition>();
        private float regenAccumulator;

        public EnergyRegenerator(EnergyConfigSO config)
        {
            this.config = config ?? throw new System.ArgumentNullException(nameof(config));
        }

        public void AddCondition(IRegenerationCondition condition)
        {
            if (condition != null && !conditions.Contains(condition))
            {
                conditions.Add(condition);
                Debug.Log($"EnergyRegenerator: Added condition '{condition.ConditionName}'");
            }
        }

        public void RemoveCondition(IRegenerationCondition condition)
        {
            if (conditions.Remove(condition))
            {
                Debug.Log($"EnergyRegenerator: Removed condition '{condition.ConditionName}'");
            }
        }

        public void ClearConditions()
        {
            conditions.Clear();
        }

        public void Update(IEnergyModifiable energy, IEnergyReadable energyState, float deltaTime)
        {
            // Нет регенерации если уже на максимуме
            if (config.RegenRate <= 0f || energyState.CurrentEnergy >= energyState.MaxEnergy)
            {
                regenAccumulator = 0f;
                return;
            }

            // Проверяем все условия
            foreach (var condition in conditions)
            {
                if (!condition.CanRegenerate())
                {
                    regenAccumulator = 0f;
                    return;
                }
            }

            // Накапливаем регенерацию
            regenAccumulator += config.RegenRate * deltaTime;

            if (regenAccumulator >= 1f)
            {
                int regenAmount = Mathf.FloorToInt(regenAccumulator);
                energy.RestoreEnergy(regenAmount);
                regenAccumulator -= regenAmount;
            }
        }

        public void Reset()
        {
            regenAccumulator = 0f;
        }
    }
}