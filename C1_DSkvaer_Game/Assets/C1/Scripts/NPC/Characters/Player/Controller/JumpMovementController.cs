using UnityEngine;
using NPC.Characters.Player;

[RequireComponent(typeof(PlayerMovement), typeof(JumpStateController))]
public class JumpMovementController : MonoBehaviour, IMovable {
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private JumpStateController jumpStateController;
    [SerializeField] private MovementSettings movementSettings;
    [SerializeField] private float airControlFactor = 0.5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float maxSpeed;

    public Vector2 MoveInput => moveInput;
    public Rigidbody2D Rigidbody => rb;

    private void Awake()
    {
        if (!TryGetComponent(out playerMovement))
        {
            Debug.LogError("JumpMovementController: PlayerMovement not found!", this);
            enabled = false;
            return;
        }
        if (!TryGetComponent(out jumpStateController))
        {
            Debug.LogError("JumpMovementController: JumpStateController not found!", this);
            enabled = false;
            return;
        }
        if (!TryGetComponent(out rb))
        {
            Debug.LogError("JumpMovementController: Rigidbody2D not found!", this);
            enabled = false;
            return;
        }
        if (movementSettings == null)
        {
            Debug.LogError("JumpMovementController: MovementSettings not assigned!", this);
            enabled = false;
            return;
        }
        Debug.Log("JumpMovementController: Initialized", this);
    }

    public void Initialize()
    {
        UpdateMaxSpeed();
        Debug.Log($"JumpMovementController: Initialized with maxSpeed={maxSpeed}", this);
    }

    private void UpdateMaxSpeed()
    {
        if (TryGetComponent(out PlayerStateManager stateManager) && stateManager.IsArmed)
        {
            maxSpeed = movementSettings.ArmedMaxSpeed;
        }
        else
        {
            maxSpeed = movementSettings.StandardMaxSpeed;
        }
        Debug.Log($"JumpMovementController: Updated maxSpeed={maxSpeed}, IsArmed={(stateManager != null ? stateManager.IsArmed : false)}", this);
    }

    public void Move(Vector2 input)
    {
        moveInput = input;
        var currentPhase = jumpStateController.CurrentPhase();

        if (jumpStateController.IsLocked)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            Debug.Log($"JumpMovementController: Movement locked due to phase {currentPhase}, velocity={rb.linearVelocity}", this);
            return;
        }

        UpdateMaxSpeed();
        if (currentPhase == JumpPhase.JumpAir)
        {
            float targetSpeed = input.x * maxSpeed * airControlFactor;
            rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
            Debug.Log($"JumpMovementController: Moving in air, input={input}, phase={currentPhase}, velocity={rb.linearVelocity}, airControl={airControlFactor}, maxSpeed={maxSpeed}", this);
        }
        else
        {
            playerMovement.Move(input);
            Debug.Log($"JumpMovementController: Delegating to PlayerMovement, input={input}, phase={currentPhase}, velocity={rb.linearVelocity}", this);
        }
    }

    public void SetSpeed(float newSpeed)
    {
        maxSpeed = newSpeed;
        playerMovement.SetSpeed(newSpeed);
        Debug.Log($"JumpMovementController: Speed set to {newSpeed}", this);
    }
}