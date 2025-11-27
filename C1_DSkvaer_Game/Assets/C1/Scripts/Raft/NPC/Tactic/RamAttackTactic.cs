using Ship;
using UnityEngine;

namespace Ship {
    /// <summary>
    /// Тактика тарана для NPC (врагов).
    /// Заставляет NPC приближаться к игроку, выполнять таран для нанесения урона и отходить на заданное время.
    /// Работает в трёх состояниях: Approach (приближение), Ram (таран), Retreat (отход).
    /// </summary>
    /// <remarks>
    /// Привязка в Unity Inspector:
    /// - Привязать к объекту Enemy_Ship как дочерний компонент NPCAI.
    /// - RamAttackTacticConfig: Настройки тактики (AttackSpeed, RamDistance, ContactDistance, RetreatTime, MinRamSpeed, HealthThreshold, DamageCooldown, SmoothTurnSpeed).
    /// - Требуется компонент NPCAI на родительском объекте (Enemy_Ship).
    /// Настройка сцены:
    /// - Убедитесь, что объект Enemy_Ship имеет компоненты NPCAI, ShipHealth, ShipMovement, ShipRamAttack (на AttackArea), ShipHitArea (на HitArea).
    /// - ShipRamConfig должен быть привязан к NPCAI и ShipRamAttack для согласованности параметров тарана.
    /// - Требуется Rigidbody2D на Enemy_Ship и Collider2D (isTrigger=true, layer=Default, tag="AttackArea") на AttackArea.
    /// Логика работы:
    /// - CanExecute: Проверяет, обнаружен ли игрок (context.Player != null) и достаточно ли здоровья (выше HealthThreshold).
    /// - Execute: Переключает состояния (Approach, Ram, Retreat) на основе дистанции и скорости.
    /// - Approach: Движение к игроку с AttackSpeed, пока не достигнута RamDistance.
    /// - Ram: Ускорение к игроку, нанесение урона при ContactDistance и MinRamSpeed, затем переход в Retreat.
    /// - Retreat: Движение назад с уменьшенной скоростью (AttackSpeed * 0.6) в течение RetreatTime, затем возврат в Approach.
    /// - DealRamDamage: Рассчитывает урон на основе ShipRamConfig (BaseDamage, DamageBoost, DamageMultiplier) и текущей скорости, вызывает TakeShipDamage с isRam=true.
    /// - ApplyKnockbackToSelf: Применяет отбрасывание к NPC после тарана (использует ShipRamConfig.BaseKnockbackForce и RamKnockbackMultiplier).
    /// </remarks>
    public class RamAttackTactic : MonoBehaviour, IEnemyTactic {
        [SerializeField] private RamAttackTacticConfig config; // Настройки тактики
        private enum State { Approach, Ram, Retreat } // Состояния тактики
        private State currentState = State.Approach; // Текущее состояние
        private float retreatTimer; // Таймер отхода
        private float lastDamageTime; // Время последнего урона
        private NPCAI npcAI; // Компонент NPCAI

        // Инициализация при старте
        private void Awake()
        {
            if (config == null) // Проверяем наличие конфига
            {
                Debug.LogError($"RamAttackTacticConfig не привязан для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"})!", this); // Логируем ошибку
                enabled = false; // Отключаем компонент
                return;
            }
            npcAI = GetComponentInParent<NPCAI>(); // Получаем NPCAI
            if (npcAI == null) // Проверяем наличие NPCAI
            {
                Debug.LogError($"NPCAI не найден для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"})!", this); // Логируем ошибку
                enabled = false; // Отключаем компонент
                return;
            }
            Debug.Log($"RamAttackTactic инициализирован для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"})"); // Логируем инициализацию
        }

        // Проверка возможности выполнения тактики
        public bool CanExecute(EnemyAIContext context)
        {
            bool canExecute = context.Player != null && context.ShipHealth.GetCurrentShipHealth() > context.ShipHealth.GetMaxShipHealth() * config.HealthThreshold; // Проверяем игрока и здоровье
            Debug.Log($"RamAttackTactic CanExecute для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"}): {canExecute}, Player={(context.Player != null ? "Detected" : "Not Detected")}, Health={context.ShipHealth.GetCurrentShipHealth()}/{context.ShipHealth.GetMaxShipHealth()}"); // Логируем проверку
            return canExecute;
        }

        // Выполнение тактики
        public void Execute(EnemyAIContext context, float deltaTime)
        {
            switch (currentState) // Переключаем состояния
            {
                case State.Approach:
                    Approach(context); // Приближение
                    break;
                case State.Ram:
                    Ram(context); // Таран
                    break;
                case State.Retreat:
                    Retreat(context, deltaTime); // Отход
                    break;
            }
        }

        // Состояние: Приближение
        private void Approach(EnemyAIContext context)
        {
            if (context.Player == null) // Проверяем наличие игрока
            {
                Debug.LogWarning($"Игрок не обнаружен для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"}) в состоянии Approach!", this); // Логируем предупреждение
                return;
            }
            float distanceToPlayer = Vector2.Distance(context.Rigidbody.position, (Vector2)context.Player.position); // Рассчитываем расстояние до игрока
            if (distanceToPlayer <= config.RamDistance) // Проверяем дистанцию для тарана
            {
                currentState = State.Ram; // Переключаемся на таран
                Debug.Log($"RamAttackTactic: {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"}) начинает таран на расстоянии {distanceToPlayer:F2}"); // Логируем переход
                return;
            }
            Vector2 targetPos = (Vector2)context.Player.position; // Получаем позицию игрока
            targetPos = npcAI.ClampToTilemapBounds(context, targetPos); // Ограничиваем позицию
            npcAI.MoveToSmooth(context, targetPos, config.AttackSpeed, config.SmoothTurnSpeed); // Двигаемся к цели
        }

        // Состояние: Таран
        private void Ram(EnemyAIContext context)
        {
            if (context.Player == null) // Проверяем наличие игрока
            {
                Debug.LogWarning($"Игрок не обнаружен для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"}) в состоянии Ram!", this); // Логируем предупреждение
                currentState = State.Retreat; // Переключаемся на отход
                retreatTimer = config.RetreatTime; // Устанавливаем таймер
                return;
            }
            float distanceToPlayer = Vector2.Distance(context.Rigidbody.position, (Vector2)context.Player.position); // Рассчитываем расстояние до игрока
            float currentSpeed = context.Rigidbody.linearVelocity.magnitude; // Получаем текущую скорость

            if (distanceToPlayer <= config.ContactDistance && currentSpeed >= config.MinRamSpeed) // Проверяем условия для урона
            {
                if (Time.time - lastDamageTime >= config.DamageCooldown) // Проверяем кулдаун урона
                {
                    DealRamDamage(context); // Наносим урон
                    ApplyKnockbackToSelf(context); // Применяем отбрасывание
                    lastDamageTime = Time.time; // Обновляем время урона
                    currentState = State.Retreat; // Переключаемся на отход
                    retreatTimer = config.RetreatTime; // Устанавливаем таймер
                    Debug.Log($"RamAttackTactic: {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"}) нанёс урон и отступает, speed={currentSpeed:F2}"); // Логируем таран
                    return;
                }
            }

            Vector2 targetPos = (Vector2)context.Player.position; // Получаем позицию игрока
            targetPos = npcAI.ClampToTilemapBounds(context, targetPos); // Ограничиваем позицию
            Vector2 worldDir = (targetPos - context.Rigidbody.position).normalized; // Рассчитываем направление
            Vector2 localDir = context.Rigidbody.transform.InverseTransformDirection(worldDir); // Преобразуем в локальные координаты
            float smoothRotation = Mathf.Lerp(0f, localDir.x, config.SmoothTurnSpeed); // Плавный поворот
            context.ShipMovement.ShipRotate(new Vector2(Mathf.Clamp(smoothRotation, -1f, 1f), 0f), config.AttackSpeed); // Поворачиваем корабль
            context.ShipMovement.ShipMove(new Vector2(0f, 1f), config.AttackSpeed); // Двигаем корабль вперёд
        }

        // Состояние: Отход
        private void Retreat(EnemyAIContext context, float deltaTime)
        {
            retreatTimer -= deltaTime; // Уменьшаем таймер отхода
            if (retreatTimer <= 0) // Проверяем завершение отхода
            {
                currentState = State.Approach; // Переключаемся на приближение
                Debug.Log($"RamAttackTactic: {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"}) завершил отход, возвращается к Approach"); // Логируем переход
                return;
            }

            context.ShipMovement.ShipMove(new Vector2(0f, -1f), config.AttackSpeed * 0.6f); // Двигаемся назад
            if (context.Player != null) // Проверяем наличие игрока
            {
                Vector2 directionToPlayer = ((Vector2)context.Player.position - context.Rigidbody.position).normalized; // Направление к игроку
                Vector2 localDir = context.Rigidbody.transform.InverseTransformDirection(directionToPlayer); // Преобразуем в локальные координаты
                float smoothRotation = Mathf.Lerp(0f, localDir.x, config.SmoothTurnSpeed); // Плавный поворот
                context.ShipMovement.ShipRotate(new Vector2(Mathf.Clamp(smoothRotation, -1f, 1f), 0f), config.AttackSpeed); // Поворачиваем корабль
            }
        }

        // Нанесение урона тараном
        private void DealRamDamage(EnemyAIContext context)
        {
            if (context.PlayerHealth == null || context.RamConfig == null) // Проверяем наличие здоровья игрока и конфига
            {
                Debug.LogWarning($"PlayerHealth или RamConfig отсутствует для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"})!", this); // Логируем предупреждение
                return;
            }
            float currentSpeed = context.Rigidbody.linearVelocity.magnitude; // Получаем текущую скорость
            int damage = Mathf.RoundToInt(context.RamConfig.BaseDamage * (currentSpeed >= context.RamConfig.MinRamSpeed ? context.RamConfig.DamageBoost : 1f) * context.RamConfig.DamageMultiplier); // Рассчитываем урон
            context.PlayerHealth.TakeShipDamage(damage, isRam: true); // Наносим урон игроку
            Debug.Log($"RamAttackTactic: {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"}) нанёс {damage} урона игроку (speed={currentSpeed:F2})"); // Логируем урон
        }

        // Применение отбрасывания к себе
        private void ApplyKnockbackToSelf(EnemyAIContext context)
        {
            if (context.Rigidbody == null || context.Player == null) // Проверяем наличие Rigidbody и игрока
            {
                Debug.LogWarning($"Rigidbody или Player отсутствует для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"})!", this); // Логируем предупреждение
                return;
            }
            Vector2 knockbackDir = (context.Rigidbody.position - (Vector2)context.Player.position).normalized; // Направление отбрасывания
            float knockbackForce = context.RamConfig.BaseKnockbackForce * context.RamConfig.RamKnockbackMultiplier * 0.5f; // Рассчитываем силу
            IShipDamageable damageable = context.Rigidbody.GetComponent<IShipDamageable>(); // Получаем IShipDamageable
            if (damageable != null) // Проверяем наличие IShipDamageable
            {
                damageable.ApplyShipKnockback(knockbackDir * knockbackForce); // Применяем отбрасывание
                Debug.Log($"RamAttackTactic: {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"}) применил отбрасывание к себе: {knockbackForce:F2}"); // Логируем отбрасывание
            }
        }
    }
}