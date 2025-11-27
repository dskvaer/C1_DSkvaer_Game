using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EdgeBalanceChecker : MonoBehaviour, IEdgeBalanceChecker {
    [SerializeField] private EdgeBalanceConfigSO config;

    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        if (!TryGetComponent<Rigidbody2D>(out _rigidbody))
        {
#if DEBUG
            Debug.LogError("EdgeBalanceChecker: Rigidbody2D не найден!", this);
#endif
            enabled = false;
            return;
        }

        if (config == null)
        {
#if DEBUG
            Debug.LogError("EdgeBalanceChecker: EdgeBalanceConfigSO не привязан!", this);
#endif
            enabled = false;
            return;
        }

        if (!config.IsValid())
        {
#if DEBUG
            Debug.LogError("EdgeBalanceChecker: EdgeBalanceConfigSO содержит невалидные настройки!", this);
#endif
            enabled = false;
            return;
        }

#if DEBUG
        Debug.Log($"EdgeBalanceChecker: Инициализирован, config={config.name}, groundLayer={(config.IsValid() ? LayerMask.LayerToName((int)Mathf.Log(config.GroundLayer.value, 2)) : "Invalid")}, pushableLayer={(config.IsValid() ? LayerMask.LayerToName((int)Mathf.Log(config.PushableLayer.value, 2)) : "Invalid")}", this);
#endif
    }

    public bool IsAtEdge()
    {
        if (!IsGrounded()) return false;

        Vector2 center = _rigidbody.position;
        Vector2 leftRay = center + new Vector2(-config.EdgeCheckDistance, 0f);
        Vector2 rightRay = center + new Vector2(config.EdgeCheckDistance, 0f);

        bool leftHit = Physics2D.Raycast(leftRay, Vector2.down, config.EdgeCheckDepth, config.GroundLayer | config.PushableLayer).collider != null;
        bool rightHit = Physics2D.Raycast(rightRay, Vector2.down, config.EdgeCheckDepth, config.GroundLayer | config.PushableLayer).collider != null;


        return (leftHit && !rightHit) || (!leftHit && rightHit);
    }

    public bool IsGrounded()
    {
        Vector2 rayOrigin = _rigidbody.position + new Vector2(0f, -0.5f);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 0.1f, config.GroundLayer | config.PushableLayer);


        return hit.collider != null;
    }
}