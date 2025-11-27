using UnityEngine;
using Spine.Unity;
using Spine;
using NPC.Characters.Player;

public class JumpAnimation : MonoBehaviour, IJumpAnimation, IAnimation {
    private SpineAnimationController spineController;
    private AnimationStateManager animationStateManager;
    private SkeletonAnimation skeletonAnimation;
    private JumpStateController jumpStateController;
    private bool isJumpAnimationActive;
    private bool canPlayJumpAir = true;

    public bool IsJumpAnimationActive => isJumpAnimationActive;

    private void Awake()
    {
        spineController = GetComponent<SpineAnimationController>();
        if (spineController == null)
        {
            Debug.LogError("JumpAnimation: SpineAnimationController not found!", this);
            enabled = false;
            return;
        }

        skeletonAnimation = GetComponent<SkeletonAnimation>();
        if (skeletonAnimation == null)
        {
            Debug.LogError("JumpAnimation: SkeletonAnimation not found!", this);
            enabled = false;
            return;
        }

        animationStateManager = GetComponent<AnimationStateManager>();
        if (animationStateManager == null)
        {
            Debug.LogError("JumpAnimation: AnimationStateManager not found!", this);
            enabled = false;
            return;
        }

        jumpStateController = GetComponent<JumpStateController>();
        if (jumpStateController == null)
        {
            Debug.LogError("JumpAnimation: JumpStateController not found!", this);
            enabled = false;
            return;
        }

        var jumpStartAnim = animationStateManager.GetAnimation(CharacterAnimationType.Jump, 0);
        var jumpAirAnim = animationStateManager.GetAnimation(CharacterAnimationType.Jump, 1);
        var jumpLandAnim = animationStateManager.GetAnimation(CharacterAnimationType.Jump, 2);

        if (!jumpStartAnim.IsValid() || !jumpAirAnim.IsValid() || !jumpLandAnim.IsValid())
        {
            Debug.LogError($"JumpAnimation: Not all jump animations found! JumpStart={jumpStartAnim.IsValid()}, JumpAir={jumpAirAnim.IsValid()}, JumpLand={jumpLandAnim.IsValid()}", this);
            enabled = false;
            return;
        }

        jumpStateController.OnJumpStart += PlayJumpStart;
        jumpStateController.OnJumpLandComplete += OnJumpLandCompleteHandler;
        jumpStateController.OnJumpAir += PlayJumpAir;
        jumpStateController.OnResetJumpPhase += () => canPlayJumpAir = true;
        Debug.Log($"JumpAnimation: Initialized successfully, JumpStart={jumpStartAnim.Animation}, JumpAir={jumpAirAnim.Animation}, JumpLand={jumpLandAnim.Animation}", this);
    }

    private void OnDestroy()
    {
        if (jumpStateController != null)
        {
            jumpStateController.OnJumpStart -= PlayJumpStart;
            jumpStateController.OnJumpLandComplete -= OnJumpLandCompleteHandler;
            jumpStateController.OnJumpAir -= PlayJumpAir;
            jumpStateController.OnResetJumpPhase -= () => canPlayJumpAir = true;
        }
    }

    public void Play()
    {
        PlayJumpStart();
    }

    public void PlayJumpStart()
    {
        if (!enabled)
        {
            Debug.LogError("JumpAnimation: Component disabled, cannot play!", this);
            return;
        }

        var jumpStartAnim = animationStateManager.GetAnimation(CharacterAnimationType.Jump, 0);
        if (!jumpStartAnim.IsValid() || string.IsNullOrEmpty(jumpStartAnim.Animation))
        {
            Debug.LogError($"JumpAnimation: JumpStart animation invalid. AnimationName: {(jumpStartAnim.Animation ?? "null")}", this);
            return;
        }

        if (spineController.CurrentAnimation != jumpStartAnim.Animation)
        {
            isJumpAnimationActive = true;
            spineController.PlayAnimation(jumpStartAnim.Animation, jumpStartAnim.Loop, jumpStartAnim.Speed);
            Debug.Log($"JumpAnimation: Played JumpStart animation: {jumpStartAnim.Animation}, Loop={jumpStartAnim.Loop}, Speed={jumpStartAnim.Speed}", this);
        }
    }

    public void PlayJumpAir()
    {
        if (!enabled)
        {
            Debug.LogError("JumpAnimation: Component disabled, cannot play!", this);
            return;
        }

        if (!canPlayJumpAir)
        {
            Debug.Log($"JumpAnimation: PlayJumpAir blocked until JumpLand completes, canPlayJumpAir={canPlayJumpAir}", this);
            return;
        }

        var jumpAirAnim = animationStateManager.GetAnimation(CharacterAnimationType.Jump, 1);
        if (!jumpAirAnim.IsValid() || string.IsNullOrEmpty(jumpAirAnim.Animation))
        {
            Debug.LogError($"JumpAnimation: JumpAir animation invalid. AnimationName: {(jumpAirAnim.Animation ?? "null")}", this);
            return;
        }

        if (spineController.CurrentAnimation != jumpAirAnim.Animation)
        {
            isJumpAnimationActive = true;
            canPlayJumpAir = false; // Блокируем повторное воспроизведение
            spineController.PlayAnimation(jumpAirAnim.Animation, jumpAirAnim.Loop, jumpAirAnim.Speed);
            Debug.Log($"JumpAnimation: Played JumpAir animation: {jumpAirAnim.Animation}, Loop={jumpAirAnim.Loop}, Speed={jumpAirAnim.Speed}, canPlayJumpAir={canPlayJumpAir}", this);
        }
        else
        {
            Debug.Log($"JumpAnimation: JumpAir already playing, skipping: {jumpAirAnim.Animation}, Loop={jumpAirAnim.Loop}, Speed={jumpAirAnim.Speed}", this);
        }
    }

    public void PlayJumpLand()
    {
        if (!enabled)
        {
            Debug.LogError("JumpAnimation: Component disabled, cannot play!", this);
            return;
        }

        var jumpLandAnim = animationStateManager.GetAnimation(CharacterAnimationType.Jump, 2);
        if (!jumpLandAnim.IsValid() || string.IsNullOrEmpty(jumpLandAnim.Animation))
        {
            Debug.LogError($"JumpAnimation: JumpLand animation invalid. AnimationName: {(jumpLandAnim.Animation ?? "null")}", this);
            return;
        }

        if (spineController.CurrentAnimation != jumpLandAnim.Animation)
        {
            isJumpAnimationActive = true;
            spineController.PlayAnimation(jumpLandAnim.Animation, jumpLandAnim.Loop, jumpLandAnim.Speed);
            Debug.Log($"JumpAnimation: Played JumpLand animation: {jumpLandAnim.Animation}, Loop={jumpLandAnim.Loop}, Speed={jumpLandAnim.Speed}, canPlayJumpAir={canPlayJumpAir}", this);
        }
    }

    private void OnJumpLandCompleteHandler()
    {
        isJumpAnimationActive = false;
        if (spineController != null)
        {
            spineController.OnJumpEndComplete?.Invoke();
            Debug.Log("JumpAnimation: OnJumpLandCompleteHandler invoked", this);
        }
        else
        {
            Debug.LogWarning("JumpAnimation: SpineAnimationController is null, OnJumpEndComplete not invoked!", this);
        }
    }
}