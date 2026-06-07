using UnityEngine;

[CreateAssetMenu(fileName = "ShipCameraConfig", menuName = "Configs/ShipCameraConfig", order = 4)]
public class ShipCameraConfig : ScriptableObject {
    [Header("Следование камеры")]
    [InspectorLabel("Плавность следования")]
    [Tooltip("Скорость сглаживания движения камеры к цели. Меньше значение - плавнее и медленнее.")]
    public float FollowSpeed = 0.125f;

    [InspectorLabel("Смещение")]
    [Tooltip("Смещение камеры относительно цели по X и Y.")]
    public Vector2 Offset = Vector2.zero;

    [InspectorLabel("Позиция Z")]
    [Tooltip("Глубина камеры по оси Z. Для 2D обычно используется -10.")]
    public float ZPosition = -10f;
}
