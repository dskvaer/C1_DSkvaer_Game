using UnityEngine;
using UnityEngine.Events;
using NPC.Characters.Player;

public class HealthManager : MonoBehaviour, IHealth {
    [Header("Настройки здоровья")]
    [InspectorLabel("Конфиг здоровья")]
    [Tooltip("ScriptableObject с максимальным здоровьем, регенерацией и порогом урона.")]
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
            Debug.LogError("HealthManager: HealthConfigSO не назначен!", this);
            enabled = false;
            return;
        }

        currentHealth = MaxHealth;
    }

    private void Start()
    {
        OnHealthChanged.Invoke();
    }

    private void Update()
    {
        if (config.RegenRate > 0f && currentHealth < MaxHealth)
        {
            Heal(config.RegenRate * Time.deltaTime);
        }
    }

    public void TakeDamage(float hitXP)
    {
        if (hitXP <= config.DamageThreshold)
        {
            return;
        }

        currentHealth -= Mathf.RoundToInt(hitXP);
        if (currentHealth < 0)
        {
            currentHealth = 0;
            OnDeath.Invoke();
        }

        OnHealthChanged.Invoke();
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

        if (currentHealth != oldHealth)
        {
            OnHealthChanged.Invoke();
        }
    }
}
