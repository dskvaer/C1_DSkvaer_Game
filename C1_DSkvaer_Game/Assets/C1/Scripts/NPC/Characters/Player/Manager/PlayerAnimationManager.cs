using NPC.Characters.Player;
using Spine.Unity;
using UnityEngine;

[RequireComponent(typeof(PlayerController), typeof(SpineAnimationController))]
[RequireComponent(typeof(AnimationStateManager), typeof(MovementAnimationHandler))]
public class PlayerAnimationManager : MonoBehaviour {
    [SerializeField] private PlayerController playerController;
    [SerializeField] private SpineAnimationController animationController;
    [SerializeField] private AnimationStateManager animationStateManager;
    [SerializeField] private MovementAnimationHandler movementAnimationHandler;
    private bool isRunning;
    private float lastMoveInputX = float.MaxValue;
    private bool lastIsGrounded;
    private JumpPhase lastJumpPhase;
    private bool lastIsTransitioning;
    private bool lastIsHitActive;
    private bool lastIsPushContact;
    private bool lastIsInteractHeld;

    private void Awake()
    {
        if (!TryGetComponent(out playerController))
        {
            Debug.LogError("PlayerAnimationManager: PlayerController not found!", this);
            enabled = false;
            return;
        }
        if (!TryGetComponent(out animationController))
        {
            Debug.LogError("PlayerAnimationManager: SpineAnimationController not found!", this);
            enabled = false;
            return;
        }
        if (!TryGetComponent(out animationStateManager))
        {
            Debug.LogError("PlayerAnimationManager: AnimationStateManager not found!", this);
            enabled = false;
            return;
        }
        if (!TryGetComponent(out movementAnimationHandler))
        {
            Debug.LogError("PlayerAnimationManager: MovementAnimationHandler not found!", this);
            enabled = false;
            return;
        }
        if (!TryGetComponent(out SkeletonAnimation skeletonAnimation))
        {
            Debug.LogError("PlayerAnimationManager: SkeletonAnimation not found!", this);
            enabled = false;
            return;
        }

        InitializeAnimationComponents();
        Debug.Log("PlayerAnimationManager: Initialized", this);
    }

    private void InitializeAnimationComponents()
    {
        if (!TryGetComponent(out JumpAnimation _))
        {
            gameObject.AddComponent<JumpAnimation>();
            Debug.Log("PlayerAnimationManager: Added JumpAnimation component", this);
        }

        if (!TryGetComponent(out PlayerPushAnimation pushAnim))
        {
            gameObject.AddComponent<PlayerPushAnimation>();
            Debug.Log("PlayerAnimationManager: Added PlayerPushAnimation component", this);
        }
        else
        {
            pushAnim.Initialize(GetComponent<SkeletonAnimation>());
        }

        if (!TryGetComponent(out PlayerHit _))
        {
            gameObject.AddComponent<PlayerHit>();
            Debug.Log("PlayerAnimationManager: Added PlayerHit component", this);
        }
    }

    private void OnEnable()
    {
        if (animationController != null)
        {
            animationController.OnJumpEndComplete += OnJumpEndCompleteHandler;
        }
    }

    private void OnDisable()
    {
        if (animationController != null)
        {
            animationController.OnJumpEndComplete -= OnJumpEndCompleteHandler;
        }
    }

    private void Start()
    {
        var idleAnim = animationStateManager.GetAnimation(CharacterAnimationType.Idle);
        if (!idleAnim.IsValid())
        {
            var stateConfig = animationStateManager.GetCurrentStateConfig();
            Debug.LogWarning($"PlayerAnimationManager: Idle animation invalid! State: {(stateConfig != null ? stateConfig.StateName : "null")}", this);
            return;
        }
        movementAnimationHandler.PlayIdle();
        lastIsGrounded = playerController.Jump.IsGrounded();
        HandleAnimationPriority();
    }

    private void Update()
    {
        if (!enabled || playerController == null || animationController == null)
        {
            Debug.LogWarning("PlayerAnimationManager: Skipping Update due to missing main components", this);
            return;
        }

        if (playerController.Movement == null || playerController.Jump == null || playerController.JumpState == null)
        {
            Debug.LogWarning("PlayerAnimationManager: Skipping Update due to uninitialized PlayerController subcomponents", this);
            return;
        }

        bool isTransitioning = playerController.ArmState != null && playerController.ArmState.IsTransitioning;
        Vector2 moveInput = playerController.Movement.MoveInput;
        bool isGrounded = playerController.Jump.IsGrounded();
        JumpPhase currentPhase = playerController.JumpState != null ? playerController.JumpState.CurrentPhase() : JumpPhase.None;
        bool isHitActive = TryGetComponent(out PlayerHit hitAnim) && hitAnim.IsHitAnimationActive;
        bool isPushContact = playerController.Push is PlayerPush pushComponent && pushComponent.IsInitialized && pushComponent.IsInPushContact;
        bool isInteractHeld = playerController.Push is PlayerPush push && push.IsInitialized && push.IsInteractHeld;

        bool stateChanged = moveInput.x != lastMoveInputX ||
                            isGrounded != lastIsGrounded ||
                            currentPhase != lastJumpPhase ||
                            isTransitioning != lastIsTransitioning ||
                            isHitActive != lastIsHitActive ||
                            isPushContact != lastIsPushContact ||
                            isInteractHeld != lastIsInteractHeld;

        if (stateChanged)
        {
            if (Mathf.Abs(moveInput.x) > 0.1f && moveInput.x != lastMoveInputX)
            {
                animationController.FlipSkeleton(moveInput.x);
                Debug.Log($"PlayerAnimationManager: Flipped skeleton, direction={moveInput.x}", this);
            }

            HandleAnimationPriority();

            lastMoveInputX = moveInput.x;
            lastIsGrounded = isGrounded;
            lastJumpPhase = currentPhase;
            lastIsTransitioning = isTransitioning;
            lastIsHitActive = isHitActive;
            lastIsPushContact = isPushContact;
            lastIsInteractHeld = isInteractHeld;
        }
    }

    private void HandleAnimationPriority()
    {
        if (playerController == null || playerController.Movement == null || playerController.Jump == null) return;
        if (animationStateManager == null || movementAnimationHandler == null) return;

        if (!TryGetComponent(out SkeletonAnimation skeletonAnimation) || skeletonAnimation.Skeleton == null)
        {
            Debug.LogError("PlayerAnimationManager: SkeletonAnimation or Skeleton is null!", this);
            enabled = false;
            return;
        }

        float skeletonScaleX = skeletonAnimation.Skeleton.ScaleX;
        Vector2 moveInput = playerController.Movement.MoveInput;
        bool isGrounded = playerController.Jump.IsGrounded();
        JumpPhase currentPhase = playerController.JumpState != null ? playerController.JumpState.CurrentPhase() : JumpPhase.None;

        // Priority 1: Hit
        if (TryGetComponent(out PlayerHit hitAnim) && hitAnim.IsHitAnimationActive) return;

        // Priority 2: Push
        if (playerController.Push is PlayerPush pushComponent && pushComponent.IsInitialized)
        {
            pushComponent.UpdatePush();
            if (pushComponent.IsInPushContact && TryGetComponent(out PlayerPushAnimation pushAnim))
            {
                bool isInteractHeld = pushComponent.IsInteractHeld;
                bool isMovingInDirection = Mathf.Abs(moveInput.x) > 0.1f && Mathf.Sign(moveInput.x) == skeletonScaleX;

                if (isMovingInDirection)
                {
                    pushAnim.Play(loop: isInteractHeld);
                    return;
                }
                else if (moveInput.x == 0f)
                {
                    movementAnimationHandler.PlayIdle();
                    return;
                }
                else
                {
                    movementAnimationHandler.PlayRun();
                    return;
                }
            }
        }

        // Priority 3: Jump
        if (TryGetComponent(out JumpAnimation jumpAnim))
        {
            if (currentPhase != lastJumpPhase)
            {
                switch (currentPhase)
                {
                    case JumpPhase.JumpStart: jumpAnim.PlayJumpStart(); break;
                    case JumpPhase.JumpAir:
                        if (lastJumpPhase != JumpPhase.JumpStart) jumpAnim.PlayJumpAir();
                        break;
                    case JumpPhase.JumpLand: jumpAnim.PlayJumpLand(); break;
                }
                lastJumpPhase = currentPhase;
            }

            if (playerController.JumpState != null && (jumpAnim.IsJumpAnimationActive || playerController.JumpState.IsLocked)) return;
        }

        // Priority 4: Run
        if (isGrounded && Mathf.Abs(moveInput.x) > 0.1f && !animationController.IsJumpStartOrEnd)
        {
            movementAnimationHandler.PlayRun();
            isRunning = true;
            return;
        }

        // Priority 5: EdgeBalance
        if (TryGetComponent(out IEdgeBalanceAnimation edgeBalanceAnim) && isGrounded && edgeBalanceAnim.IsAtEdge() && !edgeBalanceAnim.IsOtherAnimationActive())
        {
            edgeBalanceAnim.Play();
            return;
        }

        // Приоритет 6: Transition
        // Безопасная проверка ArmState
        bool isTransitioning = false;
        if (playerController != null && playerController.ArmState != null)
        {
            try
            {
                isTransitioning = playerController.ArmState.IsTransitioning;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"PlayerAnimationManager: Error accessing ArmState.IsTransitioning: {ex.Message}", this);
                isTransitioning = false;
            }
        }

        if (isTransitioning)
        {
            // Добавляем проверку на null для animationStateManager
            if (animationStateManager == null)
            {
                Debug.LogError("PlayerAnimationManager: animationStateManager is null during transition!", this);
                if (movementAnimationHandler != null)
                {
                    movementAnimationHandler.PlayIdle();
                }
                isRunning = false;
                return;
            }

            // Пробуем безопасно получить анимации перехода
            bool hasArmedTransition = false;
            bool hasDisarmedTransition = false;
            string transitionAnimName = null;

            try
            {
                // Проверяем наличие анимаций через безопасный вызов
                var currentState = animationStateManager.CurrentState;
                Debug.Log($"PlayerAnimationManager: Checking transitions, CurrentState={currentState}", this);

                // Получаем анимации только если менеджер инициализирован
                if (animationStateManager.GetCurrentStateConfig() != null)
                {
                    try
                    {
                        var armedAnim = animationStateManager.GetAnimation(CharacterAnimationType.ArmedTransition);
                        hasArmedTransition = !string.IsNullOrEmpty(armedAnim.Animation);
                        if (hasArmedTransition) transitionAnimName = armedAnim.Animation;
                    }
                    catch
                    {
                        hasArmedTransition = false;
                    }

                    try
                    {
                        var disarmedAnim = animationStateManager.GetAnimation(CharacterAnimationType.DisarmedTransition);
                        hasDisarmedTransition = !string.IsNullOrEmpty(disarmedAnim.Animation);
                        if (hasDisarmedTransition && transitionAnimName == null) transitionAnimName = disarmedAnim.Animation;
                    }
                    catch
                    {
                        hasDisarmedTransition = false;
                    }
                }

                if (!hasArmedTransition && !hasDisarmedTransition)
                {
                    Debug.LogWarning($"PlayerAnimationManager: No transition animations found, playing Idle", this);
                    if (movementAnimationHandler != null)
                    {
                        movementAnimationHandler.PlayIdle();
                    }
                    isRunning = false;
                    return;
                }
                else
                {
                    Debug.Log($"PlayerAnimationManager: Transition active with animation: {transitionAnimName}", this);
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"PlayerAnimationManager: Exception in transition block: {ex.Message}", this);
                if (movementAnimationHandler != null)
                {
                    movementAnimationHandler.PlayIdle();
                }
                isRunning = false;
                return;
            }
        }

        // Priority 7: Idle
        if (isGrounded && Mathf.Abs(moveInput.x) <= 0.1f && !animationController.IsJumpStartOrEnd)
        {
            movementAnimationHandler.PlayIdle();
            isRunning = false;
            return;
        }
    }

    private void OnJumpEndCompleteHandler()
    {
        if (playerController == null || playerController.Jump == null || playerController.JumpState == null) return;

        lastJumpPhase = JumpPhase.None;
        HandleAnimationPriority();
    }
}
