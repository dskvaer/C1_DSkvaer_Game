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
        [Header("Настройки здоровья")]
        [InspectorLabel("Конфиг здоровья")]
        [Tooltip("ScriptableObject с базовым запасом прочности корабля.")]
        [SerializeField] private ShipHealthConfig config;

        [InspectorLabel("Конфиг улучшений")]
        [Tooltip("Дополнительное здоровье и боевые модификаторы корабля. Можно оставить пустым, если улучшений нет.")]
        [SerializeField] private ShipUpgradeConfig upgradeConfig;

        [InspectorLabel("Конфиг тарана")]
        [Tooltip("Настройки ответного урона и модификаторов, которые используются при таране.")]
        [SerializeField] private ShipRamConfig ramConfig;

        [Header("Связанные компоненты")]
        [InspectorLabel("Компонент движения")]
        [Tooltip("Компонент, который реализует IShipMovable и IShipDamageable. Обычно это ShipMovement на этом же корабле.")]
        [SerializeField] private MonoBehaviour shipMovement;

        [InspectorLabel("Спрайт корабля")]
        [Tooltip("SpriteRenderer визуала корабля. Используется для будущих эффектов повреждения и затопления.")]
        [SerializeField] private SpriteRenderer shipSpriteRenderer;

        private Rigidbody2D rb;
        private int currentHealth;
        private int? maxHealthOverride;
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
                ;
            }

            if (shipSpriteRenderer == null) {
                ;
            }

            ResetHealth();
            ;
        }

        public void ResetHealth() {
            currentHealth = config != null ? GetMaxShipHealth() : 100;
            isDead = false;
            gameObject.SetActive(true);
            if (shipMovement != null) shipMovement.enabled = true;
            OnHealthChanged.Invoke();
            ;
        }

        public void TakeShipDamage(int amount) => TakeShipDamage(amount, false);

        public void TakeShipDamage(int amount, bool isRam) {
            if (isDead || amount <= 0) return;

            currentHealth = Mathf.Max(0, currentHealth - amount);

            if (isRam && ramConfig != null) {
                int selfDamage = Mathf.RoundToInt(amount * ramConfig.SelfDamagePercentage * (upgradeConfig?.RamSelfDamageReduction ?? 1f));
                currentHealth = Mathf.Max(0, currentHealth - selfDamage);
                ;
            }

            OnHealthChanged.Invoke();
            ;
            if (currentHealth <= 0) HandleShipDestruction();
        }

        public void SetShipHealth(int value) {
            if (isDead) return;
            currentHealth = Mathf.Clamp(value, 0, GetMaxShipHealth());
            OnHealthChanged.Invoke();
            if (currentHealth <= 0) HandleShipDestruction();
        }

        public int GetCurrentShipHealth() => currentHealth;
        public int GetMaxShipHealth() => maxHealthOverride ?? (config != null ? config.MaxHealth + (upgradeConfig?.AdditionalHealth ?? 0) : 100);

        public void SetMaxShipHealthOverride(int maxHealth, bool refillHealth = true) {
            maxHealthOverride = Mathf.Max(1, maxHealth);
            if (refillHealth) {
                currentHealth = maxHealthOverride.Value;
                isDead = false;
                OnHealthChanged.Invoke();
            } else {
                currentHealth = Mathf.Clamp(currentHealth, 0, maxHealthOverride.Value);
                OnHealthChanged.Invoke();
            }
        }

        public void ClearMaxShipHealthOverride(bool refillHealth = true) {
            maxHealthOverride = null;
            if (refillHealth) {
                ResetHealth();
            } else {
                currentHealth = Mathf.Clamp(currentHealth, 0, GetMaxShipHealth());
                OnHealthChanged.Invoke();
            }
        }

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
