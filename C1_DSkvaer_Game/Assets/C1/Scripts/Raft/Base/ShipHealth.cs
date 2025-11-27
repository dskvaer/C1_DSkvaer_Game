// ====================================================================================================
// ShipHealth.cs – УДАЛЁН ВИЗУАЛ ЖЕЛЕЗНОГО НОСА (перенесён в ShipVisualUpgrade)
// ====================================================================================================

using UnityEngine;
using UnityEngine.Events;

namespace Ship {
    /// <summary>
    /// Управление здоровьем корабля. Реализует IShipHealth, IShipDamageable, IHealth.
    /// </summary>
    /// <remarks>
    /// **ИЗМЕНЕНИЯ:**
    /// • Удалены: `ironNoseSprite`, проверка `RamDamageModifier > 1`, замена спрайта.
    /// • Визуальные улучшения (железный нос) → перенесены в **ShipVisualUpgrade.cs**.
    /// 
    /// **Inspector:**
    /// • `Config` → базовое здоровье
    /// • `UpgradeConfig` → доп. здоровье, модификаторы
    /// • `RamConfig` → урон от тарана
    /// • `ShipMovement` → IShipMovable + IShipDamageable
    /// • `ShipSpriteRenderer` → для эффекта затопления
    /// </remarks>
    [RequireComponent(typeof(Rigidbody2D))]
    public class ShipHealth : MonoBehaviour, IShipHealth, IShipDamageable, IHealth {
        [SerializeField] private ShipHealthConfig config;
        [SerializeField] private ShipUpgradeConfig upgradeConfig;
        [SerializeField] private ShipRamConfig ramConfig;
        [SerializeField] private MonoBehaviour shipMovement;
        [SerializeField] private SpriteRenderer shipSpriteRenderer; // для затопления

        private Rigidbody2D rb;
        private int currentHealth;
        private bool isDead;
        private IShipDamageable shipDamageable;
        private IShipMovable shipMovementInterface;

        public UnityEvent OnHealthChanged { get; } = new UnityEvent();
        public UnityEvent OnDeath { get; } = new UnityEvent();
        public GameObject GameObject => gameObject;

        private void Awake() {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null) {
                Debug.LogError($"Rigidbody2D не найден у {gameObject.name}!", this);
                enabled = false;
                return;
            }

            shipMovementInterface = shipMovement as IShipMovable;
            shipDamageable = shipMovement as IShipDamageable;
            if (shipMovement != null && (shipMovementInterface == null || shipDamageable == null)) {
                Debug.LogWarning($"ShipMovement не реализует нужные интерфейсы у {gameObject.name}.", this);
            }

            if (shipSpriteRenderer == null) {
                Debug.LogWarning($"SpriteRenderer не привязан (эффекты затопления отключены) у {gameObject.name}.", this);
            }

            ResetHealth();
            Debug.Log($"ShipHealth инициализирован: {gameObject.name}");
        }

        public void ResetHealth() {
            currentHealth = config != null ? GetMaxShipHealth() : 100;
            isDead = false;
            gameObject.SetActive(true);
            if (shipMovement != null) shipMovement.enabled = true;
            OnHealthChanged.Invoke();
            Debug.Log($"Здоровье сброшено: {currentHealth}/{GetMaxShipHealth()}");
        }

        public void TakeShipDamage(int amount) => TakeShipDamage(amount, false);

        public void TakeShipDamage(int amount, bool isRam) {
            if (isDead || amount <= 0) return;

            currentHealth = Mathf.Max(0, currentHealth - amount);

            if (isRam && ramConfig != null) {
                int selfDamage = Mathf.RoundToInt(amount * ramConfig.SelfDamagePercentage * (upgradeConfig?.RamSelfDamageReduction ?? 1f));
                currentHealth = Mathf.Max(0, currentHealth - selfDamage);
                Debug.Log($"Урон от тарана: {selfDamage}");
            }

            OnHealthChanged.Invoke();
            Debug.Log($"Урон: {amount}, Здоровье: {currentHealth}/{GetMaxShipHealth()}");

            if (currentHealth <= 0) HandleShipDestruction();
        }

        public void SetShipHealth(int value) {
            if (isDead) return;
            currentHealth = Mathf.Clamp(value, 0, GetMaxShipHealth());
            OnHealthChanged.Invoke();
            if (currentHealth <= 0) HandleShipDestruction();
        }

        public int GetCurrentShipHealth() => currentHealth;
        public int GetMaxShipHealth() => config != null ? config.MaxHealth + (upgradeConfig?.AdditionalHealth ?? 0) : 100;

        // IHealth
        public int CurrentHealth => GetCurrentShipHealth();
        public int MaxHealth => GetMaxShipHealth();
        public bool IsDead => isDead;

        public void ApplyShipKnockback(Vector2 force) {
            shipDamageable?.ApplyShipKnockback(force);
        }

        public void TakeDamage(float hitXP) => TakeShipDamage(Mathf.RoundToInt(hitXP));
        public void Heal(float amount) => SetShipHealth(currentHealth + Mathf.RoundToInt(amount));

        private void HandleShipDestruction() {
            if (isDead) return;
            isDead = true;
            if (shipMovement != null) shipMovement.enabled = false;
            if (rb != null) {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            if (shipSpriteRenderer != null) {
                LeanTween.value(gameObject, UpdateSinkingEffect, 1f, 0f, 2f)
                    .setOnComplete(() => {
                        OnDeath.Invoke();
                        gameObject.SetActive(false);
                    });
            } else {
                OnDeath.Invoke();
                gameObject.SetActive(false);
            }
        }

        private void UpdateSinkingEffect(float alpha) {
            if (shipSpriteRenderer != null) {
                shipSpriteRenderer.color = new Color(0f, 0.5f, 1f, alpha);
            }
        }
    }
}