using UnityEngine;

namespace Ship {
    /// <summary>
    /// Настройки спавнера NPC. Хранит параметры для управления количеством, интервалом и режимом спавна.
    /// </summary>
    /// <remarks>
    /// Привязка в Unity Inspector:
    /// - Создать через Assets > Create > ShipConfigs > SpawnerConfigs > EnemySpawnerConfig.
    /// - MaxEnemies: Максимальное количество NPC на сцене (например, 3).
    /// - SpawnInterval: Интервал между спавнами в секундах (например, 10).
    /// - UseRandomSpawn: Включить случайный спавн (true для случайного количества NPC).
    /// - MinSpawnCount: Минимальное количество NPC за спавн (например, 1).
    /// - MaxSpawnCount: Максимальное количество NPC за спавн (например, 3).
    /// - RestrictSpawnUntilCondition: Блокировать спавн до выполнения условия (например, false).
    /// Настройка сцены:
    /// - Привязать к объекту EnemySpawner в компоненте EnemySpawner.
    /// </remarks>
    [CreateAssetMenu(fileName = "EnemySpawnerConfig", menuName = "ShipConfigs/SpawnerConfigs/EnemySpawnerConfig", order = 1)]
    public class EnemySpawnerConfig : ScriptableObject {
        [SerializeField] private int maxEnemies = 3; // Максимальное количество NPC
        /// <summary>
        /// Максимальное количество NPC, которые могут быть активны на сцене одновременно.
        /// </summary>
        public int MaxEnemies => maxEnemies;

        [SerializeField] private float spawnInterval = 10f; // Интервал между спавнами
        /// <summary>
        /// Интервал между спавнами в секундах (используется, если случайный спавн отключен).
        /// </summary>
        public float SpawnInterval => spawnInterval;

        [SerializeField] private bool useRandomSpawn = false; // Включение случайного спавна
        /// <summary>
        /// Включает случайный спавн. Если true, спавнер создаёт случайное количество NPC за один раз, игнорируя интервал.
        /// </summary>
        public bool UseRandomSpawn => useRandomSpawn;

        [SerializeField] private int minSpawnCount = 1; // Минимальное количество NPC за спавн
        /// <summary>
        /// Минимальное количество NPC, спавнимых за один раз в режиме случайного спавна.
        /// </summary>
        public int MinSpawnCount => minSpawnCount;

        [SerializeField] private int maxSpawnCount = 3; // Максимальное количество NPC за спавн
        /// <summary>
        /// Максимальное количество NPC, спавнимых за один раз в режиме случайного спавна.
        /// </summary>
        public int MaxSpawnCount => maxSpawnCount;

        [SerializeField] private bool restrictSpawnUntilCondition = false; // Блокировка спавна до условия
        /// <summary>
        /// Если true, спавн блокируется, пока не выполнено условие, заданное делегатом CanSpawnCondition.
        /// </summary>
        public bool RestrictSpawnUntilCondition => restrictSpawnUntilCondition;
    }
}