using NPC.Characters.Player;
using UnityEngine;
using UnityEngine.Events;

#pragma warning disable 0414

namespace C1.Platformer.Characters {
    [DisallowMultipleComponent]
    public sealed class PlatformerCharacterVitals : MonoBehaviour, global::IHealth, IEnergy {
        [Header("Описание")]
        [SerializeField, TextArea(3, 7)] private string inspectorDescription =
            "Базовые ресурсы платформерного персонажа: здоровье, стамина, патроны и метательные предметы. Компонент реализует IHealth и IEnergy, поэтому его можно подключать к текущим UI-полоскам.";

        [Header("Здоровье")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth = 100;
        [SerializeField] private float damageThreshold;

        [Header("Стамина")]
        [SerializeField] private int maxEnergy = 100;
        [SerializeField] private int currentEnergy = 100;
        [SerializeField] private float energyRegenPerSecond = 12f;
        [SerializeField] private float regenDelayAfterSpend = 0.8f;

        [Header("Боезапас")]
        [SerializeField] private int ammo;
        [SerializeField] private int maxAmmo = 30;
        [SerializeField] private int throwables;
        [SerializeField] private int maxThrowables = 6;

        private float regenDelayTimer;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsDead => currentHealth <= 0;
        public int CurrentEnergy => currentEnergy;
        public int MaxEnergy => maxEnergy;
        public bool IsEnergyEmpty => currentEnergy <= 0;
        public int Ammo => ammo;
        public int MaxAmmo => maxAmmo;
        public int Throwables => throwables;
        public int MaxThrowables => maxThrowables;

        public UnityEvent OnHealthChanged { get; } = new UnityEvent();
        public UnityEvent OnDeath { get; } = new UnityEvent();
        public UnityEvent OnEnergyChanged { get; } = new UnityEvent();
        public UnityEvent OnEnergyEmpty { get; } = new UnityEvent();
        public UnityEvent OnAmmoChanged { get; } = new UnityEvent();

        private void Awake()
        {
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
            ammo = Mathf.Clamp(ammo, 0, maxAmmo);
            throwables = Mathf.Clamp(throwables, 0, maxThrowables);
        }

        private void Update()
        {
            if (regenDelayTimer > 0f)
            {
                regenDelayTimer -= Time.deltaTime;
                return;
            }

            if (energyRegenPerSecond > 0f && currentEnergy < maxEnergy)
            {
                RestoreEnergy(energyRegenPerSecond * Time.deltaTime);
            }
        }

        public void TakeDamage(float hitXP)
        {
            if (IsDead || hitXP <= damageThreshold)
            {
                return;
            }

            int oldHealth = currentHealth;
            currentHealth = Mathf.Max(0, currentHealth - Mathf.RoundToInt(hitXP));
            if (currentHealth != oldHealth)
            {
                OnHealthChanged.Invoke();
            }

            if (currentHealth <= 0)
            {
                OnDeath.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || IsDead)
            {
                return;
            }

            int oldHealth = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.RoundToInt(amount));
            if (currentHealth != oldHealth)
            {
                OnHealthChanged.Invoke();
            }
        }

        public void ConsumeForSingleAction(float cost)
        {
            SpendEnergy(cost);
        }

        public void ConsumeForHeldAction(float costPerSecond, float deltaTime)
        {
            SpendEnergy(costPerSecond * deltaTime);
        }

        public void RestoreEnergy(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            int oldEnergy = currentEnergy;
            currentEnergy = Mathf.Min(maxEnergy, currentEnergy + Mathf.RoundToInt(amount));
            if (currentEnergy != oldEnergy)
            {
                OnEnergyChanged.Invoke();
            }
        }

        public bool TrySpendEnergy(float amount)
        {
            if (amount <= 0f)
            {
                return true;
            }

            if (currentEnergy < amount)
            {
                return false;
            }

            SpendEnergy(amount);
            return true;
        }

        public bool TrySpendAmmo(int amount = 1)
        {
            if (amount <= 0)
            {
                return true;
            }

            if (ammo < amount)
            {
                return false;
            }

            ammo -= amount;
            OnAmmoChanged.Invoke();
            return true;
        }

        public bool TrySpendThrowable(int amount = 1)
        {
            if (amount <= 0)
            {
                return true;
            }

            if (throwables < amount)
            {
                return false;
            }

            throwables -= amount;
            OnAmmoChanged.Invoke();
            return true;
        }

        public void AddAmmo(int amount)
        {
            ammo = Mathf.Clamp(ammo + amount, 0, maxAmmo);
            OnAmmoChanged.Invoke();
        }

        public void AddThrowables(int amount)
        {
            throwables = Mathf.Clamp(throwables + amount, 0, maxThrowables);
            OnAmmoChanged.Invoke();
        }

        private void SpendEnergy(float amount)
        {
            int oldEnergy = currentEnergy;
            currentEnergy = Mathf.Max(0, currentEnergy - Mathf.RoundToInt(amount));
            regenDelayTimer = regenDelayAfterSpend;

            if (currentEnergy != oldEnergy)
            {
                OnEnergyChanged.Invoke();
            }

            if (currentEnergy <= 0)
            {
                OnEnergyEmpty.Invoke();
            }
        }
    }
}
