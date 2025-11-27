using UnityEngine;

public class ParallaxBackground : MonoBehaviour {
    public enum MovementDirection { Left, Right } // Перечисление для направления движения

    [SerializeField] private GameObject skySpritePrefab; // Префаб спрайта неба (Sky.png)
    [SerializeField] private float speed = 1f; // Скорость движения фона (единицы/сек)
    [SerializeField] private float spriteWidth = 19.2f; // Ширина спрайта в единицах Unity
    [SerializeField] private Camera mainCamera; // Основная камера для расчёта границ
    [SerializeField] private int spriteCount = 4; // Количество спрайтов (1-2-3-4)
    [SerializeField] private float x0 = 0f; // Начальная X-позиция центра Sky_2
    [SerializeField] private float y0 = 0f; // Начальная Y-позиция центра Sky_2
    [SerializeField] private MovementDirection direction = MovementDirection.Left; // Направление движения
    [SerializeField] private float overlap = 0.01f; // Наложение спрайтов для устранения пробелов

    private Transform[] backgroundSprites; // Массив спрайтов (Sky_1, Sky_2, Sky_3, Sky_4)
    private float cameraEdge; // Край области видимости камеры (левый или правый, в зависимости от направления)

    void Start()
    {
        // Проверяем, что префаб и камера назначены
        if (skySpritePrefab == null)
        {
            Debug.LogError("Префаб SkySprite не назначен!");
            return;
        }
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Основная камера не найдена!");
                return;
            }
        }

        // Создаём массив спрайтов
        backgroundSprites = new Transform[spriteCount];
        // Вычисляем начальную позицию так, чтобы Sky_2 был в (x0, y0)
        float startX = x0 - spriteWidth; // Sky_1 слева от Sky_2

        for (int i = 0; i < spriteCount; i++)
        {
            // Создаём спрайт как дочерний объект Backgrounds
            GameObject spriteObj = Instantiate(skySpritePrefab, transform);
            spriteObj.name = $"Sky_{(i + 1)}";
            // Устанавливаем позицию: Sky_1, Sky_2 (центр в x0, y0), Sky_3, Sky_4
            float xPosition = startX + (i * (spriteWidth - overlap));
            spriteObj.transform.position = new Vector3(xPosition, y0, 10);
            backgroundSprites[i] = spriteObj.transform;
        }
    }

    void Update()
    {
        // Вычисляем край камеры в зависимости от направления
        float cameraWidth = mainCamera.orthographicSize * mainCamera.aspect;
        cameraEdge = direction == MovementDirection.Left
            ? mainCamera.transform.position.x - cameraWidth
            : mainCamera.transform.position.x + cameraWidth;

        // Перемещаем каждый спрайт
        for (int i = 0; i < backgroundSprites.Length; i++)
        {
            Vector3 newPosition = backgroundSprites[i].position;
            newPosition.x += (direction == MovementDirection.Left ? -speed : speed) * Time.deltaTime;
            backgroundSprites[i].position = newPosition;

            // Проверяем, вышел ли спрайт за край камеры
            bool outOfView = direction == MovementDirection.Left
                ? backgroundSprites[i].position.x + spriteWidth / 2 <= cameraEdge
                : backgroundSprites[i].position.x - spriteWidth / 2 >= cameraEdge;

            if (outOfView)
            {
                // Находим крайний спрайт (самый правый для Left, самый левый для Right)
                float extremeX = backgroundSprites[0].position.x;
                for (int j = 1; j < backgroundSprites.Length; j++)
                {
                    if (direction == MovementDirection.Left)
                    {
                        if (backgroundSprites[j].position.x > extremeX)
                        {
                            extremeX = backgroundSprites[j].position.x;
                        }
                    }
                    else
                    {
                        if (backgroundSprites[j].position.x < extremeX)
                        {
                            extremeX = backgroundSprites[j].position.x;
                        }
                    }
                }
                // Перемещаем спрайт в конец очереди
                newPosition.x = extremeX + (direction == MovementDirection.Left ? spriteWidth - overlap : -(spriteWidth - overlap));
                backgroundSprites[i].position = newPosition;
            }
        }
    }
}