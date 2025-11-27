using UnityEngine;
using Spine.Unity;
using System;

namespace NPC.Characters.Player {
    [RequireComponent(typeof(SkeletonAnimation))]
    public class AnimationController : MonoBehaviour, IAnimationController {
        [SerializeField] private SkeletonAnimation skeletonAnimation;
        [SerializeField] private AnimationStateManager animationStateManager;

        private string _currentAnimation;
        private bool _isLooping;
        private bool _isJumpStartOrEnd;

        public string CurrentAnimation => _currentAnimation;
        public bool IsLooping => _isLooping;
        public bool IsJumpStartOrEnd => _isJumpStartOrEnd;
        public Action OnJumpEndComplete { get; set; }

        private void Awake()
        {
            if (skeletonAnimation == null && !TryGetComponent(out skeletonAnimation))
            {
#if DEBUG
                Debug.LogError("AnimationController: SkeletonAnimation не найден на объекте!", this);
#endif
                enabled = false;
                return;
            }

            if (skeletonAnimation.SkeletonDataAsset == null)
            {
#if DEBUG
                Debug.LogError("AnimationController: SkeletonDataAsset не назначен в SkeletonAnimation!", this);
#endif
                enabled = false;
                return;
            }

            if (!TryGetComponent(out animationStateManager))
            {
#if DEBUG
                Debug.LogError("AnimationController: AnimationStateManager не найден на объекте!", this);
#endif
                enabled = false;
                return;
            }

            skeletonAnimation.AnimationState.Complete += OnAnimationComplete;
#if DEBUG
            Debug.Log("AnimationController: Инициализирован", this);
#endif
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
#if DEBUG
                Debug.LogError("AnimationController: Animation или SkeletonAnimation/SkeletonDataAsset равны null!", this);
#endif
                return;
            }

            _currentAnimation = animation;
            _isLooping = loop;
            _isJumpStartOrEnd = animation == animationStateManager.GetAnimation(CharacterAnimationType.Jump, 0).Animation ||
                                animation == animationStateManager.GetAnimation(CharacterAnimationType.Jump, 2).Animation;

            skeletonAnimation.Skeleton.SetToSetupPose();
            skeletonAnimation.AnimationState.SetAnimation(0, animation, loop).TimeScale = speed;

            if (onComplete != null)
            {
                skeletonAnimation.AnimationState.Complete += (te) => onComplete?.Invoke();
            }

#if DEBUG
            Debug.Log($"AnimationController: Воспроизведение анимации: {animation}, loop={loop}, speed={speed}", this);
#endif
        }

        public void StopAnimation()
        {
            if (skeletonAnimation != null)
            {
                skeletonAnimation.Skeleton.SetToSetupPose();
                _currentAnimation = null;
                _isLooping = false;
                _isJumpStartOrEnd = false;
#if DEBUG
                Debug.Log("AnimationController: Анимация остановлена", this);
#endif
            }
        }

        public void FlipSkeleton(float direction)
        {
            if (skeletonAnimation == null || skeletonAnimation.Skeleton == null)
            {
#if DEBUG
                Debug.LogError("AnimationController: SkeletonAnimation или его Skeleton равны null!", this);
#endif
                return;
            }

            if (Mathf.Abs(direction) > 0.1f)
            {
                skeletonAnimation.Skeleton.ScaleX = direction > 0 ? 1f : -1f;
#if DEBUG
                Debug.Log($"AnimationController: Разворот скелета, direction={direction}", this);
#endif
            }
        }

        public bool IsOtherAnimationActive()
        {
            if (skeletonAnimation == null || animationStateManager == null)
            {
                return false;
            }

            var idleAnim = animationStateManager.GetAnimation(CharacterAnimationType.Idle);
            bool isActive = !string.IsNullOrEmpty(_currentAnimation) && _currentAnimation != idleAnim.Animation;
            return isActive;
        }

        private void OnAnimationComplete(Spine.TrackEntry trackEntry)
        {
            if (animationStateManager == null || skeletonAnimation == null)
            {
#if DEBUG
                Debug.LogError("AnimationController: AnimationStateManager или skeletonAnimation равны null!", this);
#endif
                return;
            }

            string animationName = trackEntry.Animation?.Name;
            var jumpStartAnim = animationStateManager.GetAnimation(CharacterAnimationType.Jump, 0);
            var jumpLandAnim = animationStateManager.GetAnimation(CharacterAnimationType.Jump, 2);

            if (animationName == jumpStartAnim.Animation && animationStateManager.GetAnimation(CharacterAnimationType.Jump, 1).IsValid())
            {
                var jumpAirAnim = animationStateManager.GetAnimation(CharacterAnimationType.Jump, 1);
                PlayAnimation(jumpAirAnim.Animation, jumpAirAnim.Loop, jumpAirAnim.Speed);
#if DEBUG
                Debug.Log($"AnimationController: JumpStart завершён, переход к JumpAir", this);
#endif
            }
            else if (animationName == jumpLandAnim.Animation)
            {
                _isJumpStartOrEnd = false;
                OnJumpEndComplete?.Invoke();
#if DEBUG
                Debug.Log($"AnimationController: JumpLand завершён, вызов OnJumpEndComplete, currentAnimation={_currentAnimation}", this);
#endif
            }
        }
    }
}