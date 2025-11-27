using UnityEngine;
using NPC.Characters.Player;

[RequireComponent(typeof(PlayerStateManager), typeof(PlayerHit))]
public class PlayerCollisionHandler : MonoBehaviour, ICollisionHandler {
    [SerializeField] private CollisionConfig collisionConfig;
    [SerializeField] private float hitCooldown = 1f; // Увеличено до 1 сек
    private PlayerStateManager playerStateManager;
    private PlayerHit playerHit;
    private float lastHitTime;
    private int logCount; // Для ограничения логов
    private const int MAX_LOG_COUNT = 5; // Ограничение на количество логов
    public event System.Action OnCollisionHit;

    private void Awake()
    {
        if (collisionConfig == null)
        {
            collisionConfig = Resources.Load<CollisionConfig>("CollisionConfig");
            if (collisionConfig == null)
            {
                Debug.LogError("PlayerCollisionHandler: CollisionConfig not found in Resources!", this);
                enabled = false;
                return;
            }
        }

        playerStateManager = GetComponent<PlayerStateManager>();
        if (playerStateManager == null)
        {
            Debug.LogError("PlayerCollisionHandler: PlayerStateManager not found!", this);
            enabled = false;
            return;
        }

        playerHit = GetComponent<PlayerHit>();
        if (playerHit == null)
        {
            Debug.LogError("PlayerCollisionHandler: PlayerHit not found!", this);
            enabled = false;
            return;
        }

        Debug.Log("PlayerCollisionHandler: Initialized", this);
    }

    public void HandleCollision(Collision2D collision)
    {
        if (!enabled || collision == null)
        {
            if (logCount < MAX_LOG_COUNT)
            {
                Debug.LogWarning($"PlayerCollisionHandler: Invalid collision or component disabled", this);
                logCount++;
            }
            return;
        }

        if (Time.time < lastHitTime + hitCooldown)
        {
            // Логирование только первых MAX_LOG_COUNT случаев
            if (logCount < MAX_LOG_COUNT)
            {
                Debug.Log($"PlayerCollisionHandler: Cooldown active (Time: {Time.time}, LastHit: {lastHitTime}, Cooldown: {hitCooldown})", this);
                logCount++;
            }
            return;
        }

        if (!playerStateManager.IsArmed)
        {
            // Логируем только если состояние изменилось или редко
            if (logCount < MAX_LOG_COUNT)
            {
                Debug.Log("PlayerCollisionHandler: Player not in Armed state, ignoring collision", this);
                logCount++;
            }
            return;
        }

        int collisionLayer = collision.gameObject.layer;
        int combinedLayerMask = collisionConfig.WallLayer | collisionConfig.PushableLayer;
        foreach (var layer in collisionConfig.AdditionalLayers)
        {
            combinedLayerMask |= layer;
        }

        if (((1 << collisionLayer) & combinedLayerMask) != 0)
        {
            lastHitTime = Time.time;
            logCount = 0; // Сбрасываем счётчик логов при успешном Hit
            OnCollisionHit?.Invoke();
            Debug.Log($"PlayerCollisionHandler: Collision with {(collision.gameObject.layer == collisionConfig.WallLayer ? "Wall" : collision.gameObject.layer == collisionConfig.PushableLayer ? "Pushable" : "AdditionalLayer")} detected, triggering Hit", this);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleCollision(collision);
    }
}