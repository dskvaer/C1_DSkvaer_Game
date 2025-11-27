using Ship;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Ship {
    /// <summary>
    /// Тактика патрулирования для NPC.
    /// Двигает NPC к случайным точкам в пределах водного тайлмапа.
    /// </summary>
    /// <remarks>
    /// Привязка в Unity Inspector:
    /// - Привязать к объекту Enemy_Ship как дочерний компонент NPCAI.
    /// - PatrolTacticConfig: Настройки патрулирования (PatrolSpeed, MinPatrolTime, MaxPatrolTime, SmoothTurnSpeed).
    /// - Требуется NPCAI на родительском объекте.
    /// Настройка сцены:
    /// - Убедитесь, что объект Enemy_Ship имеет компоненты NPCAI, ShipHealth, ShipMovement, ShipHitArea (на HitArea).
    /// - Требуется Rigidbody2D на Enemy_Ship и Tilemap для WaterTilemap в NPCAI.
    /// Логика работы:
    /// - CanExecute: Проверяет, что игрок не обнаружен (context.Player == null) или здоровье выше порога побега.
    /// - Execute: Двигается к случайной точке в пределах WaterTilemap с PatrolSpeed, меняет точку через случайный интервал (MinPatrolTime–MaxPatrolTime).
    /// </remarks>
    public class PatrolTactic : MonoBehaviour, IEnemyTactic {
        [SerializeField] private PatrolTacticConfig config; // Настройки патрулирования
        private Vector2 targetPosition; // Целевая позиция патруля
        private float patrolTimer; // Таймер патрулирования
        private NPCAI npcAI; // Компонент NPCAI
        private bool isInitialized; // Флаг инициализации

        // Инициализация при старте
        private void Awake()
        {
            if (config == null) // Проверяем наличие конфига
            {
                Debug.LogError($"PatrolTacticConfig не привязан для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"})!", this); // Логируем ошибку
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
            Debug.Log($"PatrolTactic инициализирован для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"})"); // Логируем инициализацию
        }

        // Проверка возможности выполнения тактики
        public bool CanExecute(EnemyAIContext context)
        {
            bool canExecute = context.Player == null || context.ShipHealth.GetCurrentShipHealth() > context.ShipHealth.GetMaxShipHealth() * 0.2f; // Проверяем игрока и здоровье
            Debug.Log($"PatrolTactic CanExecute для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"}): {canExecute}, Player={(context.Player != null ? "Detected" : "Not Detected")}, Health={context.ShipHealth.GetCurrentShipHealth()}/{context.ShipHealth.GetMaxShipHealth()}"); // Логируем проверку
            return canExecute;
        }

        // Выполнение тактики
        public void Execute(EnemyAIContext context, float deltaTime)
        {
            patrolTimer -= deltaTime; // Уменьшаем таймер
            if (!isInitialized || patrolTimer <= 0 || Vector2.Distance(context.Rigidbody.position, targetPosition) < 0.5f) // Проверяем необходимость новой точки
            {
                SetPatrolTarget(context); // Устанавливаем новую точку
                isInitialized = true; // Устанавливаем флаг инициализации
            }
            npcAI.MoveToSmooth(context, targetPosition, config.PatrolSpeed, config.SmoothTurnSpeed); // Двигаемся к цели
        }

        // Установка случайной точки патрулирования
        private void SetPatrolTarget(EnemyAIContext context)
        {
            if (context.WaterTilemap == null) // Проверяем наличие тайлмапа
            {
                Debug.LogError($"WaterTilemap не найден для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"})!", this); // Логируем ошибку
                return;
            }
            Bounds bounds = context.WaterTilemap.localBounds; // Получаем границы тайлмапа
            Vector2 randomPoint = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x), // Случайная X-координата
                Random.Range(bounds.min.y, bounds.max.y) // Случайная Y-координата
            );
            targetPosition = context.WaterTilemap.transform.TransformPoint(randomPoint); // Преобразуем в мировые координаты
            patrolTimer = Random.Range(config.MinPatrolTime, config.MaxPatrolTime); // Устанавливаем таймер
            Debug.Log($"PatrolTactic: Новая точка патрулирования для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"}) на {targetPosition}"); // Логируем новую точку
        }
    }
}