using Ship;
using UnityEngine;

namespace Ship {
    /// <summary>
    /// Компонент для обработки таранной атаки корабля (игрока, врагов, торговцев).
    /// Наносит урон и отбрасывание при столкновении AttackArea с HitArea другого корабля.
    /// </summary>
    /// <remarks>
    /// Привязка в Unity Inspector:
    /// - RamConfig: Настройки тарана (ShipRamConfig: BaseDamage, DamageBoost, DamageMultiplier, MinRamSpeed, BaseKnockbackForce, RamKnockbackMultiplier).
    /// - UpgradeConfig: Модификаторы тарана (ShipUpgradeConfig: RamDamageModifier, опционально).
    /// Настройка сцены:
    /// - Объект AttackArea должен иметь Collider2D (isTrigger=true, layer=Default, tag="AttackArea").
    /// - Целевой объект должен иметь ShipHitArea с Collider2D (isTrigger=true, tag="HitArea") и компонент ShipHealth.
    /// - ShipHealth должен реализовывать IShipHealth с методом TakeShipDamage(int, bool).
    /// Логика работы:
    /// - Awake: Проверяет наличие ShipRamConfig и Rigidbody2D.
    /// - CalculateDamage: Рассчитывает урон на основе скорости, конфигурации и модификаторов.
    /// - OnTriggerEnter2D: Обрабатывает столкновение с HitArea, наносит урон и отбрасывание.
    /// </remarks>
    [RequireComponent(typeof(Collider2D))]
    public class ShipRamAttack : MonoBehaviour {
        [SerializeField] private ShipRamConfig ramConfig; // Настройки тарана
        [SerializeField] private ShipUpgradeConfig upgradeConfig; // Модификаторы тарана (опционально)
        private Rigidbody2D rb; // Rigidbody2D для расчёта скорости

        // Инициализация при старте
        private void Awake()
        {
            if (ramConfig == null) // Проверяем наличие конфигурации
            {
                Debug.LogError($"ShipRamConfig не привязан для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"})!", this); // Логируем ошибку
                enabled = false; // Отключаем компонент
                return;
            }
            rb = GetComponentInParent<Rigidbody2D>(); // Получаем Rigidbody2D с родительского объекта
            if (rb == null) // Проверяем наличие Rigidbody2D
            {
                Debug.LogError($"Rigidbody2D не найден для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"})!", this); // Логируем ошибку
                enabled = false; // Отключаем компонент
                return;
            }
            Debug.Log($"ShipRamAttack инициализирован для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"})"); // Логируем инициализацию
        }

        // Рассчитывает урон от тарана
        public float CalculateDamage()
        {
            if (rb == null || ramConfig == null) // Проверяем наличие компонентов
            {
                Debug.LogWarning($"Rigidbody2D или ShipRamConfig отсутствует для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"}). Урон=0.", this); // Логируем предупреждение
                return 0f; // Возвращаем нулевой урон
            }
            float currentSpeed = rb.linearVelocity.magnitude; // Получаем текущую скорость
            float damage = ramConfig.BaseDamage * (currentSpeed >= ramConfig.MinRamSpeed ? ramConfig.DamageBoost : 1f) * ramConfig.DamageMultiplier * (upgradeConfig?.RamDamageModifier ?? 1f); // Рассчитываем урон
            Debug.Log($"Рассчитан урон тарана для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"}): Скорость={currentSpeed:F2}, Урон={damage:F2}"); // Логируем расчёт
            return damage;
        }

        // Обрабатывает столкновение с зоной попадания
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("HitArea")) // Проверяем, что столкнулись с HitArea
            {
                IShipHealth targetHealth = other.GetComponentInParent<IShipHealth>(); // Получаем IShipHealth с родительского объекта
                if (targetHealth == null) // Проверяем наличие ShipHealth
                {
                    Debug.LogWarning($"ShipHealth не найден на {other.gameObject.name} (ID={other.GetComponentInParent<ShipID>()?.ID ?? "Unknown"})!", this); // Логируем предупреждение
                    return;
                }

                float damage = CalculateDamage(); // Рассчитываем урон
                int finalDamage = Mathf.RoundToInt(damage); // Округляем урон
                targetHealth.TakeShipDamage(finalDamage, isRam: true); // Наносим урон с флагом тарана
                Debug.Log($"ShipRamAttack: {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"}) нанёс {finalDamage} урона {other.gameObject.name} (скорость={rb.linearVelocity.magnitude:F2})"); // Логируем нанесение урона

                IShipDamageable targetDamageable = other.GetComponentInParent<IShipDamageable>(); // Получаем IShipDamageable
                if (targetDamageable != null) // Проверяем наличие IShipDamageable
                {
                    Vector2 knockbackDir = (other.transform.position - transform.position).normalized; // Рассчитываем направление отбрасывания
                    float knockbackForce = ramConfig.BaseKnockbackForce * ramConfig.RamKnockbackMultiplier; // Рассчитываем силу отбрасывания
                    targetDamageable.ApplyShipKnockback(knockbackDir * knockbackForce); // Применяем отбрасывание
                    Debug.Log($"ShipRamAttack: Применено отбрасывание {knockbackForce:F2} к {other.gameObject.name} (ID={other.GetComponentInParent<ShipID>()?.ID ?? "Unknown"})"); // Логируем отбрасывание
                }
            }
        }
    }
}