using UnityEngine;

public class FollowTarget : MonoBehaviour {
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 offset = new Vector2(0f, 1f);

    private Canvas canvas;
    private RectTransform rectTransform;
    private Camera mainCamera;

    private void Awake()
    {
        CacheReferences();
    }

    private void LateUpdate()
    {
        if (target == null) return;
        if (!CacheReferences()) return;

        if (mainCamera == null) {
            mainCamera = Camera.main;
        }

        if (mainCamera == null) return;

        Vector3 worldPos = target.position + new Vector3(offset.x, offset.y, 0f);
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localPoint
        );
        rectTransform.localPosition = new Vector3(localPoint.x, localPoint.y, 0f);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        enabled = target != null;
        LateUpdate();
    }

    private bool CacheReferences()
    {
        if (canvas == null) {
            canvas = GetComponentInParent<Canvas>();
        }

        if (rectTransform == null) {
            rectTransform = GetComponent<RectTransform>();
        }

        return canvas != null && rectTransform != null;
    }
}
