using UnityEngine;

namespace Ship {
    /// <summary>
    /// Настройки спавнера NPC.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemySpawnerConfig", menuName = "ShipConfigs/SpawnerConfigs/EnemySpawnerConfig", order = 1)]
    public class EnemySpawnerConfig : ScriptableObject {
        [Header("Что спавнить")]
        [InspectorLabel("Префаб NPC")]
        [Tooltip("Префаб врага, который будет создаваться этим спавнером. Можно оставить пустым и назначить префаб прямо на компоненте спавнера.")]
        [SerializeField] private GameObject enemyPrefab;
        public GameObject EnemyPrefab => enemyPrefab;

        [Header("Количество")]
        [InspectorLabel("Максимум NPC")]
        [Tooltip("Максимальное количество живых NPC, которое спавнер держит одновременно.")]
        [SerializeField, Min(1)] private int maxEnemies = 3;
        public int MaxEnemies => maxEnemies;

        [InspectorLabel("Интервал спавна")]
        [Tooltip("Пауза между обычными волнами спавна в секундах.")]
        [SerializeField, Min(0.1f)] private float spawnInterval = 10f;
        public float SpawnInterval => spawnInterval;

        [InspectorLabel("Случайная волна")]
        [Tooltip("Если включено, спавнер создает случайное количество NPC один раз, затем ждет освобождения места.")]
        [SerializeField] private bool useRandomSpawn = false;
        public bool UseRandomSpawn => useRandomSpawn;

        [InspectorLabel("Мин. NPC за волну")]
        [Tooltip("Минимальное количество NPC в случайной волне.")]
        [SerializeField, Min(1)] private int minSpawnCount = 1;
        public int MinSpawnCount => minSpawnCount;

        [InspectorLabel("Макс. NPC за волну")]
        [Tooltip("Максимальное количество NPC в случайной волне.")]
        [SerializeField, Min(1)] private int maxSpawnCount = 3;
        public int MaxSpawnCount => maxSpawnCount;

        [Header("Зона")]
        [InspectorLabel("Спавнить внутри зоны")]
        [Tooltip("Если включено и у спавнера назначена зона, точки спавна берутся из нее. Иначе используется весь water tilemap.")]
        [SerializeField] private bool spawnInsideZone = true;
        public bool SpawnInsideZone => spawnInsideZone;

        [InspectorLabel("Мин. дистанция от игрока")]
        [Tooltip("Спавнер не создаст NPC ближе этой дистанции к игроку.")]
        [SerializeField, Min(0f)] private float minDistanceFromPlayer = 5f;
        public float MinDistanceFromPlayer => minDistanceFromPlayer;

        [Header("Ограничения")]
        [InspectorLabel("Ждать внешнего условия")]
        [Tooltip("Если включено, спавн запрещен, пока внешний код не разрешит его через SetSpawnCondition.")]
        [SerializeField] private bool restrictSpawnUntilCondition = false;
        public bool RestrictSpawnUntilCondition => restrictSpawnUntilCondition;

        private void OnValidate()
        {
            if (maxSpawnCount < minSpawnCount) {
                maxSpawnCount = minSpawnCount;
            }
        }
    }
}
