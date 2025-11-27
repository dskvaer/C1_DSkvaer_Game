using UnityEngine;
using Spine.Unity;
using Spine;
using NPC.Characters.Player;

public class PlayerHit : MonoBehaviour, IHit, IAnimation {
    private SpineAnimationController spineController;
    private AnimationStateManager animationStateManager;
    private SkeletonAnimation skeletonAnimation;
    private bool isHitAnimationActive;

    public bool IsHitAnimationActive => isHitAnimationActive;
    public event System.Action OnHitComplete;

    private void Awake()
    {
        spineController = GetComponent<SpineAnimationController>();
        if (spineController == null)
        {
            Debug.LogError("PlayerHit: SpineAnimationController not found!", this);
            enabled = false;
            return;
        }

        skeletonAnimation = GetComponent<SkeletonAnimation>();
        if (skeletonAnimation == null)
        {
            Debug.LogError("PlayerHit: SkeletonAnimation not found!", this);
            enabled = false;
            return;
        }

        animationStateManager = GetComponent<AnimationStateManager>();
        if (animationStateManager == null)
        {
            Debug.LogError("PlayerHit: AnimationStateManager not found!", this);
            enabled = false;
            return;
        }

        var hitAnim = animationStateManager.GetAnimation(CharacterAnimationType.Hit);
        if (!hitAnim.IsValid())
        {
            Debug.LogError($"PlayerHit: Hit animation invalid! AnimationName: {(hitAnim.Animation ?? "null")}", this);
            enabled = false;
            return;
        }

        skeletonAnimation.AnimationState.Complete += OnAnimationComplete;
        Debug.Log("PlayerHit: Initialized successfully", this);
    }

    private void OnDestroy()
    {
        if (skeletonAnimation != null)
        {
            skeletonAnimation.AnimationState.Complete -= OnAnimationComplete;
        }
    }

    public void Play()
    {
        Hit();
    }

    public void Hit()
    {
        if (!enabled)
        {
            Debug.LogError("PlayerHit: Component disabled, cannot play!", this);
            return;
        }

        var hitAnim = animationStateManager.GetAnimation(CharacterAnimationType.Hit);
        if (!hitAnim.IsValid() || string.IsNullOrEmpty(hitAnim.Animation))
        {
            Debug.LogError($"PlayerHit: Hit animation invalid. AnimationName: {(hitAnim.Animation ?? "null")}", this);
            return;
        }

        if (spineController.CurrentAnimation != hitAnim.Animation)
        {
            isHitAnimationActive = true;
            spineController.PlayAnimation(hitAnim.Animation, hitAnim.Loop, hitAnim.Speed);
            Debug.Log($"PlayerHit: Played Hit animation: {hitAnim.Animation}, Loop={hitAnim.Loop}, Speed={hitAnim.Speed}", this);
        }
    }

    private void OnAnimationComplete(TrackEntry trackEntry)
    {
        if (trackEntry == null || trackEntry.Animation == null)
        {
            Debug.LogError("PlayerHit: Invalid trackEntry in OnAnimationComplete!", this);
            return;
        }

        var hitAnim = animationStateManager.GetAnimation(CharacterAnimationType.Hit);
        if (hitAnim.IsValid() && trackEntry.Animation.Name == hitAnim.Animation)
        {
            isHitAnimationActive = false;
            OnHitComplete?.Invoke();
            Debug.Log("PlayerHit: Hit animation completed, invoking OnHitComplete", this);
        }
    }
}