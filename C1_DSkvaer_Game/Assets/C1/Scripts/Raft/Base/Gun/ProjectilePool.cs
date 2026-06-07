using System.Collections.Generic;
using UnityEngine;

namespace Ship {
    /// <summary>
    /// Простой пул для снарядов, чтобы переиспользовать объекты вместо постоянного создания.
    /// </summary>
    public class ProjectilePool : MonoBehaviour {
        public static ProjectilePool Instance { get; private set; }

        [Header("Предзагрузка снарядов")]
        [InspectorLabel("Префабы снарядов")]
        [Tooltip("Префабы, которые пул создаст заранее при старте.")]
        public GameObject[] registeredPrefabs;

        [InspectorLabel("Количество на префаб")]
        [Tooltip("Сколько экземпляров каждого префаба создать заранее.")]
        [SerializeField, Min(0)] private int warmCountPerPrefab = 0;

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
            var meta = go.GetComponent<PoolMeta>();
            if (meta == null) meta = go.AddComponent<PoolMeta>();
            meta.originalPrefab = prefab;
            return go;
        }

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

        public void ReturnProjectile(GameObject projectile)
        {
            if (projectile == null) return;

            var meta = projectile.GetComponent<PoolMeta>();
            if (meta == null || meta.originalPrefab == null)
            {
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

        private class PoolMeta : MonoBehaviour {
            public GameObject originalPrefab;
        }
    }
}
