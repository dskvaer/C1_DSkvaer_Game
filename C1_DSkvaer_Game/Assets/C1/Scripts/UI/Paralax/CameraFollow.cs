using UnityEngine;

public class CameraFollow : MonoBehaviour {
    [SerializeField] private Transform target; // Цель (игрок)
    [SerializeField] private Vector3 offset; // Смещение камеры относительно игрока
    [SerializeField] private float smoothSpeed = 0.125f; // Скорость сглаживания (0.0 - моментально, 1.0 - очень медленно)
    [SerializeField] private bool useBounds; // Использовать границы?
    [SerializeField] private Vector2 minBounds; // Минимальные координаты камеры (x, y)
    [SerializeField] private Vector2 maxBounds; // Максимальные координаты камеры (x, y)

    private void Start()
    {
        // Найти игрока по тегу, если не привязан
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogError("Цель для камеры не найдена! Убедитесь, что объект игрока имеет тег 'Player'.");
                enabled = false;
                return;
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Вычисляем желаемую позицию камеры
        Vector3 desiredPosition = target.position + offset;
        // Плавное перемещение
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Ограничиваем позицию, если включены границы
        if (useBounds)
        {
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minBounds.x, maxBounds.x);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minBounds.y, maxBounds.y);
        }

        // Применяем позицию, сохраняя z камеры
        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
    }
}