using UnityEngine;

public class ShipCamera : MonoBehaviour {
    [Header("Следование")]
    [InspectorLabel("Конфиг камеры")]
    [Tooltip("ScriptableObject с плавностью, смещением и Z-позицией камеры.")]
    [SerializeField] private ShipCameraConfig config;

    [InspectorLabel("Цель")]
    [Tooltip("Transform корабля или другого объекта, за которым должна следовать камера.")]
    [SerializeField] private Transform target;

    private void Awake()
    {
        if (config == null)
        {
            Debug.LogError("ShipCameraConfig не назначен в инспекторе!", this);
            return;
        }
        if (target == null)
        {
            Debug.LogError("Цель камеры не назначена в инспекторе!", this);
            return;
        }

        transform.position = new Vector3(target.position.x + config.Offset.x, target.position.y + config.Offset.y, config.ZPosition);
        transform.rotation = Quaternion.identity;
    }

    private void LateUpdate()
    {
        if (config == null || target == null) return;

        Vector3 targetPosition = new Vector3(target.position.x + config.Offset.x, target.position.y + config.Offset.y, config.ZPosition);
        transform.position = Vector3.Lerp(transform.position, targetPosition, config.FollowSpeed);
        transform.rotation = Quaternion.identity;
    }
}
