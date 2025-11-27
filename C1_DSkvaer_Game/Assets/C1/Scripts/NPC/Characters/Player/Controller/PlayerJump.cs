using UnityEngine;
using UnityEngine.InputSystem;
using NPC.Characters.Player;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInput), typeof(JumpStateController))]
[RequireComponent(typeof(PlayerStateManager), typeof(PlayerAttack), typeof(PlayerPush))]
public class PlayerJump : MonoBehaviour, IJump {
    [SerializeField] private JumpConfig jumpConfig;
    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private JumpStateController jumpStateController;
    private PlayerStateManager playerStateManager;
    private PlayerAttack playerAttack;
    private PlayerPush playerPush;

    public JumpPhase CurrentPhase => jumpStateController != null ? jumpStateController.CurrentPhase() : JumpPhase.None;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        if (!TryGetComponent<Rigidbody2D>(out rb))
        {
            Debug.LogError("PlayerJump: Rigidbody2D not found!", this);
            enabled = false;
            return;
        }
        if (!TryGetComponent<PlayerInput>(out playerInput))
        {
            Debug.LogError("PlayerJump: PlayerInput not found!", this);
            enabled = false;
            return;
        }
        if (!TryGetComponent<JumpStateController>(out jumpStateController))
        {
            Debug.LogError("PlayerJump: JumpStateController not found!", this);
            enabled = false;
            return;
        }
        if (!TryGetComponent<PlayerStateManager>(out playerStateManager))
        {
            Debug.LogError("PlayerJump: PlayerStateManager not found!", this);
            enabled = false;
            return;
        }
        if (!TryGetComponent<PlayerAttack>(out playerAttack))
        {
            Debug.LogError("PlayerJump: PlayerAttack not found!", this);
            enabled = false;
            return;
        }
        if (!TryGetComponent<PlayerPush>(out playerPush))
        {
            Debug.LogError("PlayerJump: PlayerPush not found!", this);
            enabled = false;
            return;
        }
        if (jumpConfig == null)
        {
            Debug.LogError("PlayerJump: JumpConfig not assigned!", this);
            enabled = false;
            return;
        }

        rb.gravityScale = 2f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.mass = 1f;
        rb.linearDamping = 0f;

        Subscribe();
        Debug.Log("PlayerJump: Initialized components", this);
    }

    public void Initialize(float jumpForce, float jumpDelay, float groundCheckDistance)
    {
        if (jumpConfig == null)
        {
            Debug.LogError("PlayerJump: JumpConfig not assigned!", this);
            return;
        }
        jumpConfig.JumpForce = jumpForce;
        jumpConfig.JumpDelay = jumpDelay;
        jumpConfig.GroundCheckDistance = groundCheckDistance;
        jumpConfig.GroundLayer = LayerMask.GetMask("Ground");
        jumpConfig.PushableLayer = LayerMask.GetMask("Pushable");
        jumpStateController.Initialize(jumpForce, jumpDelay, groundCheckDistance);
        Debug.Log($"PlayerJump: Initialized with JumpForce={jumpForce}, JumpDelay={jumpDelay}, GroundCheckDistance={groundCheckDistance}", this);
    }

    private void Subscribe()
    {
        if (playerInput == null || playerInput.actions == null)
        {
            Debug.LogError("PlayerJump: PlayerInput or actions are null!", this);
            return;
        }
        var jumpAction = playerInput.actions.FindAction("Jump");
        if (jumpAction == null)
        {
            Debug.LogError("PlayerJump: Jump action not found!", this);
            return;
        }
        jumpAction.performed += OnJumpPerformed;
    }

    private void Unsubscribe()
    {
        if (playerInput == null || playerInput.actions == null) return;
        var jumpAction = playerInput.actions.FindAction("Jump");
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJumpPerformed;
        }
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (CanPerformJump())
        {
            jumpStateController.SetPreJumpVelocity(rb.linearVelocity.x);
            rb.linearVelocity = Vector2.zero; // Обнуляем скорость
            jumpStateController.Jump();
            Debug.Log($"PlayerJump: Jump initiated: phase={jumpStateController.CurrentPhase()}, velocity={rb.linearVelocity}, grounded={jumpStateController.IsGrounded()}", this);
        }
        else
        {
            Debug.LogWarning($"PlayerJump: Jump rejected: grounded={jumpStateController.IsGrounded()}, jumping={jumpStateController.IsJumping()}, canJump={jumpStateController.CanJump()}, isAttacking={playerAttack.IsAttacking}, isPushing={playerPush.IsPushing}, isTransitioning={playerStateManager.IsTransitioning}", this);
        }
    }

    public void Jump()
    {
        if (CanPerformJump())
        {
            jumpStateController.SetPreJumpVelocity(rb.linearVelocity.x);
            rb.linearVelocity = Vector2.zero;
            jumpStateController.Jump();
            Debug.Log($"PlayerJump: External Jump initiated: phase={jumpStateController.CurrentPhase()}, velocity={rb.linearVelocity}, grounded={jumpStateController.IsGrounded()}", this);
        }
        else
        {
            Debug.LogWarning($"PlayerJump: External Jump rejected: grounded={jumpStateController.IsGrounded()}, jumping={jumpStateController.IsJumping()}, canJump={jumpStateController.CanJump()}, isAttacking={playerAttack.IsAttacking}, isPushing={playerPush.IsPushing}, isTransitioning={playerStateManager.IsTransitioning}", this);
        }
    }

    private bool CanPerformJump()
    {
        return jumpStateController != null &&
               jumpStateController.CanJump() &&
               !playerAttack.IsAttacking &&
               !playerPush.IsPushing &&
               !playerStateManager.IsTransitioning;
    }

    public bool IsGrounded()
    {
        return jumpStateController != null && jumpStateController.IsGrounded();
    }

    public bool IsJumping()
    {
        return jumpStateController != null && jumpStateController.IsJumping();
    }
}