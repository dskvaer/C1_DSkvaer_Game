using UnityEngine;

public class Parallax : MonoBehaviour {
    [SerializeField] private Transform cameraTransform; // Камера, за которой следим
    [SerializeField] private float parallaxSpeed = 0.5f; // Скорость параллакса (0 - неподвижный, 1 - движется с камерой)
    [SerializeField] private bool infiniteScroll; // Бесконечный скроллинг?
    [SerializeField] private float backgroundLength; // Длина фона (для бесконечного скроллинга)

    private Vector3 startPos; // Начальная позиция фона
    private float cameraStartX; // Начальная позиция камеры по X

    private void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
            if (cameraTransform == null)
            {
                Debug.LogError("Камера не найдена! Привяжите камеру в инспекторе.");
                enabled = false;
                return;
            }
        }

        startPos = transform.position;
        cameraStartX = cameraTransform.position.x;

        // Если backgroundLength не задан, пытаемся вычислить из SpriteRenderer
        if (infiniteScroll && backgroundLength == 0)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                backgroundLength = spriteRenderer.bounds.size.x;
            }
            else
            {
                Debug.LogWarning("BackgroundLength не задан и не удалось вычислить из SpriteRenderer!");
                infiniteScroll = false;
            }
        }
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        // Вычисляем смещение камеры
        float deltaX = cameraTransform.position.x - cameraStartX;
        float newX = startPos.x + deltaX * parallaxSpeed;

        // Применяем позицию
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);

        // Бесконечный скроллинг
        if (infiniteScroll && backgroundLength > 0)
        {
            float cameraOffset = cameraTransform.position.x - newX;
            if (cameraOffset > backgroundLength)
            {
                startPos.x += backgroundLength;
            }
            else if (cameraOffset < -backgroundLength)
            {
                startPos.x -= backgroundLength;
            }
        }
    }
}