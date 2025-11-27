using UnityEngine;

// Настройки камеры для следования за кораблём
[CreateAssetMenu(fileName = "ShipCameraConfig", menuName = "Configs/ShipCameraConfig", order = 4)]
public class ShipCameraConfig : ScriptableObject {
    // Скорость следования камеры (чем выше, тем быстрее, 0-1 для Lerp)
    public float FollowSpeed = 0.125f;

    // Смещение камеры относительно корабля (x, y)
    public Vector2 Offset = Vector2.zero;

    // Позиция камеры по оси Z (обычно -10 для 2D)
    public float ZPosition = -10f;
}