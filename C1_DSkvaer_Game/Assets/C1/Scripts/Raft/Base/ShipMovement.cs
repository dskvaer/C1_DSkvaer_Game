using Ship;
using System;
using UnityEngine;

// Компонент для управления движением и поворотом корабля.
// Реализует IShipMovable и IShipDamageable для унифицированного управления.
[RequireComponent(typeof(Rigidbody2D))]
public class ShipMovement : MonoBehaviour, IShipMovable, IShipDamageable {
    // Событие, вызываемое при изменении ввода движения (активен/неактивен).
    public event Action<bool> OnMoveInputChanged;

    [SerializeField] private ShipMovementConfig config; // Настройки движения
    [SerializeField] private SpriteRenderer shipSpriteRenderer; // SpriteRenderer на дочернем ShipVisual для визуализации.
    private Rigidbody2D rb; // Физический компонент для управления движением.
    private Vector2 raftVelocity; // Текущая скорость корабля в мировых координатах.
    private Vector2 knockbackVelocity; // Скорость отбрасывания от столкновений.
    private float rowingTimer; // Таймер для переключения гребков.
    private int currentRowingStep; // Текущий уровень гребка (0-4 для вперёд, 0 для назад).
    private float targetSpeedRatio; // Целевая скорость (0-1, в процентах от BaseSpeed).
    private bool lastMoveInputActive; // Последнее состояние ввода движения.
    private float lastMoveInput; // Последнее значение ввода Y для отслеживания смены направления.

    // Инициализация компонента.
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError($"Rigidbody2D не найден для {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"})!", this);

        // Проверяем SpriteRenderer.
        if (shipSpriteRenderer != null)
        {
            shipSpriteRenderer.sortingLayerName = "Default";
            shipSpriteRenderer.sortingOrder = 10; // Корабль поверх тайлмапа.
        }
        else
        {
            Debug.LogWarning($"SpriteRenderer не привязан в ShipMovement для {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"}). Убедитесь, что он прикреплён к ShipVisual.");
        }

        // Инициализируем скорости и параметры.
        raftVelocity = Vector2.zero;
        knockbackVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        rowingTimer = 0f;
        currentRowingStep = 0;
        targetSpeedRatio = 0f;
        lastMoveInputActive = false;
        lastMoveInput = 0f;
        Debug.Log($"ShipMovement: Initialized for {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"}), Velocity={rb.linearVelocity}, Position={transform.position}");
    }

    // Перемещает корабль в заданном направлении.
    public void ShipMove(Vector2 inputDirection, float inputStrength)
    {
        if (config == null)
        {
            Debug.LogError($"ShipConfig не привязан в ShipMovement для {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"})!", this);
            return;
        }

        Debug.Log($"ShipMovement: Move for {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"}): X={inputDirection.x}, Y={inputDirection.y}, Strength={inputStrength}");

        Vector2 forwardDirection = transform.up; // Нос корабля смотрит вверх.
        float moveInput = inputDirection.y; // Y управляет движением вперёд/назад.

        // Проверка изменения ввода движения.
        bool isMoveInputActive = moveInput != 0f;
        if (isMoveInputActive != lastMoveInputActive)
        {
            lastMoveInputActive = isMoveInputActive;
            OnMoveInputChanged?.Invoke(isMoveInputActive);
            Debug.Log($"ShipMovement: Move input for {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"}): {(isMoveInputActive ? "active" : "inactive")}");
        }

        // Обновляем уровень гребка.
        if (moveInput != 0f)
        {
            // Смена направления (вперёд → назад или наоборот).
            if ((lastMoveInput >= 0f && moveInput < 0f) || (lastMoveInput <= 0f && moveInput > 0f))
            {
                currentRowingStep = 0;
                rowingTimer = 0f;
                Debug.Log($"ShipMovement: Direction changed for {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"}), rowing steps reset");
            }

            if (moveInput > 0f)
            {
                // Движение вперёд: используем RowingSteps.
                targetSpeedRatio = Mathf.Abs(moveInput);
                rowingTimer += Time.fixedDeltaTime;
                if (rowingTimer >= config.RowingInterval)
                {
                    int targetStep = Mathf.FloorToInt(targetSpeedRatio * (config.RowingSteps.Length - 1));
                    if (currentRowingStep < targetStep) currentRowingStep++; // Ускорение.
                    else if (currentRowingStep > targetStep) currentRowingStep--; // Замедление.
                    rowingTimer = 0f;
                }
            }
            else
            {
                // Движение назад: используем ReverseRowingSteps.
                currentRowingStep = 0;
                targetSpeedRatio = Mathf.Abs(moveInput);
            }
        }
        else
        {
            // Нет ввода: снижаем гребок.
            targetSpeedRatio = 0f;
            rowingTimer += Time.fixedDeltaTime;
            if (rowingTimer >= config.RowingInterval)
            {
                currentRowingStep = Mathf.Max(0, currentRowingStep - 1);
                rowingTimer = 0f;
            }
        }

        // Вычисляем целевую скорость.
        float speedModifier = 1f;
        float currentSpeed = moveInput >= 0f
            ? config.BaseSpeed * config.RowingSteps[currentRowingStep]
            : config.BaseSpeed * config.ReverseRowingSteps[0];
        Vector2 targetVelocity = forwardDirection * currentSpeed * speedModifier * Mathf.Sign(moveInput);

        // Плавное изменение скорости.
        float acceleration = moveInput < 0f
            ? config.BaseSpeed * Mathf.Abs(config.ReverseRowingSteps[0])
            : config.Acceleration;
        raftVelocity = Vector2.MoveTowards(raftVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        // Ограничиваем максимальную скорость.
        if (raftVelocity.magnitude > config.MaxSpeed)
        {
            raftVelocity = raftVelocity.normalized * config.MaxSpeed;
        }

        // Учёт отбрасывания.
        knockbackVelocity = Vector2.MoveTowards(knockbackVelocity, Vector2.zero, config.Deceleration * Time.fixedDeltaTime);
        rb.linearVelocity = raftVelocity + knockbackVelocity;

        // Плавное замедление при отсутствии ввода.
        if (moveInput == 0f && currentRowingStep == 0)
        {
            float smoothFriction = config.Friction * 0.5f;
            raftVelocity = Vector2.MoveTowards(raftVelocity, Vector2.zero, smoothFriction * Time.fixedDeltaTime);
        }

        lastMoveInput = moveInput;
        Debug.Log($"ShipMovement: Velocity for {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"}): {rb.linearVelocity}, Direction={forwardDirection}, RowingStep={currentRowingStep}");
    }

    // Поворачивает корабль.
    public void ShipRotate(Vector2 inputDirection, float inputStrength)
    {
        if (config == null)
        {
            Debug.LogError($"ShipConfig не привязан в ShipMovement для {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"})!", this);
            return;
        }

        float rotationInput = inputDirection.x;
        if (rotationInput != 0f)
        {
            float rotationSpeed = config.TurnSpeed * inputStrength;
            float rotationDelta = -rotationInput * rotationSpeed * Time.fixedDeltaTime;
            transform.Rotate(0, 0, rotationDelta);
            Debug.Log($"ShipMovement: Rotate for {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"}): Angle={transform.eulerAngles.z}, Delta={rotationDelta}");
        }
    }

    // Возвращает текущую скорость корабля.
    public Vector2 GetShipVelocity() => raftVelocity;

    // Применяет отбрасывание (реализация IShipDamageable).
    public void ApplyShipKnockback(Vector2 force)
    {
        knockbackVelocity += force;
        Debug.Log($"ShipMovement: Knockback for {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"}): {force}");
    }

    // Наносит урон (реализация IShipDamageable, перенаправляет в ShipHealth).
    public void TakeShipDamage(int amount)
    {
        var health = GetComponent<IShipHealth>();
        if (health != null)
        {
            health.TakeShipDamage(amount);
            Debug.Log($"ShipMovement: Redirected damage {amount} to ShipHealth for {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"})");
        }
    }
}