using C1.Scripts.UI.Game_play_UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Ship {
    /// <summary>
    /// Спавнер NPC (врагов или торговцев). Создаёт объекты с заданным ID, полосами здоровья и управляет их удалением.
    /// Поддерживает интервальный и случайный спавн с возможностью ограничения по условиям.
    /// </summary>
    /// <remarks>
    /// Привязка в Unity Inspector:
    /// - EnemySpawnerConfig: Настройки спавна (SpawnInterval, MaxEnemies, MinSpawnCount, MaxSpawnCount, RestrictSpawnUntilCondition, UseRandomSpawn).
    /// - ShipIDConfig: Конфигурация ID для NPC (например, префикс "RE" для врагов, "RT" для торговцев).
    /// - EnemyPrefab: Префаб NPC (Enemy_Ship или Trader_Ship) с компонентами ShipHealth, ShipMovement, ShipID.
    /// - WaterTilemap: Tilemap воды для определения области спавна.
    /// - HealthBarCanvas: Canvas (Screen Space - Camera) для полос здоровья.
    /// - HealthBarPrefab: Префаб полосы здоровья с компонентами HealthBar и FollowTarget.
    /// Настройка сцены:
    /// - Убедитесь, что EnemyPrefab не содержит ShipPlayerInputHandler (только для игрока).
    /// - WaterTilemap должен быть настроен с корректными границами (localBounds).
    /// - HealthBarCanvas: Screen Space - Camera, привязка к Main Camera, Scale With Screen Size.
    /// - HealthBarPrefab: Должен иметь HealthBar (для отображения здоровья) и FollowTarget (для слежения за NPC).
    /// Логика работы:
    /// - Awake: Проверяет наличие компонентов и отключает спавнер, если они не привязаны.
    /// - Update: Управляет интервальным или случайным спавном в зависимости от EnemySpawnerConfig.
    /// - SpawnEnemy: Создаёт NPC и полосу здоровья, регистрирует их в словаре, подписывается на событие смерти.
    /// - RemoveEntity: Удаляет NPC и полосу здоровья при смерти.
    /// </remarks>
    public class EnemySpawner : MonoBehaviour {
        [SerializeField] private EnemySpawnerConfig config; // Настройки спавнера
        [SerializeField] private ShipIDConfig idConfig; // Конфигурация ID для NPC
        [SerializeField] private GameObject enemyPrefab; // Префаб NPC
        [SerializeField] private Tilemap waterTilemap; // Водный тайлмап для спавна
        [SerializeField] private Canvas healthBarCanvas; // Canvas для полос здоровья
        [SerializeField] private GameObject healthBarPrefab; // Префаб полосы здоровья
        private float spawnTimer; // Таймер для интервального спавна
        private readonly Dictionary<string, (GameObject enemy, GameObject healthBar, ShipHealth shipHealth)> spawnedEntities = new(); // Отслеживание NPC
        private bool isRandomSpawnTriggered; // Флаг случайного спавна
        private System.Func<bool> canSpawnCondition; // Условие для спавна

        // Инициализация при старте
        private void Awake()
        {
            if (config == null) // Проверяем наличие конфига
            {
                Debug.LogWarning($"EnemySpawnerConfig не привязан для {gameObject.name}. Спавнер отключен.", this); // Логируем предупреждение
                enabled = false; // Отключаем спавнер
                return;
            }
            if (idConfig == null) // Проверяем наличие ID конфига
            {
                Debug.LogWarning($"ShipIDConfig не привязан для {gameObject.name}. Спавнер отключен.", this); // Логируем предупреждение
                enabled = false; // Отключаем спавнер
                return;
            }
            if (enemyPrefab == null) // Проверяем наличие префаба
            {
                Debug.LogWarning($"EnemyPrefab не привязан для {gameObject.name}. Спавнер отключен.", this); // Логируем предупреждение
                enabled = false; // Отключаем спавнер
                return;
            }
            if (waterTilemap == null) // Проверяем наличие тайлмапа
            {
                Debug.LogWarning($"WaterTilemap не привязан для {gameObject.name}. Спавнер отключен.", this); // Логируем предупреждение
                enabled = false; // Отключаем спавнер
                return;
            }
            if (healthBarCanvas == null) // Проверяем наличие Canvas
            {
                Debug.LogWarning($"HealthBarCanvas не привязан для {gameObject.name}. Спавнер отключен.", this); // Логируем предупреждение
                enabled = false; // Отключаем спавнер
                return;
            }
            if (healthBarPrefab == null) // Проверяем наличие префаба полосы здоровья
            {
                Debug.LogWarning($"HealthBarPrefab не привязан для {gameObject.name}. Спавнер отключен.", this); // Логируем предупреждение
                enabled = false; // Отключаем спавнер
                return;
            }
            if (enemyPrefab.GetComponent<ShipPlayerInputHandler>()) // Проверяем отсутствие ShipPlayerInputHandler
            {
                Debug.LogWarning($"EnemyPrefab ошибочно ссылается на Player_Ship для {gameObject.name}. Спавнер отключен.", this); // Логируем предупреждение
                enabled = false; // Отключаем спавнер
                return;
            }

            spawnTimer = config.SpawnInterval; // Инициализируем таймер спавна
            Debug.Log($"Спавнер инициализирован для {gameObject.name}"); // Логируем инициализацию
        }

        // Обновление таймера и спавна
        private void Update()
        {
            if (config.RestrictSpawnUntilCondition && canSpawnCondition != null && !canSpawnCondition()) return; // Проверяем условие спавна

            if (config.UseRandomSpawn) // Проверяем режим случайного спавна
            {
                if (!isRandomSpawnTriggered && spawnedEntities.Count < config.MaxEnemies) // Проверяем возможность спавна
                {
                    TriggerRandomSpawn(); // Запускаем случайный спавн
                    isRandomSpawnTriggered = true; // Устанавливаем флаг
                }
            }
            else
            {
                spawnTimer -= Time.deltaTime; // Уменьшаем таймер
                if (spawnTimer <= 0 && spawnedEntities.Count < config.MaxEnemies) // Проверяем возможность спавна
                {
                    SpawnEnemy(); // Спавним NPC
                    spawnTimer = config.SpawnInterval; // Сбрасываем таймер
                }
            }
        }

        // Задаёт условие для спавна
        public void SetSpawnCondition(System.Func<bool> condition)
        {
            canSpawnCondition = condition; // Устанавливаем условие спавна
            Debug.Log($"Условие спавна установлено для {gameObject.name}"); // Логируем установку условия
        }

        // Выполняет случайный спавн
        private void TriggerRandomSpawn()
        {
            int spawnCount = Random.Range(config.MinSpawnCount, config.MaxSpawnCount + 1); // Рассчитываем количество NPC
            for (int i = 0; i < spawnCount && spawnedEntities.Count < config.MaxEnemies; i++) // Спавним NPC
            {
                SpawnEnemy(); // Спавним одного NPC
            }
            Debug.Log($"Случайный спавн: {spawnCount} NPC для {gameObject.name}"); // Логируем спавн
        }

        // Создаёт NPC и полосу здоровья
        private void SpawnEnemy()
        {
            Vector2 spawnPoint; // Точка спавна
            Transform player = GameObject.FindGameObjectWithTag("Player")?.transform; // Находим игрока
            do
            {
                Bounds bounds = waterTilemap.localBounds; // Получаем границы тайлмапа
                spawnPoint = new Vector2(
                    Random.Range(bounds.min.x, bounds.max.x), // Случайная X-координата
                    Random.Range(bounds.min.y, bounds.max.y) // Случайная Y-координата
                );
                spawnPoint = waterTilemap.transform.TransformPoint(spawnPoint); // Преобразуем в мировые координаты
            } while (player != null && Vector2.Distance(spawnPoint, player.position) < 5f); // Проверяем расстояние до игрока

            GameObject enemy = Instantiate(enemyPrefab, spawnPoint, Quaternion.identity); // Создаём NPC
            enemy.tag = idConfig.GetID().StartsWith("RE") ? "Enemy" : "Trader"; // Устанавливаем тег (Enemy или Trader)
            enemy.SetActive(true); // Активируем NPC

            ShipID shipID = enemy.GetComponent<ShipID>(); // Получаем ShipID
            if (shipID == null) shipID = enemy.AddComponent<ShipID>(); // Добавляем ShipID, если отсутствует
            shipID.ID = idConfig.GetID(); // Устанавливаем ID
            enemy.name = $"{enemy.tag}_Ship_{shipID.ID}"; // Задаём имя NPC

            ShipHealth shipHealth = enemy.GetComponent<ShipHealth>(); // Получаем ShipHealth
            if (shipHealth != null) // Проверяем наличие ShipHealth
            {
                shipHealth.ResetHealth(); // Сбрасываем здоровье NPC
                shipHealth.OnDeath.AddListener(() => RemoveEntity(shipID.ID)); // Подписываемся на событие смерти
                Debug.Log($"Здоровье инициализировано для {enemy.name}: {shipHealth.CurrentHealth}/{shipHealth.MaxHealth}"); // Логируем здоровье
            }

            GameObject healthBar = Instantiate(healthBarPrefab, healthBarCanvas.transform); // Создаём полосу здоровья
            healthBar.name = $"HealthBar_{shipID.ID}"; // Задаём имя полосы
            HealthBar healthBarScript = healthBar.GetComponent<HealthBar>(); // Получаем компонент HealthBar
            FollowTarget followTarget = healthBar.GetComponent<FollowTarget>(); // Получаем компонент FollowTarget
            if (healthBarScript != null && followTarget != null) // Проверяем наличие компонентов
            {
                healthBarScript.SetHealthComponent(shipHealth); // Привязываем здоровье
                followTarget.SetTarget(enemy.transform); // Устанавливаем цель для слежения
                Debug.Log($"Полоса здоровья создана: {healthBar.name} для {enemy.name} на позиции {healthBar.transform.position}"); // Логируем создание полосы
            }

            spawnedEntities[shipID.ID] = (enemy, healthBar, shipHealth); // Регистрируем NPC
            Debug.Log($"Создан NPC: {enemy.name} на позиции {spawnPoint}"); // Логируем спавн
        }

        // Удаляет NPC и полосу здоровья
        private void RemoveEntity(string id)
        {
            if (spawnedEntities.TryGetValue(id, out var entity)) // Проверяем наличие NPC
            {
                if (entity.shipHealth != null) // Проверяем наличие ShipHealth
                {
                    entity.shipHealth.OnDeath.RemoveListener(() => RemoveEntity(id)); // Отписываемся от события смерти
                }

                if (entity.enemy != null) Destroy(entity.enemy); // Уничтожаем NPC
                if (entity.healthBar != null) Destroy(entity.healthBar); // Уничтожаем полосу здоровья
                spawnedEntities.Remove(id); // Удаляем из словаря
                isRandomSpawnTriggered = false; // Сбрасываем флаг случайного спавна
                Debug.Log($"NPC удалён: {id} и его полоса здоровья для {gameObject.name}"); // Логируем удаление
            }
        }
    }
}