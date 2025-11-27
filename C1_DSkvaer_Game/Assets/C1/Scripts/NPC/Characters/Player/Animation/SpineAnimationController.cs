using UnityEngine;
using Spine.Unity;
using System;

namespace NPC.Characters.Player {
    [RequireComponent(typeof(SkeletonAnimation))]
    public class SpineAnimationController : MonoBehaviour, IAnimationController {
        [SerializeField] private SkeletonAnimation skeletonAnimation;
        [SerializeField] private AnimationStateManager animationStateManager;

        private string _currentAnimation;
        private bool _isLooping;
        private float _currentSpeed;
        private Action _onComplete;
        private bool _isJumpStartOrEnd;

        public string CurrentAnimation => _currentAnimation;
        public bool IsLooping => _isLooping;
        public bool IsJumpStartOrEnd => _isJumpStartOrEnd;
        public Action OnJumpEndComplete { get; set; }
        public Action<int> OnAttackComplete { get; set; }

        private void Awake()
        {
            if (skeletonAnimation == null && !TryGetComponent(out skeletonAnimation))
            {
                Debug.LogError("SpineAnimationController: SkeletonAnimation not found!", this);
                enabled = false;
                return;
            }

            if (skeletonAnimation.SkeletonDataAsset == null)
            {
                Debug.LogError("SpineAnimationController: SkeletonDataAsset not assigned!", this);
                enabled = false;
                return;
            }

            if (!TryGetComponent(out animationStateManager))
            {
                Debug.LogError("SpineAnimationController: AnimationStateManager not found!", this);
                enabled = false;
                return;
            }

            skeletonAnimation.AnimationState.Complete += OnAnimationComplete;
            Debug.Log("SpineAnimationController: Initialized", this);
        }

        private void OnDestroy()
        {
            if (skeletonAnimation != null)
            {
                skeletonAnimation.AnimationState.Complete -= OnAnimationComplete;
            }
        }

        public void PlayAnimation(string animation, bool loop, float speed = 1f, int trackIndex = 0, Action onComplete = null)
        {
            if (string.IsNullOrEmpty(animation) || skeletonAnimation == null || skeletonAnimation.SkeletonDataAsset == null)
            {
                Debug.LogError($"SpineAnimationController: Cannot play animation! Animation={(animation ?? "null")}, SkeletonAnimation={(skeletonAnimation == null ? "null" : "exists")}, SkeletonDataAsset={(skeletonAnimation?.SkeletonDataAsset == null ? "null" : "exists")}", this);
                return;
            }

            _currentAnimation = animation;
            _isLooping = loop;
            _currentSpeed = speed;
            _onComplete = onComplete;
            _isJumpStartOrEnd = animation == animationStateManager.GetAnimation(CharacterAnimationType.Jump, 0)?.Animation ||
                                animation == animationStateManager.GetAnimation(CharacterAnimationType.Jump, 2)?.Animation;

            skeletonAnimation.Skeleton.SetToSetupPose();
            skeletonAnimation.AnimationState.SetAnimation(trackIndex, animation, loop).TimeScale = speed;
            Debug.Log($"SpineAnimationController: Playing animation: {animation}, Loop={loop}, Speed={speed}, IsJumpStartOrEnd={_isJumpStartOrEnd}", this);
        }

        public void StopAnimation()
        {
            if (skeletonAnimation != null)
            {
                skeletonAnimation.Skeleton.SetToSetupPose();
                _currentAnimation = null;
                _isLooping = false;
                _isJumpStartOrEnd = false;
                _onComplete = null;
                Debug.Log("SpineAnimationController: Animation stopped", this);
            }
            else
            {
                Debug.LogError("SpineAnimationController: SkeletonAnimation not initialized, cannot stop!", this);
            }
        }

        public void FlipSkeleton(float direction)
        {
            if (skeletonAnimation == null || skeletonAnimation.Skeleton == null)
            {
                Debug.LogError("SpineAnimationController: SkeletonAnimation or Skeleton is null!", this);
                return;
            }

            float previousScaleX = skeletonAnimation.Skeleton.ScaleX;
            if (Mathf.Abs(direction) > 0.1f)
            {
                skeletonAnimation.Skeleton.ScaleX = direction > 0 ? 1f : -1f;
                Debug.Log($"SpineAnimationController: Flipped skeleton, direction={direction}, ScaleX={skeletonAnimation.Skeleton.ScaleX} (was {previousScaleX})", this);
            }
        }

        public bool IsOtherAnimationActive()
        {
            if (skeletonAnimation == null || animationStateManager == null)
            {
                Debug.LogError("SpineAnimationController: SkeletonAnimation or AnimationStateManager is null!", this);
                return false;
            }

            var idleAnim = animationStateManager.GetAnimation(CharacterAnimationType.Idle);
            bool isActive = !string.IsNullOrEmpty(_currentAnimation) && _currentAnimation != idleAnim?.Animation;
            return isActive;
        }

        private void OnAnimationComplete(Spine.TrackEntry trackEntry)
        {
            if (animationStateManager == null || skeletonAnimation == null || trackEntry == null || trackEntry.Animation == null)
            {
                return;
            }

            string animationName = trackEntry.Animation.Name ?? "null";

            // 🔹 если анимация зацикленная – Spine сам её крутит, игнорируем Complete
            if (_isLooping)
            {
                Debug.Log($"SpineAnimationController: Looping animation completed but ignored (animation={animationName})", this);
                return;
            }

            var jumpStartAnim = animationStateManager.GetAnimation(CharacterAnimationType.Jump, 0);
            var jumpAirAnim = animationStateManager.GetAnimation(CharacterAnimationType.Jump, 1);
            var jumpLandAnim = animationStateManager.GetAnimation(CharacterAnimationType.Jump, 2);
            var armedTransitionAnim = animationStateManager.GetAnimation(CharacterAnimationType.ArmedTransition);
            var idleAnim = animationStateManager.GetAnimation(CharacterAnimationType.Idle);

            if (idleAnim?.IsValid() == true && animationName == idleAnim.Animation)
            {
                return;
            }

            if (jumpStartAnim?.IsValid() == true && animationName == jumpStartAnim.Animation && jumpAirAnim?.IsValid() == true)
            {
                PlayAnimation(jumpAirAnim.Animation, jumpAirAnim.Loop, jumpAirAnim.Speed);
            }
            else if (jumpAirAnim?.IsValid() == true && animationName == jumpAirAnim.Animation)
            {
                Debug.Log($"SpineAnimationController: JumpAir completed, frozen in last frame until landing", this);
            }
            else if (jumpLandAnim?.IsValid() == true && animationName == jumpLandAnim.Animation)
            {
                _isJumpStartOrEnd = false;
                OnJumpEndComplete?.Invoke();
                _onComplete?.Invoke();
            }
            else if (animationStateManager.CurrentState == CharacterState.Armed)
            {
                var attack1Anim = animationStateManager.GetAnimation(CharacterAnimationType.Attack, 0);
                var attack2Anim = animationStateManager.GetAnimation(CharacterAnimationType.Attack, 1);
                var attack3Anim = animationStateManager.GetAnimation(CharacterAnimationType.Attack, 2);

                if (attack1Anim?.IsValid() == true && animationName == attack1Anim.Animation)
                {
                    OnAttackComplete?.Invoke(1);
                }
                else if (attack2Anim?.IsValid() == true && animationName == attack2Anim.Animation)
                {
                    OnAttackComplete?.Invoke(2);
                }
                else if (attack3Anim?.IsValid() == true && animationName == attack3Anim.Animation)
                {
                    OnAttackComplete?.Invoke(3);
                }
            }
            else if (armedTransitionAnim?.IsValid() == true && animationName == armedTransitionAnim.Animation)
            {
                Debug.Log($"SpineAnimationController: ArmedTransition completed", this);
            }
        }
    }
}
