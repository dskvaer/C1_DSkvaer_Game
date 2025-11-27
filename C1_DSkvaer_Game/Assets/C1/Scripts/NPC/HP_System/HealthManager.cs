using UnityEngine;
using UnityEngine.Events;
using NPC.Characters.Player;

public class HealthManager : MonoBehaviour, IHealth {
    [SerializeField] private HealthConfigSO config;
    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => config != null ? config.MaxHealth : 100;
    public bool IsDead => currentHealth <= 0;

    public UnityEvent OnHealthChanged { get; } = new UnityEvent();
    public UnityEvent OnDeath { get; } = new UnityEvent();

    private void Awake()
    {
        if (config == null)
        {
            Debug.LogError("HealthManager: HealthConfigSO not assigned!", this);
            enabled = false;
            return;
        }

        currentHealth = MaxHealth;
        Debug.Log("HealthManager: Initialized with maxHealth = " + MaxHealth);
    }

    private void Start()
    {
        // Вызываем событие после инициализации всех компонентов
        OnHealthChanged.Invoke();
    }

    private void Update()
    {
        // Auto-regen if regenRate > 0
        if (config.RegenRate > 0f && currentHealth < MaxHealth)
        {
            Heal(config.RegenRate * Time.deltaTime);
        }
    }

    public void TakeDamage(float hitXP)
    {
        if (hitXP <= config.DamageThreshold)
        {
            Debug.Log("HealthManager: Damage below threshold, ignored");
            return;
        }

        currentHealth -= Mathf.RoundToInt(hitXP);
        if (currentHealth < 0)
        {
            currentHealth = 0;
            OnDeath.Invoke();
            Debug.Log("HealthManager: Character died!");
        }

        OnHealthChanged.Invoke();
        Debug.Log("HealthManager: Damage taken, currentHealth = " + currentHealth);
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;

        int oldHealth = currentHealth;
        currentHealth += Mathf.RoundToInt(amount);

        if (currentHealth > MaxHealth)
        {
            currentHealth = MaxHealth;
        }

        // Вызываем событие только если здоровье действительно изменилось
        if (currentHealth != oldHealth)
        {
            OnHealthChanged.Invoke();
            Debug.Log("HealthManager: Health restored, currentHealth = " + currentHealth);
        }
    }
}