using Ship;
using UnityEngine;

namespace Ship {
    /// <summary>
    /// Компонент для обработки урона в зоне попадания (HitArea).
    /// Применяет множитель урона из ShipRamConfig.
    /// </summary>
    /// <remarks>
    /// Привязка в Unity Inspector:
    /// - HealthComponent: Компонент ShipHealth (IShipHealth).
    /// - RamConfig: Настройки тарана для множителя урона (ShipRamConfig, опционально).
    /// Настройка сцены:
    /// - Объект HitArea (дочерний для корабля) должен иметь Collider2D (isTrigger=true, Layer=Default, Tag=HitArea).
    /// - HealthComponent должен ссылаться на ShipHealth на родительском объекте.
    /// Логика работы:
    /// - Awake: Инициализирует IShipHealth и Collider2D.
    /// - OnTriggerEnter2D: Обрабатывает столкновение с AttackArea, применяет урон.
    /// - DamageMultiplier: Возвращает множитель урона из ShipRamConfig.
    /// </remarks>
    [RequireComponent(typeof(Collider2D))]
    public class ShipHitArea : MonoBehaviour {
        [SerializeField] private MonoBehaviour healthComponent; // Компонент здоровья
        [SerializeField] private ShipRamConfig ramConfig; // Настройки тарана
        private IShipHealth shipHealth; // Интерфейс здоровья

        // Возвращает множитель урона
        public float DamageMultiplier => ramConfig != null ? ramConfig.DamageMultiplier : 1.0f; // Множитель из конфига или 1.0

        // Инициализация при старте
        private void Awake()
        {
            shipHealth = healthComponent as IShipHealth; // Приводим к IShipHealth
            if (shipHealth == null && healthComponent != null) // Проверяем возможность получения
            {
                shipHealth = healthComponent.GetComponent<IShipHealth>(); // Пытаемся получить IShipHealth
            }
            if (shipHealth == null) // Проверяем успешность получения
            {
                Debug.LogError($"IShipHealth не привязан для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"})!", this); // Логируем ошибку
                enabled = false; // Отключаем компонент
                return;
            }

            Collider2D collider = GetComponent<Collider2D>(); // Получаем Collider2D
            collider.isTrigger = true; // Устанавливаем триггер
            collider.gameObject.layer = LayerMask.NameToLayer("Default"); // Устанавливаем слой
            Debug.Log($"ShipHitArea инициализирован для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"}), Множитель урона={DamageMultiplier:F2}"); // Логируем инициализацию
        }

        // Обрабатывает столкновение
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("AttackArea")) // Проверяем столкновение с AttackArea
            {
                if (ramConfig == null) // Проверяем наличие конфигурации
                {
                    Debug.LogWarning($"ShipRamConfig не привязан для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"}). Множитель урона=1.0.", this); // Логируем предупреждение
                }

                ShipRamAttack attackerCollision = other.GetComponentInParent<ShipRamAttack>(); // Получаем ShipRamAttack
                if (attackerCollision != null && shipHealth != null) // Проверяем наличие компонентов
                {
                    float damage = attackerCollision.CalculateDamage(); // Рассчитываем базовый урон
                    float finalDamage = damage * DamageMultiplier; // Применяем множитель
                    shipHealth.TakeShipDamage(Mathf.RoundToInt(finalDamage), isRam: true); // Наносим урон
                    Debug.Log($"ShipHitArea: {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"}) получил урон: Базовый={damage:F2}, Итоговый={finalDamage:F2}, Здоровье={shipHealth.GetCurrentShipHealth()}"); // Логируем урон
                }
            }
        }
    }
}