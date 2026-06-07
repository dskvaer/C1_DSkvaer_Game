using C1.Scripts.UI.Game_play_UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

namespace Ship {
    public class EnemySpawner : MonoBehaviour {
        [Header("Настройки спавна")]
        [InspectorLabel("Конфиг спавнера")]
        [Tooltip("ScriptableObject с правилами спавна: интервал, лимит врагов, случайная волна и prefab по умолчанию.")]
        [SerializeField] private EnemySpawnerConfig config;

        [InspectorLabel("Конфиг ID")]
        [Tooltip("Настройки генерации ID для созданных врагов. Можно не задавать, если prefab имеет свой IShipIdProvider.")]
        [SerializeField] private ShipIDConfig idConfig;

        [InspectorLabel("Префаб врага")]
        [Tooltip("Префаб, который будет создан спавнером. Если пусто, используется prefab из конфига спавнера.")]
        [SerializeField] private GameObject enemyPrefab;

        [InspectorLabel("Зона спавна")]
        [Tooltip("Зона, внутри которой появляются враги и которая передается им как зона патруля по умолчанию.")]
        [SerializeField] private NPCPatrolZone spawnZone;

        [InspectorLabel("Tilemap воды")]
        [Tooltip("Tilemap воды, по которому выбираются допустимые точки спавна и патруля.")]
        [SerializeField] private Tilemap waterTilemap;

        [Header("Полоски здоровья")]
        [InspectorLabel("Canvas здоровья")]
        [Tooltip("Canvas, в который будут добавляться полоски здоровья созданных врагов.")]
        [SerializeField] private Canvas healthBarCanvas;

        [InspectorLabel("Префаб полоски здоровья")]
        [Tooltip("UI prefab полоски здоровья, который привязывается к каждому созданному врагу.")]
        [SerializeField] private GameObject healthBarPrefab;
        [SerializeField] private bool createHealthBarsForSceneEnemies = true;

        private readonly Dictionary<string, SpawnedEntity> spawnedEntities = new();
        private float spawnTimer;
        private bool isRandomSpawnTriggered;
        private System.Func<bool> canSpawnCondition;

        private sealed class SpawnedEntity {
            public GameObject Enemy;
            public GameObject HealthBar;
            public ShipHealth ShipHealth;
            public UnityAction OnDeath;
        }

        private void Awake()
        {
            if (!ValidateSetup()) {
                enabled = false;
                return;
            }

            spawnTimer = config.SpawnInterval;
        }

        private void Start()
        {
            if (createHealthBarsForSceneEnemies) {
                CreateHealthBarsForExistingEnemies();
            }
        }

        private void Update()
        {
            if (config.RestrictSpawnUntilCondition && canSpawnCondition != null && !canSpawnCondition()) {
                return;
            }

            if (config.UseRandomSpawn) {
                if (!isRandomSpawnTriggered && spawnedEntities.Count < config.MaxEnemies) {
                    TriggerRandomSpawn();
                    isRandomSpawnTriggered = true;
                }

                return;
            }

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f && spawnedEntities.Count < config.MaxEnemies) {
                SpawnEnemy();
                spawnTimer = config.SpawnInterval;
            }
        }

        public void SetSpawnCondition(System.Func<bool> condition)
        {
            canSpawnCondition = condition;
        }

        private bool ValidateSetup()
        {
            enemyPrefab = enemyPrefab != null ? enemyPrefab : config != null ? config.EnemyPrefab : null;
            spawnZone = spawnZone != null ? spawnZone : GetComponent<NPCPatrolZone>();

            if (config == null || enemyPrefab == null || waterTilemap == null || healthBarCanvas == null || healthBarPrefab == null) {
                return false;
            }

            if (enemyPrefab.GetComponent<ShipPlayerInputHandler>() != null) {
                return false;
            }

            return idConfig != null || GetIdProvider(enemyPrefab) != null;
        }

        private void TriggerRandomSpawn()
        {
            int spawnCount = Random.Range(config.MinSpawnCount, config.MaxSpawnCount + 1);
            for (int i = 0; i < spawnCount && spawnedEntities.Count < config.MaxEnemies; i++) {
                SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            Vector2 spawnPoint = FindSpawnPoint();
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint, Quaternion.identity);
            enemy.SetActive(true);
            AssignPatrolZone(enemy);

            IShipIdProvider idProvider = GetIdProvider(enemy);
            string assignedId = ResolveUniqueId(idProvider);
            string assignedTag = ResolveTag(idProvider, assignedId);

            TrySetTag(enemy, assignedTag);

            ShipID shipID = enemy.GetComponent<ShipID>();
            if (shipID == null) {
                shipID = enemy.AddComponent<ShipID>();
            }

            shipID.ID = assignedId;
            enemy.name = $"{enemy.tag}_Ship_{assignedId}";

            ShipHealth shipHealth = enemy.GetComponent<ShipHealth>() ?? enemy.GetComponentInChildren<ShipHealth>(true);
            if (shipHealth != null) {
                shipHealth.ResetHealth();
            }

            GameObject healthBar = CreateHealthBar(assignedId, enemy.transform, shipHealth);
            if (shipHealth == null) {
                Debug.LogWarning($"EnemySpawner: spawned enemy {enemy.name} has no ShipHealth, health bar was not created.", enemy);
            }

            UnityAction onDeath = () => RemoveEntity(assignedId);
            if (shipHealth != null) {
                shipHealth.OnDeath.AddListener(onDeath);
            }

            spawnedEntities[assignedId] = new SpawnedEntity {
                Enemy = enemy,
                HealthBar = healthBar,
                ShipHealth = shipHealth,
                OnDeath = onDeath
            };
        }

        private void CreateHealthBarsForExistingEnemies()
        {
            ShipHealth[] sceneHealth = FindObjectsByType<ShipHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < sceneHealth.Length; i++) {
                ShipHealth shipHealth = sceneHealth[i];
                if (shipHealth == null || shipHealth.CompareTag("Player")) {
                    continue;
                }

                IShipIdProvider idProvider = GetIdProvider(shipHealth.gameObject);
                string assignedId = ResolveUniqueId(idProvider);
                if (spawnedEntities.ContainsKey(assignedId)) {
                    continue;
                }

                ShipID shipID = shipHealth.GetComponent<ShipID>();
                if (shipID == null) {
                    shipID = shipHealth.gameObject.AddComponent<ShipID>();
                }

                shipID.ID = assignedId;
                GameObject healthBar = CreateHealthBar(assignedId, shipHealth.transform, shipHealth);
                UnityAction onDeath = () => RemoveEntity(assignedId);
                shipHealth.OnDeath.AddListener(onDeath);

                spawnedEntities[assignedId] = new SpawnedEntity {
                    Enemy = shipHealth.gameObject,
                    HealthBar = healthBar,
                    ShipHealth = shipHealth,
                    OnDeath = onDeath
                };
            }
        }

        private GameObject CreateHealthBar(string assignedId, Transform target, ShipHealth shipHealth)
        {
            if (shipHealth == null || target == null) {
                return null;
            }

            GameObject healthBar = Instantiate(healthBarPrefab, healthBarCanvas.transform);
            healthBar.name = $"HealthBar_{assignedId}";

            HealthBar healthBarScript = healthBar.GetComponent<HealthBar>();
            FollowTarget followTarget = healthBar.GetComponent<FollowTarget>() ?? healthBar.AddComponent<FollowTarget>();
            if (healthBarScript != null) {
                healthBarScript.SetHealthComponent(shipHealth);
            }

            if (followTarget != null) {
                followTarget.SetTarget(target);
            }

            return healthBar;
        }

        private Vector2 FindSpawnPoint()
        {
            Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
            Vector2 spawnPoint;

            do {
                if (config.SpawnInsideZone && spawnZone != null) {
                    spawnPoint = spawnZone.GetRandomPoint(waterTilemap);
                }
                else {
                    Bounds bounds = waterTilemap.localBounds;
                    spawnPoint = new Vector2(
                        Random.Range(bounds.min.x, bounds.max.x),
                        Random.Range(bounds.min.y, bounds.max.y)
                    );
                    spawnPoint = waterTilemap.transform.TransformPoint(spawnPoint);
                }
            } while (player != null && Vector2.Distance(spawnPoint, player.position) < config.MinDistanceFromPlayer);

            return spawnPoint;
        }

        private void AssignPatrolZone(GameObject enemy)
        {
            if (enemy == null || spawnZone == null) {
                return;
            }

            PatrolTactic[] patrols = enemy.GetComponentsInChildren<PatrolTactic>(true);
            for (int i = 0; i < patrols.Length; i++) {
                patrols[i]?.SetPatrolZone(spawnZone);
            }
        }

        private IShipIdProvider GetIdProvider(GameObject source)
        {
            if (source == null) {
                return null;
            }

            IShipIdProvider provider = source.GetComponent<IShipIdProvider>();
            return provider ?? source.GetComponentInChildren<IShipIdProvider>(true);
        }

        private string ResolveUniqueId(IShipIdProvider idProvider)
        {
            string id = idProvider?.CreateId();
            if (string.IsNullOrWhiteSpace(id) && idConfig != null) {
                id = idConfig.GetID();
            }

            if (string.IsNullOrWhiteSpace(id)) {
                id = $"{enemyPrefab.name}_{Random.Range(1000, 9999)}";
            }

            string uniqueId = id;
            int suffix = 1;
            while (spawnedEntities.ContainsKey(uniqueId)) {
                uniqueId = $"{id}_{suffix}";
                suffix++;
            }

            return uniqueId;
        }

        private string ResolveTag(IShipIdProvider idProvider, string id)
        {
            string tagHint = idProvider?.TagHint;
            if (!string.IsNullOrWhiteSpace(tagHint)) {
                return tagHint;
            }

            if (id.StartsWith("RE") || id.StartsWith("RB")) {
                return "Enemy";
            }

            return "Trader";
        }

        private void TrySetTag(GameObject target, string tagName)
        {
            try {
                target.tag = tagName;
            }
            catch (UnityException) {
                target.tag = "Untagged";
            }
        }

        private void RemoveEntity(string id)
        {
            if (!spawnedEntities.TryGetValue(id, out SpawnedEntity entity)) {
                return;
            }

            if (entity.ShipHealth != null && entity.OnDeath != null) {
                entity.ShipHealth.OnDeath.RemoveListener(entity.OnDeath);
            }

            if (entity.Enemy != null) {
                Destroy(entity.Enemy);
            }

            if (entity.HealthBar != null) {
                Destroy(entity.HealthBar);
            }

            spawnedEntities.Remove(id);
            isRandomSpawnTriggered = false;
        }
    }
}
