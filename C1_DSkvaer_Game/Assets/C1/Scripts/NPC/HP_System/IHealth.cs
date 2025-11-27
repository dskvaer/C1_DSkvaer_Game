using UnityEngine.Events;

public interface IHealth {
    int CurrentHealth { get; }
    int MaxHealth { get; }
    bool IsDead { get; }

    void TakeDamage(float hitXP);
    void Heal(float amount);

    UnityEvent OnHealthChanged { get; }
    UnityEvent OnDeath { get; }
}