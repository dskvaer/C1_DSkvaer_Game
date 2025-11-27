using UnityEngine;
using Spine.Unity;
using NPC.Characters.Player;

public class PlayerBlockAnimation : MonoBehaviour, IBlockAnimation {
    [SerializeField] private AnimationStateManager animationStateManager;
    private SkeletonAnimation skeletonAnimation;
    private bool isBlockAnimationActive;

    public bool IsBlockAnimationActive => isBlockAnimationActive;

    public void Initialize(SkeletonAnimation skeletonAnimation)
    {
        this.skeletonAnimation = skeletonAnimation;
        if (!skeletonAnimation || !skeletonAnimation.SkeletonDataAsset)
        {
            Debug.LogError("PlayerBlockAnimation: SkeletonAnimation or SkeletonDataAsset not assigned!", this);
            enabled = false;
            return;
        }

        if (!animationStateManager)
        {
            animationStateManager = GetComponent<AnimationStateManager>();
            if (!animationStateManager)
            {
                Debug.LogError("PlayerBlockAnimation: AnimationStateManager not found!", this);
                enabled = false;
                return;
            }
        }

        Debug.Log("PlayerBlockAnimation: Initialized", this);
    }

    public void PlayBlock(bool loop)
    {
        if (skeletonAnimation == null || !skeletonAnimation.SkeletonDataAsset)
        {
            Debug.LogWarning("PlayerBlockAnimation: SkeletonAnimation not initialized!", this);
            return;
        }

        var blockAnim = animationStateManager.GetAnimation(CharacterAnimationType.Block);
        if (!blockAnim.IsValid())
        {
            Debug.LogWarning("PlayerBlockAnimation: Block animation not found or invalid!", this);
            return;
        }

        skeletonAnimation.AnimationState.SetAnimation(0, blockAnim.Animation, loop);
        isBlockAnimationActive = true;
        Debug.Log($"PlayerBlockAnimation: Playing Block animation, loop={loop}", this);
    }

    public void StopBlock()
    {
        if (skeletonAnimation != null)
        {
            isBlockAnimationActive = false;
            skeletonAnimation.AnimationState.ClearTrack(0);
            Debug.Log("PlayerBlockAnimation: Block animation stopped", this);
        }
    }
}