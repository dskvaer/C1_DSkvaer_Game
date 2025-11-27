using UnityEngine;

// Компонент для слежения полосы здоровья за целью (например, Enemy_Ship) в экранном пространстве.
public class FollowTarget : MonoBehaviour {
    [SerializeField] private Transform target; // Цель (Enemy_Ship), за которой следует полоса.
    [SerializeField] private Vector2 offset = new Vector2(0, 1); // Смещение полосы над целью.
    private Canvas canvas; // Canvas (Screen Space - Camera) для позиционирования.
    private RectTransform rectTransform; // RectTransform для полосы здоровья.

    // Инициализация компонента.
    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        if (canvas == null)
        {
            Debug.LogError("FollowTarget: Canvas не найден!", this);
            enabled = false;
            return;
        }
        if (rectTransform == null)
        {
            Debug.LogError("FollowTarget: RectTransform не найден!", this);
            enabled = false;
            return;
        }
        if (target == null)
        {
            Debug.LogError("FollowTarget: Target не привязан!", this);
            enabled = false;
            return;
        }
    }

    // Обновляет позицию полосы в экранном пространстве.
    private void LateUpdate()
    {
        if (target == null || canvas == null || rectTransform == null) return;

        Vector3 worldPos = target.position + new Vector3(offset.x, offset.y, 0);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPos,
            canvas.worldCamera,
            out Vector2 localPoint
        );
        rectTransform.localPosition = new Vector3(localPoint.x, localPoint.y, 0);
        ShipID shipID = target.GetComponent<ShipID>();
        Debug.Log($"FollowTarget: Position updated for {target.name} (ID={shipID?.ID ?? "Unknown"}) to local={localPoint}, world={worldPos}");
    }

    // Устанавливает новую цель для слежения.
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        ShipID shipID = newTarget?.GetComponent<ShipID>();
        Debug.Log($"FollowTarget: Target set to {newTarget.name} (ID={shipID?.ID ?? "Unknown"})");
    }
}