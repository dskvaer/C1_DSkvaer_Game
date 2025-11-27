using UnityEngine;
using UnityEngine.InputSystem;
using NPC.Characters.Player;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInput), typeof(PlayerStateManager))]
public class PlayerMovement : MonoBehaviour, IMovable {
    [SerializeField] private MovementSettings movementSettings;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PlayerInput playerInput;
    private PlayerStateManager stateManager;
    private PlayerPush playerPush;
    private float maxSpeed;
    private float acceleration;
    private float deceleration;

    public Vector2 MoveInput => moveInput;
    public Rigidbody2D Rigidbody => rb;

    public void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        stateManager = GetComponent<PlayerStateManager>();
        playerPush = GetComponent<PlayerPush>();

        if (!rb)
        {
            Debug.LogError("PlayerMovement: Rigidbody2D not found!", this);
            return;
        }
        if (!playerInput)
        {
            Debug.LogError("PlayerMovement: PlayerInput not found!", this);
            return;
        }
        if (!movementSettings)
        {
            Debug.LogError("PlayerMovement: MovementSettings not assigned!", this);
            return;
        }
        if (!stateManager)
        {
            Debug.LogWarning("PlayerMovement: PlayerStateManager not found.", this);
        }
        if (!playerPush)
        {
            Debug.LogWarning("PlayerMovement: PlayerPush not found.", this);
        }

        rb.gravityScale = 1f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        UpdateMovementSettings();
        Subscribe();
    }

    private void UpdateMovementSettings()
    {
        if (stateManager && stateManager.IsArmed)
        {
            maxSpeed = movementSettings.ArmedMaxSpeed;
            acceleration = movementSettings.ArmedAcceleration;
            deceleration = movementSettings.ArmedDeceleration;
        }
        else
        {
            maxSpeed = movementSettings.StandardMaxSpeed;
            acceleration = movementSettings.StandardAcceleration;
            deceleration = movementSettings.StandardDeceleration;
        }
    }

    private void Subscribe()
    {
        if (playerInput == null || playerInput.actions == null)
        {
            Debug.LogError("PlayerMovement: PlayerInput or actions are null!", this);
            return;
        }
        var moveAction = playerInput.actions.FindAction("Move");
        if (moveAction == null)
        {
            Debug.LogError("PlayerMovement: Move action not found!", this);
            return;
        }
        moveAction.performed += OnMovePerformed;
        moveAction.canceled += OnMoveCanceled;
    }

    private void Unsubscribe()
    {
        if (playerInput == null || playerInput.actions == null) return;
        var moveAction = playerInput.actions.FindAction("Move");
        if (moveAction != null)
        {
            moveAction.performed -= OnMovePerformed;
            moveAction.canceled -= OnMoveCanceled;
        }
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void FixedUpdate()
    {
        if (!rb || !playerInput || !movementSettings)
        {
            return;
        }

        UpdateMovementSettings();
        HandleMovement();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    public void Move(Vector2 input)
    {
        moveInput = input;
    }

    public void SetSpeed(float speed)
    {
        maxSpeed = speed;
    }

    private void HandleMovement()
    {
        if (playerPush != null && playerPush.IsInPushContact && !playerPush.IsPushing)
        {
            return;
        }

        float targetSpeed = moveInput.x * maxSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        float movement = speedDiff * accelRate * Time.fixedDeltaTime * 100f;
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);

        if (Mathf.Abs(rb.linearVelocity.x) > maxSpeed)
        {
            rb.linearVelocity = new Vector2(Mathf.Sign(rb.linearVelocity.x) * maxSpeed, rb.linearVelocity.y);
        }
    }
}