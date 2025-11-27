using UnityEngine;

// Компонент для управления камерой, следующей за кораблём
public class ShipCamera : MonoBehaviour {
    [SerializeField] private ShipCameraConfig config; // Настройки камеры
    [SerializeField] private Transform target; // Цель (Player_Ship)

    // Инициализация при старте
    private void Awake()
    {
        // Проверка привязки компонентов
        if (config == null)
        {
            Debug.LogError("ShipCameraConfig не привязан в Inspector!");
            return;
        }
        if (target == null)
        {
            Debug.LogError("Цель (Player_Ship) не привязана в Inspector!");
            return;
        }

        // Устанавливаем начальную позицию камеры
        transform.position = new Vector3(target.position.x + config.Offset.x, target.position.y + config.Offset.y, config.ZPosition);
        transform.rotation = Quaternion.identity; // Фиксируем вращение (z=0)
        Debug.Log($"Камера инициализирована: Позиция={transform.position}, Вращение={transform.rotation.eulerAngles}");
    }

    // Обновление позиции камеры каждый кадр
    private void LateUpdate()
    {
        if (config == null || target == null) return;

        // Целевая позиция с учётом смещения
        Vector3 targetPosition = new Vector3(target.position.x + config.Offset.x, target.position.y + config.Offset.y, config.ZPosition);

        // Плавное следование с помощью Lerp
        transform.position = Vector3.Lerp(transform.position, targetPosition, config.FollowSpeed);

        // Фиксируем вращение камеры (z=0)
        transform.rotation = Quaternion.identity;

        Debug.Log($"Камера: Позиция={transform.position}, Цель={targetPosition}");
    }
}