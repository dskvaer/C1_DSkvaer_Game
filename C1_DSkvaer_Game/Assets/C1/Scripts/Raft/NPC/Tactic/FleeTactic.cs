using UnityEngine;
using UnityEngine.Tilemaps;

namespace Ship {
    /// <summary>
    /// Тактика побега для NPC.
    /// Заставляет NPC отходить от игрока при низком здоровье.
    /// </summary>
    /// <remarks>
    /// Привязка в Unity Inspector:
    /// - Привязать к объекту Enemy_Ship как дочерний компонент NPCAI.
    /// - FleeTacticConfig: Настройки побега (FleeSpeed, FleeDistance, HealthThreshold, SmoothTurnSpeed).
    /// - Требуется NPCAI на родительском объекте.
    /// Настройка сцены:
    /// - Убедитесь, что объект Enemy_Ship имеет компоненты NPCAI, ShipHealth, ShipMovement, ShipHitArea (на HitArea).
    /// - Требуется Rigidbody2D на Enemy_Ship и Tilemap для WaterTilemap в NPCAI.
    /// - FleeTacticConfig должен быть создан через Assets > Create > ShipConfigs > TacticConfigs > FleeTacticConfig.
    /// Логика работы:
    /// - CanExecute: Проверяет, что игрок обнаружен (context.Player != null) и здоровье ниже HealthThreshold.
    /// - Execute: Двигается от игрока на FleeDistance с FleeSpeed, корректируя направление с SmoothTurnSpeed.
    /// - GetRandomPatrolPoint: Выбирает случайную точку в пределах WaterTilemap, если игрок не обнаружен.
    /// </remarks>
    public class FleeTactic : MonoBehaviour, IEnemyTactic {
        [SerializeField] private FleeTacticConfig config; // Настройки побега
        private Vector2 targetPosition; // Целевая позиция побега
        private NPCAI npcAI; // Компонент NPCAI

        // Инициализация при старте
        private void Awake()
        {
            if (config == null) // Проверяем наличие конфига
            {
                Debug.LogError($"FleeTacticConfig не привязан для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"})!", this); // Логируем ошибку
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
            Debug.Log($"FleeTactic инициализирован для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"})"); // Логируем инициализацию
        }

        // Проверка возможности выполнения тактики
        public bool CanExecute(EnemyAIContext context)
        {
            bool canExecute = context.Player != null && context.ShipHealth.GetCurrentShipHealth() <= context.ShipHealth.GetMaxShipHealth() * config.HealthThreshold; // Проверяем игрока и здоровье
            Debug.Log($"FleeTactic CanExecute для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"}): {canExecute}, Player={(context.Player != null ? "Detected" : "Not Detected")}, Health={context.ShipHealth.GetCurrentShipHealth()}/{context.ShipHealth.GetMaxShipHealth()}"); // Логируем проверку
            return canExecute;
        }

        // Выполнение тактики
        public void Execute(EnemyAIContext context, float deltaTime)
        {
            if (context.Player == null) // Проверяем наличие игрока
            {
                targetPosition = GetRandomPatrolPoint(context); // Получаем случайную точку
                npcAI.MoveToSmooth(context, targetPosition, config.FleeSpeed, config.SmoothTurnSpeed); // Двигаемся к точке
                return;
            }

            Vector2 fleeDirection = (context.Rigidbody.position - (Vector2)context.Player.position).normalized; // Направление побега
            targetPosition = context.Rigidbody.position + fleeDirection * config.FleeDistance; // Рассчитываем целевую позицию
            targetPosition = npcAI.ClampToTilemapBounds(context, targetPosition); // Ограничиваем позицию
            npcAI.MoveToSmooth(context, targetPosition, config.FleeSpeed, config.SmoothTurnSpeed); // Двигаемся к цели
            Debug.Log($"FleeTactic: {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"}) убегает к {targetPosition}"); // Логируем побег
        }

        // Получение случайной точки патрулирования
        private Vector2 GetRandomPatrolPoint(EnemyAIContext context)
        {
            if (context.WaterTilemap == null) // Проверяем наличие тайлмапа
            {
                Debug.LogError($"WaterTilemap не найден для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"})!", this); // Логируем ошибку
                return context.Rigidbody.position; // Возвращаем текущую позицию
            }
            Bounds bounds = context.WaterTilemap.localBounds; // Получаем границы тайлмапа
            Vector2 randomPoint = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x), // Случайная X-координата
                Random.Range(bounds.min.y, bounds.max.y) // Случайная Y-координата
            );
            return context.WaterTilemap.transform.TransformPoint(randomPoint); // Преобразуем в мировые координаты
        }
    }
}