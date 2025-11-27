using System.Collections.Generic;
using UnityEngine;

namespace Ship {
    /// <summary>
    /// Простой пул для префабов снарядов.
    /// Реализован как Singleton для быстрого доступа из GunWeaponSystem/Projectile.
    /// В инспекторе можно зарегистрировать префабы, чтобы пул заранее их подготовил.
    /// </summary>
    public class ProjectilePool : MonoBehaviour {
        public static ProjectilePool Instance { get; private set; }

        [Header("Optional: register prefabs to warm the pool")]
        public GameObject[] registeredPrefabs;
        [SerializeField, Min(0)] private int warmCountPerPrefab = 0;

        // очередь по префабу (ключ — original prefab)
        private readonly Dictionary<GameObject, Queue<GameObject>> pool = new Dictionary<GameObject, Queue<GameObject>>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (registeredPrefabs != null)
            {
                foreach (var p in registeredPrefabs)
                {
                    if (p == null) continue;
                    if (!pool.ContainsKey(p)) pool[p] = new Queue<GameObject>();
                    for (int i = 0; i < warmCountPerPrefab; i++)
                    {
                        var go = CreateNewInstance(p);
                        ReturnProjectile(go);
                    }
                }
            }
        }

        private GameObject CreateNewInstance(GameObject prefab)
        {
            var go = Instantiate(prefab, transform);
            go.SetActive(false);
            // помечаем экземпляр, чтобы понимать, к какому префабу он относится
            var meta = go.GetComponent<PoolMeta>();
            if (meta == null) meta = go.AddComponent<PoolMeta>();
            meta.originalPrefab = prefab;
            return go;
        }

        /// <summary>Взять снаряд из пула или создать новый экземпляр.</summary>
        public GameObject GetProjectile(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            if (!pool.TryGetValue(prefab, out var q))
            {
                q = new Queue<GameObject>();
                pool[prefab] = q;
            }

            GameObject instance = null;
            while (q.Count > 0)
            {
                var candidate = q.Dequeue();
                if (candidate == null) continue;
                instance = candidate;
                break;
            }

            if (instance == null) instance = CreateNewInstance(prefab);

            instance.transform.SetParent(null);
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.SetActive(true);
            return instance;
        }

        /// <summary>Возвращает снаряд в пул (дизактивирует и кладёт в очередь).</summary>
        public void ReturnProjectile(GameObject projectile)
        {
            if (projectile == null) return;

            var meta = projectile.GetComponent<PoolMeta>();
            if (meta == null || meta.originalPrefab == null)
            {
                // если у объекта нет меты — уничтожаем, чтобы не держать неопределённые объекты
                Destroy(projectile);
                return;
            }

            var prefab = meta.originalPrefab;
            if (!pool.TryGetValue(prefab, out var q))
            {
                q = new Queue<GameObject>();
                pool[prefab] = q;
            }

            projectile.SetActive(false);
            projectile.transform.SetParent(transform);
            q.Enqueue(projectile);
        }

        // простой компонент-маркер
        private class PoolMeta : MonoBehaviour {
            public GameObject originalPrefab;
        }
    }
}
