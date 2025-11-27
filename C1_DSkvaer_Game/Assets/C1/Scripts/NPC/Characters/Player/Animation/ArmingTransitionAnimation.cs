using UnityEngine;
using Spine.Unity;
using Spine;
using System;
using Digimight.Extensions;

namespace NPC.Characters.Player {
    [RequireComponent(typeof(SpineAnimationController), typeof(SkeletonAnimation), typeof(AnimationStateManager))]
    public class ArmingTransitionAnimation : MonoBehaviour, IAnimation, IArmAnimation {
        private SpineAnimationController spineController;
        private SkeletonAnimation skeletonAnimation;
        private AnimationStateManager animationStateManager;
        private string currentAnimation;
        private Action<bool> _onTransitionComplete;

        public Action<bool> OnTransitionComplete
        {
            get => _onTransitionComplete;
            set => _onTransitionComplete = value;
        }

        private void Awake()
        {
            spineController = GetComponent<SpineAnimationController>();
            skeletonAnimation = GetComponent<SkeletonAnimation>();
            animationStateManager = GetComponent<AnimationStateManager>();

            if (spineController == null || skeletonAnimation == null || animationStateManager == null)
            {
                Debug.LogError("ArmingTransitionAnimation: SpineAnimationController, SkeletonAnimation or AnimationStateManager not found!", this);
                enabled = false;
                return;
            }

            var config = animationStateManager.GetCurrentStateConfig();
            if (config == null)
            {
                Debug.LogError("ArmingTransitionAnimation: Current animation config not found in AnimationStateManager!", this);
                enabled = false;
                return;
            }

            var armedTransition = animationStateManager.GetAnimation(CharacterAnimationType.ArmedTransition);
            if (!armedTransition.IsValid())
            {
                Debug.LogWarning("ArmingTransitionAnimation: ArmedTransition animation not found, will skip to Idle!", this);
            }

            skeletonAnimation.AnimationState.Complete += OnAnimationComplete;
            Debug.Log($"ArmingTransitionAnimation: Initialized, ArmedTransition={(armedTransition.IsValid() ? armedTransition.Animation : "null")}", this);
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
            PlayArmedTransition();
        }

        public void PlayArmedTransition()
        {
            if (!enabled || spineController == null || animationStateManager == null)
            {
                Debug.LogError("ArmingTransitionAnimation: Component disabled or SpineAnimationController/AnimationStateManager not initialized!", this);
                _onTransitionComplete?.Invoke(true);
                PlayIdle();
                return;
            }

            var animConfig = animationStateManager.GetAnimation(CharacterAnimationType.ArmedTransition);
            if (animConfig.IsValid() && !string.IsNullOrEmpty(animConfig.Animation))
            {
                if (spineController.CurrentAnimation != animConfig.Animation)
                {
                    spineController.PlayAnimation(animConfig.Animation, animConfig.Loop, animConfig.Speed, 0, () =>
                    {
                        //_onTransitionComplete?.Invoke(true);
                        //PlayIdle();
                    });
                    currentAnimation = animConfig.Animation;
                    Debug.Log($"ArmingTransitionAnimation: Played ArmedTransition animation: {animConfig.Animation}, Loop={animConfig.Loop}, Speed={animConfig.Speed}", this);
                }
            }
            else
            {
                Debug.LogWarning($"ArmingTransitionAnimation: Cannot play ArmedTransition, animConfig.IsValid={animConfig.IsValid()}, Animation={(animConfig.Animation ?? "null")}. Skipping to Idle.", this);
                _onTransitionComplete?.Invoke(true);
                PlayIdle();
            }
        }

        public void PlayDisarmedTransition()
        {
            Debug.LogError("ArmingTransitionAnimation: PlayDisarmedTransition not supported, use DisarmingTransitionAnimation!", this);
        }

        private void OnAnimationComplete(TrackEntry trackEntry)
        {
            if (trackEntry.Loop) return;

            if (spineController == null || animationStateManager == null)
            {
                Debug.LogError("ArmingTransitionAnimation: SpineAnimationController or AnimationStateManager not initialized!", this);
                _onTransitionComplete?.Invoke(true);
                PlayIdle();
                return;
            }

            if (trackEntry == null || trackEntry.Animation == null)
            {
                Debug.LogError($"ArmingTransitionAnimation: Invalid trackEntry! trackEntry={(trackEntry == null ? "null" : "exists")}, trackEntry.Animation={(trackEntry?.Animation == null ? "null" : "exists")}", this);
                _onTransitionComplete?.Invoke(true);
                PlayIdle();
                return;
            }

            var armedAnim = animationStateManager.GetAnimation(CharacterAnimationType.ArmedTransition);
            string animationName = trackEntry.Animation.Name ?? "null";

            if (armedAnim == null && trackEntry.Animation.Name != "Jump/Attack_Jump2")
            {
                Debug.LogWarning("ArmingTransitionAnimation: ArmedTransition animation config not found in AnimationStateManager!", this);
                _onTransitionComplete?.Invoke(true);
                PlayIdle();
                return;
            }

            if (armedAnim == null) return;

            Debug.Log($"ArmingTransitionAnimation: OnAnimationComplete, animationName={animationName}, armedAnim={(armedAnim.IsValid() ? armedAnim.Animation : "null")}", this);

            if (armedAnim.IsValid() && animationName == armedAnim.Animation)
            {
                Debug.Log("ArmingTransitionAnimation: ArmedTransition completed, transitioning to Idle", this);
                _onTransitionComplete?.Invoke(true);
                PlayIdle();
            }

        }

        private void PlayIdle()
        {
            if (!TryGetComponent<IdleAnimation>(out var idleComponent))
            {
                Debug.LogError("ArmingTransitionAnimation: IdleAnimation component not found!", this);
                return;
            }

            var idleAnim = animationStateManager.GetAnimation(CharacterAnimationType.Idle);
            if (!idleAnim.IsValid() || string.IsNullOrEmpty(idleAnim.Animation))
            {
                Debug.LogError($"ArmingTransitionAnimation: Cannot play Idle, animation invalid: {(idleAnim.Animation ?? "null")}", this);
                return;
            }

            currentAnimation = idleAnim.Animation;
            idleComponent.Play();
            Debug.Log($"ArmingTransitionAnimation: Played Idle animation, currentAnimation={currentAnimation}", this);
        }
    }
}