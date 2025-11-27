using NPC.Characters.Player;
using Spine.Unity;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement), typeof(PlayerJump), typeof(JumpStateController))]
[RequireComponent(typeof(PlayerAttack), typeof(PlayerAttackMovement), typeof(PlayerPush))]
[RequireComponent(typeof(PlayerPushAnimation), typeof(ArmingInputHandler), typeof(DisarmingInputHandler))]
[RequireComponent(typeof(PlayerStateManager), typeof(JumpMovementController), typeof(AttackStateController))]
[RequireComponent(typeof(PlayerHit), typeof(PlayerCollisionHandler))]
public class PlayerController : MonoBehaviour {
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerJump playerJump;
    [SerializeField] private JumpStateController jumpStateController;
    [SerializeField] private ArmingInputHandler armingInputHandler;
    [SerializeField] private DisarmingInputHandler disarmingInputHandler;
    [SerializeField] private PlayerStateManager playerStateManager;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private PlayerAttackMovement playerAttackMovement;
    [SerializeField] private PlayerPush playerPush;
    [SerializeField] private PlayerPushAnimation playerPushAnimation;
    [SerializeField] private JumpMovementController jumpMovementController;
    [SerializeField] private AttackStateController attackStateController;
    [SerializeField] private PlayerHit playerHit;
    [SerializeField] private PlayerCollisionHandler collisionHandler;
    [SerializeField] private Transform attackZone;
    [SerializeField] private SkeletonAnimation skeletonAnimation;

    private InputSystem_Actions _inputSystem;

    public InputSystem_Actions InputSystem => _inputSystem;
    public Transform AttackZone => attackZone != null ? attackZone : DummyTransform.Instance;
    public SkeletonAnimation SkeletonAnimation => skeletonAnimation != null ? skeletonAnimation : DummySkeletonAnimation.Instance;
    public IMovable Movement => jumpMovementController != null ? jumpMovementController : DummyMovement.Instance;
    public IJump Jump => playerJump != null ? playerJump : DummyJump.Instance;
    public IJumpState JumpState => jumpStateController != null ? jumpStateController : DummyJumpState.Instance;
    public IArmStateManager ArmState => playerStateManager != null ? playerStateManager : DummyArmStateManager.Instance;
    public IPush Push => playerPush != null ? playerPush : DummyPush.Instance;

    private void Awake()
    {
        _inputSystem = new();

        if (!playerMovement)
        {
            playerMovement = GetComponent<PlayerMovement>();
            if (!playerMovement)
            {
                Debug.LogError("PlayerController: PlayerMovement not found!", this);
                enabled = false;
                return;
            }
        }

        if (!playerJump)
        {
            playerJump = GetComponent<PlayerJump>();
            if (!playerJump)
            {
                Debug.LogError("PlayerController: PlayerJump not found!", this);
                enabled = false;
                return;
            }
        }

        if (!jumpStateController)
        {
            jumpStateController = GetComponent<JumpStateController>();
            if (!jumpStateController)
            {
                Debug.LogError("PlayerController: JumpStateController not found!", this);
                enabled = false;
                return;
            }
        }

        if (!armingInputHandler)
        {
            armingInputHandler = GetComponent<ArmingInputHandler>();
            if (!armingInputHandler)
            {
                Debug.LogError("PlayerController: ArmingInputHandler not found!", this);
                enabled = false;
                return;
            }
        }

        if (!disarmingInputHandler)
        {
            disarmingInputHandler = GetComponent<DisarmingInputHandler>();
            if (!disarmingInputHandler)
            {
                Debug.LogError("PlayerController: DisarmingInputHandler not found!", this);
                enabled = false;
                return;
            }
        }

        if (!playerStateManager)
        {
            playerStateManager = GetComponent<PlayerStateManager>();
            if (!playerStateManager)
            {
                Debug.LogError("PlayerController: PlayerStateManager not found!", this);
                enabled = false;
                return;
            }
        }

        if (!playerAttack)
        {
            playerAttack = GetComponent<PlayerAttack>();
            if (!playerAttack)
            {
                Debug.LogError("PlayerController: PlayerAttack not found!", this);
                enabled = false;
                return;
            }
        }

        if (!playerAttackMovement)
        {
            playerAttackMovement = GetComponent<PlayerAttackMovement>();
            if (!playerAttackMovement)
            {
                Debug.LogError("PlayerController: PlayerAttackMovement not found!", this);
                enabled = false;
                return;
            }
        }

        if (!playerPush)
        {
            playerPush = GetComponent<PlayerPush>();
            if (!playerPush)
            {
                Debug.LogError("PlayerController: PlayerPush not found!", this);
                enabled = false;
                return;
            }
        }

        if (!playerPushAnimation)
        {
            playerPushAnimation = GetComponent<PlayerPushAnimation>();
            if (!playerPushAnimation)
            {
                Debug.LogError("PlayerController: PlayerPushAnimation not found!", this);
                enabled = false;
                return;
            }
        }

        if (!jumpMovementController)
        {
            jumpMovementController = GetComponent<JumpMovementController>();
            if (!jumpMovementController)
            {
                Debug.LogError("PlayerController: JumpMovementController not found!", this);
                enabled = false;
                return;
            }
        }

        if (!attackStateController)
        {
            attackStateController = GetComponent<AttackStateController>();
            if (!attackStateController)
            {
                Debug.LogError("PlayerController: AttackStateController not found!", this);
                enabled = false;
                return;
            }
        }

        if (!playerHit)
        {
            playerHit = GetComponent<PlayerHit>();
            if (!playerHit)
            {
                Debug.LogError("PlayerController: PlayerHit not found!", this);
                enabled = false;
                return;
            }
        }

        if (!collisionHandler)
        {
            collisionHandler = GetComponent<PlayerCollisionHandler>();
            if (!collisionHandler)
            {
                Debug.LogError("PlayerController: PlayerCollisionHandler not found!", this);
                enabled = false;
                return;
            }
        }

        if (!attackZone || !skeletonAnimation || !skeletonAnimation.SkeletonDataAsset)
        {
            Debug.LogError($"PlayerController: Missing required components (AttackZone: {attackZone}, SkeletonAnimation: {skeletonAnimation}, SkeletonDataAsset: {(skeletonAnimation ? skeletonAnimation.SkeletonDataAsset : "null")})!", this);
            enabled = false;
            return;
        }

        playerMovement.Initialize();
        playerJump.Initialize(10f, 0.5f, 0.2f);
        jumpStateController.Initialize(10f, 0.5f, 0.2f);
        playerPush.Initialize(skeletonAnimation);
        playerPushAnimation.Initialize(skeletonAnimation);
        armingInputHandler.Initialize(playerStateManager);
        disarmingInputHandler.Initialize(playerStateManager);
        jumpMovementController.Initialize();
        collisionHandler.OnCollisionHit += playerHit.Hit;
        Debug.Log("PlayerController: Initialized", this);
    }

    private void OnEnable()
    {
        _inputSystem.Enable();
        _inputSystem.Player.Arm.performed += context => ArmHandler();
        _inputSystem.Player.Attack.performed += context => playerAttack.Attack(context);
        _inputSystem.Player.Jump.performed += context => playerJump.Jump();
        _inputSystem.Player.TestDamage.performed += context => TestDamageHandler();
        playerPush.EnableInput();
        playerPushAnimation.EnableInput();
    }

    private void OnDisable()
    {
        _inputSystem.Disable();
        playerPush.DisableInput();
        playerPushAnimation.DisableInput();
        if (collisionHandler != null)
        {
            collisionHandler.OnCollisionHit -= playerHit.Hit;
        }
    }

    private void ArmHandler()
    {
        if (playerStateManager.IsArmed)
        {
            disarmingInputHandler.ArmHandler();
            Debug.Log("PlayerController: Triggering DisarmingInputHandler", this);
        }
        else
        {
            armingInputHandler.ArmHandler();
            Debug.Log("PlayerController: Triggering ArmingInputHandler", this);
        }
    }

    private void TestDamageHandler()
    {
        if (playerHit != null)
        {
            playerHit.Hit();
            Debug.Log("PlayerController: TestDamage triggered, calling PlayerHit.Hit()", this);
        }
        else
        {
            Debug.LogWarning("PlayerController: PlayerHit component not found, cannot trigger Hit!", this);
        }
    }

    private void Update()
    {
        if (playerAttack)
        {
            playerAttack.UpdateAttack();
        }
        if (!playerPush.IsPushing)
        {
            jumpMovementController.Move(_inputSystem.Player.Move.ReadValue<Vector2>());
        }
    }
}

// Null Object implementations
public class DummyMovement : IMovable {
    public static readonly DummyMovement Instance = new();
    public Vector2 MoveInput => Vector2.zero;
    public Rigidbody2D Rigidbody => null;
    public void Move(Vector2 input) { }
    public void SetSpeed(float speed) { }
}

public class DummyJump : IJump {
    public static readonly DummyJump Instance = new();
    public bool IsGrounded() => true;
    public bool IsJumping() => false;
    public void Initialize(float jumpForce, float coyoteTime, float jumpBufferTime) { }
    public void Jump() { }
}

public class DummyJumpState : IJumpState {
    public static readonly DummyJumpState Instance = new();
    public JumpPhase CurrentPhase() => JumpPhase.None;
    public bool IsLocked => false;
    public bool CanJump() => false;
    public bool IsGrounded() => true;
    public bool IsJumping() => false;
    public bool WasInAir() => false;
    public bool CheckGround(Vector2 _position, float _distance, LayerMask _groundLayer, LayerMask _platformLayer) => false;
    public void SetCanJump(bool canJump) { }
    public void SetCurrentPhase(JumpPhase phase) { }
    public void SetFailedJump(bool failedJump) { }
    public void SetGrounded(bool grounded) { }
    public void SetJumping(bool jumping) { }
    public void SetPreJumpVelocity(float velocity) { }
    public void SetWasInAir(bool wasInAir) { }
}

public class DummyArmStateManager : IArmStateManager {
    public static readonly DummyArmStateManager Instance = new();
    public bool IsTransitioning => false;
    public bool IsArmed => false;
    public void TriggerArmTransition(bool arm) { }
}

public class DummyPush : IPush {
    public static readonly DummyPush Instance = new();
    public bool IsPushing => false;
    public bool IsInPushContact => false;
    public bool IsInteractHeld => false;
    public void Initialize(SkeletonAnimation skeletonAnimation) { }
    public void UpdatePush() { }
    public void EnableInput() { }
    public void DisableInput() { }
}

public class DummyTransform : MonoBehaviour {
    public static readonly Transform Instance = new GameObject("DummyTransform").transform;
}

public class DummySkeletonAnimation : MonoBehaviour {
    public static readonly SkeletonAnimation Instance = new GameObject("DummySkeletonAnimation").AddComponent<SkeletonAnimation>();
}