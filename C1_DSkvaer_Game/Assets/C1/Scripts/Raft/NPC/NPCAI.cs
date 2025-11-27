// ====================================================================================================
// NPCAI.cs – ИИ ДЛЯ NPC (враги, торговцы) – БЕЗ ShipID В ЛОГАХ
// ====================================================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Ship {
    /// <summary>
    /// Компонент ИИ для NPC (враги, торговцы).
    /// Управляет общими компонентами корабля и выполняет тактики.
    /// </summary>
    /// <remarks>
    /// Привязка в Unity Inspector:
    /// - DetectionArea: CircleCollider2D для обнаружения игрока.
    /// - DetectionRadius: Радиус обнаружения (например, 10).
    /// - WaterTilemap: Водный тайлмап для ограничения движения.
    /// - ShipRamConfig: Настройки тарана.
    /// - ShipWeaponSystem: Компонент управления оружием (опционально).
    /// - Tactics: Список тактик (RamAttackTactic, PatrolTactic, FleeTactic, FireTactic).
    /// 
    /// Требуется:
    /// - IShipMovable и IShipHealth на объекте.
    /// - DetectionArea: isTrigger=true, Layer=Default.
    /// - Тактики должны реализовывать IEnemyTactic.
    /// 
    /// Логика:
    /// - Awake: Проверяет компоненты, настраивает DetectionArea.
    /// - Update: Выбирает и выполняет первую подходящую тактику.
    /// - OnTriggerEnter2D/Exit2D: Обновляет контекст при обнаружении игрока.
    /// - MoveToSmooth: Плавное движение.
    /// - ClampToTilemapBounds: Ограничивает позицию в тайлмапе.
    /// 
    /// **ВАЖНО:** ShipID больше не используется в логах (удалён из всех Debug.Log).
    /// </remarks>
    [RequireComponent(typeof(IShipMovable), typeof(IShipHealth))]
    public class NPCAI : MonoBehaviour {
        // --------------------------------------------------------------------- Inspector
        [SerializeField, Tooltip("Коллайдер обнаружения (CircleCollider2D, isTrigger=true)")]
        private Collider2D detectionArea;

        [SerializeField, Tooltip("Радиус обнаружения игрока (в единицах)")]
        private float detectionRadius = 10f;

        [SerializeField, Tooltip("Водный тайлмап (для ClampToTilemapBounds)")]
        private Tilemap waterTilemap;

        [SerializeField, Tooltip("Конфиг тарана (для RamAttackTactic)")]
        private ShipRamConfig ramConfig;

        [SerializeField, Tooltip("Система оружия (опционально, для FireTactic)")]
        private ShipWeaponSystem weaponSystem;

        [SerializeField, Tooltip("Список тактик (должны реализовывать IEnemyTactic)")]
        private List<MonoBehaviour> tactics;

        // --------------------------------------------------------------------- Private
        private IShipMovable shipMovement;
        private IShipHealth shipHealth;
        private Rigidbody2D rb;
        private Transform player;
        private IShipHealth playerHealth;
        private EnemyAIContext context;
        private List<IEnemyTactic> validTactics;

        // --------------------------------------------------------------------- Unity: Awake – инициализация с проверками
        private void Awake()
        {
            // Проверка DetectionArea
            if (detectionArea == null)
            {
                Debug.LogError($"[NPCAI] DetectionArea не привязан для {gameObject.name}!", this);
                enabled = false;
                return;
            }

            // Проверка WaterTilemap
            if (waterTilemap == null)
            {
                Debug.LogError($"[NPCAI] WaterTilemap не привязан для {gameObject.name}!", this);
                enabled = false;
                return;
            }

            // Проверка ShipRamConfig
            if (ramConfig == null)
            {
                Debug.LogError($"[NPCAI] ShipRamConfig не привязан для {gameObject.name}!", this);
                enabled = false;
                return;
            }

            // Проверка ShipWeaponSystem (опционально)
            if (weaponSystem == null)
            {
                weaponSystem = GetComponent<ShipWeaponSystem>();
                if (weaponSystem == null)
                {
                    Debug.LogWarning($"[NPCAI] ShipWeaponSystem не найден для {gameObject.name}. Стрельба отключена.", this);
                }
            }

            // Получение интерфейсов
            shipMovement = GetComponent<IShipMovable>();
            if (shipMovement == null)
            {
                Debug.LogError($"[NPCAI] IShipMovable не найден для {gameObject.name}!", this);
                enabled = false;
                return;
            }

            shipHealth = GetComponent<IShipHealth>();
            if (shipHealth == null)
            {
                Debug.LogError($"[NPCAI] IShipHealth не найден для {gameObject.name}!", this);
                enabled = false;
                return;
            }

            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogError($"[NPCAI] Rigidbody2D не найден для {gameObject.name}!", this);
                enabled = false;
                return;
            }

            // Настройка CircleCollider2D
            CircleCollider2D detectionCollider = detectionArea.GetComponent<CircleCollider2D>();
            if (detectionCollider != null)
            {
                detectionCollider.isTrigger = true;
                detectionCollider.radius = detectionRadius;
                detectionArea.gameObject.layer = LayerMask.NameToLayer("Default");
                Debug.Log($"[NPCAI] DetectionArea настроен: radius={detectionCollider.radius}, isTrigger={detectionCollider.isTrigger}, layer={detectionArea.gameObject.layer}");
            }
            else
            {
                Debug.LogError($"[NPCAI] CircleCollider2D не найден в DetectionArea для {gameObject.name}!", this);
                enabled = false;
                return;
            }

            // Валидация тактик
            validTactics = new List<IEnemyTactic>();
            foreach (var tactic in tactics)
            {
                if (tactic is IEnemyTactic enemyTactic)
                {
                    validTactics.Add(enemyTactic);
                }
                else
                {
                    Debug.LogError($"[NPCAI] Тактика {tactic?.name ?? "null"} не реализует IEnemyTactic для {gameObject.name}!", tactic);
                }
            }

            // Инициализация контекста
            context = new EnemyAIContext
            {
                ShipMovement = shipMovement,
                ShipHealth = shipHealth,
                Rigidbody = rb,
                WaterTilemap = waterTilemap,
                RamConfig = ramConfig,
                WeaponSystem = weaponSystem
            };

            Debug.Log($"[NPCAI] Инициализирован для {gameObject.name}, Тактик={validTactics.Count}");
        }

        // --------------------------------------------------------------------- Unity: Update – выполнение тактики
        private void Update()
        {
            if (shipHealth == null || shipHealth.GetCurrentShipHealth() <= 0)
            {
                Debug.Log($"[NPCAI] {gameObject.name} мёртв, обновление пропущено");
                return;
            }

            context.Player = player;
            context.PlayerHealth = playerHealth;

            foreach (var tactic in validTactics)
            {
                if (tactic.CanExecute(context))
                {
                    tactic.Execute(context, Time.deltaTime);
                    Debug.Log($"[NPCAI] Тактика {tactic.GetType().Name} выполнена для {gameObject.name}");
                    break;
                }
            }
        }

        // --------------------------------------------------------------------- Обнаружение игрока
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                player = other.transform;
                playerHealth = other.GetComponent<IShipHealth>();
                if (playerHealth == null)
                {
                    Debug.LogWarning($"[NPCAI] IShipHealth не найден на игроке {other.gameObject.name} для {gameObject.name}!", this);
                }
                Debug.Log($"[NPCAI] Игрок обнаружен: {other.gameObject.name} на {other.transform.position}");
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                player = null;
                playerHealth = null;
                Debug.Log($"[NPCAI] Игрок потерян для {gameObject.name}");
            }
        }

        // --------------------------------------------------------------------- Плавное движение
        public void MoveToSmooth(EnemyAIContext context, Vector2 target, float speed, float smoothTurnSpeed = 0.3f)
        {
            if (context.Rigidbody == null)
            {
                Debug.LogError($"[NPCAI] Rigidbody2D отсутствует в контексте для {gameObject.name}!", this);
                return;
            }

            Vector2 direction = (target - context.Rigidbody.position).normalized;
            Vector2 localDirection = context.Rigidbody.transform.InverseTransformDirection(direction);
            float smoothRotation = Mathf.Lerp(0f, localDirection.x, smoothTurnSpeed);

            context.ShipMovement.ShipRotate(new Vector2(Mathf.Clamp(smoothRotation, -1f, 1f), 0f), speed);
            context.ShipMovement.ShipMove(new Vector2(0f, Mathf.Clamp(localDirection.y, -1f, 1f)), speed);

            Debug.Log($"[NPCAI] Движение к {target}, Speed={speed:F2}, LocalDir={localDirection}");
        }

        // --------------------------------------------------------------------- Ограничение в тайлмапе
        public Vector2 ClampToTilemapBounds(EnemyAIContext context, Vector2 position)
        {
            if (context.WaterTilemap == null)
            {
                Debug.LogError($"[NPCAI] WaterTilemap не найден для {gameObject.name}!", this);
                return position;
            }

            Bounds bounds = context.WaterTilemap.localBounds;
            Vector3 min = context.WaterTilemap.transform.TransformPoint(bounds.min);
            Vector3 max = context.WaterTilemap.transform.TransformPoint(bounds.max);

            Vector2 clamped = new Vector2(
                Mathf.Clamp(position.x, min.x, max.x),
                Mathf.Clamp(position.y, min.y, max.y)
            );

            Debug.Log($"[NPCAI] Позиция ограничена: {position} → {clamped}");
            return clamped;
        }
    }
}