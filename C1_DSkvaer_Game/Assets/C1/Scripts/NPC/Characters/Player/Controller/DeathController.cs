using UnityEngine;

[RequireComponent(typeof(HealthManager))]
public class DeathController : MonoBehaviour, IDeathHandler {
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D[] colliders;
    [SerializeField] private bool destroyOnDeath = false;
    [SerializeField] private float destroyDelay = 3f;

    private HealthManager health;

    private void Awake()
    {
        health = GetComponent<HealthManager>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (colliders == null || colliders.Length == 0)
            colliders = GetComponentsInChildren<Collider2D>();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDeath.AddListener(HandleDeath);
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDeath.RemoveListener(HandleDeath);
        }
    }

    public void HandleDeath()
    {
        Debug.Log("DeathController: Character died!");

        // Отключаем физику
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic; // вместо isKinematic
        }

        // Отключаем коллайдеры
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        // Уничтожаем объект (по желанию)
        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }
}
