using UnityEngine;
using Spine.Unity;

namespace NPC.Characters.Player {
    [RequireComponent(typeof(SkeletonAnimation), typeof(PlayerController))]
    [RequireComponent(typeof(PlayerPush), typeof(SpineAnimationController))]
    public class PlayerPushAnimation : MonoBehaviour, IPush, IAnimation {
        [SerializeField] private CharacterStateAnimationsConfig stateAnimations;
        [SerializeField] private PlayerStateManager stateManager;

        private SkeletonAnimation _skeletonAnimation;
        private IAnimationController _animationController;
        private PlayerController _playerController;
        private PlayerPush _playerPush;
        private PushConfig _pushConfig;
        private bool _isPushing;
        private bool _isInitialized;

        public bool IsPushing => _isPushing;
        public bool IsInPushContact => _playerPush != null && _playerPush.IsInPushContact;
        public bool IsInteractHeld => _playerPush != null && _playerPush.IsInteractHeld;

        public void Initialize(SkeletonAnimation skeletonAnimation)
        {
            _skeletonAnimation = skeletonAnimation;
            if (_skeletonAnimation == null || _skeletonAnimation.SkeletonDataAsset == null || _skeletonAnimation.Skeleton == null)
            {
                LogError($"SkeletonAnimation initialization failed: SkeletonAnimation={_skeletonAnimation}, SkeletonDataAsset={(_skeletonAnimation != null ? _skeletonAnimation.SkeletonDataAsset : "null")}, Skeleton={(_skeletonAnimation != null ? _skeletonAnimation.Skeleton : "null")}");
                enabled = false;
                return;
            }

            if (stateAnimations == null || !stateAnimations.HasAnimation(CharacterAnimationType.Push))
            {
                LogError($"CharacterStateAnimationsConfig or Push animation not assigned! StateAnimations={stateAnimations}, HasPushAnimation={(stateAnimations != null ? stateAnimations.HasAnimation(CharacterAnimationType.Push).ToString() : "N/A")}");
                enabled = false;
                return;
            }

            if (!TryGetComponent(out _animationController))
            {
                LogError("IAnimationController (SpineAnimationController) not found!");
                enabled = false;
                return;
            }

            if (!TryGetComponent(out _playerController))
            {
                LogError("PlayerController not found!");
                enabled = false;
                return;
            }

            if (!TryGetComponent(out _playerPush))
            {
                LogError("PlayerPush not found!");
                enabled = false;
                return;
            }

            if (!TryGetComponent(out stateManager))
            {
                LogError("PlayerStateManager not found!");
                enabled = false;
                return;
            }

            _pushConfig = _playerPush.Config;
            if (_pushConfig == null)
            {
                LogError("PushConfig not found in PlayerPush!");
                enabled = false;
                return;
            }

            _skeletonAnimation.AnimationState.Complete += OnAnimationComplete;
            _isInitialized = true;
            Log("Initialized");
        }

        public void EnableInput() { }
        public void DisableInput() { }

        public void UpdatePush()
        {
            if (!_isInitialized || _animationController == null || stateAnimations == null || !stateAnimations.CanPushObjects || _playerController == null || _playerPush == null || stateManager == null)
            {
                LogError($"UpdatePush: Invalid state! Initialized={_isInitialized}, AnimationController={_animationController}, StateAnimations={stateAnimations}, CanPushObjects={(stateAnimations != null ? stateAnimations.CanPushObjects.ToString() : "N/A")}, PlayerController={_playerController}, PlayerPush={_playerPush}, StateManager={stateManager}");
                return;
            }

            _playerPush.UpdatePush();
        }

        public void Play()
        {
            Play(loop: false);
        }

        public void Play(bool loop)
        {
            if (!_isInitialized)
            {
                LogError("Play: Not initialized!");
                return;
            }

            if (stateManager == null || stateManager.IsTransitioning)
            {
                LogError($"Play: Cannot play Push animation during transition! IsTransitioning={(stateManager != null ? stateManager.IsTransitioning.ToString() : "N/A")}");
                return;
            }

            var pushAnim = stateAnimations.GetAnimation(CharacterAnimationType.Push);
            if (!pushAnim.IsValid())
            {
                LogError($"Push animation not valid! StateAnimations={stateAnimations}, HasPushAnimation={(stateAnimations != null ? stateAnimations.HasAnimation(CharacterAnimationType.Push).ToString() : "N/A")}");
                _isPushing = false;
                return;
            }

            if (_animationController.CurrentAnimation == pushAnim.Animation && _animationController.IsLooping == loop)
            {
                return;
            }

            _animationController.StopAnimation();
            _animationController.PlayAnimation(pushAnim.Animation, loop, pushAnim.Speed);
            _isPushing = true;
            Log($"Push animation started (Play), loop={loop}, animation={pushAnim.Animation}, speed={pushAnim.Speed}");
        }

        public void StopPushAnimation()
        {
            if (_isPushing)
            {
                _animationController.StopAnimation();
                _isPushing = false;
                Log("Push animation stopped");
            }
        }

        public void PlayIdle()
        {
            if (_playerController == null || _skeletonAnimation == null)
            {
                LogError($"PlayIdle: Invalid components! PlayerController={_playerController}, SkeletonAnimation={_skeletonAnimation}");
                return;
            }

            float moveInputX = _playerController.Movement != null ? _playerController.Movement.MoveInput.x : 0f;
            float skeletonScaleX = _skeletonAnimation.Skeleton != null ? _skeletonAnimation.Skeleton.ScaleX : 1f;
            bool movingOpposite = Mathf.Abs(moveInputX) > 0.1f && Mathf.Sign(moveInputX) != skeletonScaleX;

            if (movingOpposite)
            {
                PlayRun();
                return;
            }

            if (TryGetComponent(out IdleAnimation idleComponent))
            {
                _animationController.StopAnimation();
                idleComponent.Play();
                Log("Played Idle animation");
            }
            else
            {
                LogError("IdleAnimation component not found!");
            }
        }

        private void OnAnimationComplete(Spine.TrackEntry trackEntry)
        {
            if (trackEntry == null || trackEntry.Animation == null || !_isInitialized) return;

            var pushAnim = stateAnimations.GetAnimation(CharacterAnimationType.Push);
            if (pushAnim.IsValid() && trackEntry.Animation.Name == pushAnim.Animation)
            {
                if (_animationController.IsLooping)
                {
                    if (_playerPush != null)
                    {
                        _playerPush.OnPushCycle();
                    }
                    return;
                }

                float moveInputX = _playerController.Movement != null ? _playerController.Movement.MoveInput.x : 0f;
                bool isInContact = _playerPush.IsInPushContact;
                bool isInteractHeld = _playerPush.IsInteractHeld;
                float skeletonScaleX = _skeletonAnimation.Skeleton != null ? _skeletonAnimation.Skeleton.ScaleX : 1f;
                bool isMovingInDirection = Mathf.Abs(moveInputX) > 0.1f && Mathf.Sign(moveInputX) == skeletonScaleX;

                if (isInContact && isMovingInDirection)
                {
                    Play(loop: isInteractHeld);
                    Log("Repeating Push animation (non-loop) because still moving in direction");
                }
                else
                {
                    StopPushAnimation();
                    if (isInContact && moveInputX == 0f)
                    {
                        PlayIdle();
                    }
                    else if (isInContact && !isMovingInDirection)
                    {
                        PlayRun();
                    }
                    else
                    {
                        PlayIdle();
                    }
                }
            }
        }

        private void PlayRun()
        {
            if (TryGetComponent(out MovementAnimationHandler mover))
            {
                mover.PlayRun();
                Log("Played Run via MovementAnimationHandler");
                return;
            }

            if (TryGetComponent(out RunAnimation runComponent))
            {
                _animationController.StopAnimation();
                runComponent.Play();
                Log("Played Run via RunAnimation");
                return;
            }

            LogError("RunAnimation or MovementAnimationHandler not found!");
        }

        private void OnEnable()
        {
            if (_isInitialized && _skeletonAnimation != null)
            {
                _skeletonAnimation.AnimationState.Complete += OnAnimationComplete;
            }
        }

        private void OnDisable()
        {
            if (_skeletonAnimation != null)
            {
                _skeletonAnimation.AnimationState.Complete -= OnAnimationComplete;
            }
        }

        private void Log(string message)
        {
            if (_pushConfig != null && _pushConfig.ShouldLog)
            {
                Debug.Log($"PlayerPushAnimation: {message}", this);
            }
        }

        private void LogError(string message)
        {
            Debug.LogError($"PlayerPushAnimation: {message}", this);
        }
    }
}