using UnityEngine;
using Spine.Unity;
using Spine;
using System;

namespace NPC.Characters.Player {
    [RequireComponent(typeof(SpineAnimationController), typeof(SkeletonAnimation), typeof(AnimationStateManager))]
    public class DisarmingTransitionAnimation : MonoBehaviour, IAnimation, IArmAnimation {
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
                Debug.LogError("DisarmingTransitionAnimation: SpineAnimationController, SkeletonAnimation или AnimationStateManager не найдены на объекте!", this);
                enabled = false;
                return;
            }

            var config = animationStateManager.GetCurrentStateConfig();
            if (config == null)
            {
                Debug.LogError("DisarmingTransitionAnimation: Текущая конфигурация анимаций не найдена в AnimationStateManager!", this);
                enabled = false;
                return;
            }

            var disarmedTransition = animationStateManager.GetAnimation(CharacterAnimationType.DisarmedTransition);
            if (!disarmedTransition.IsValid())
            {
                Debug.LogWarning("DisarmingTransitionAnimation: Анимация DisarmedTransition не найдена!", this);
            }

            skeletonAnimation.AnimationState.Complete += OnAnimationComplete;
            Debug.Log($"DisarmingTransitionAnimation: Инициализирован, DisarmedTransition={(disarmedTransition.IsValid() ? disarmedTransition.Animation : "null")}", this);
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
            PlayDisarmedTransition();
        }

        public void PlayArmedTransition()
        {
            Debug.LogError("DisarmingTransitionAnimation: PlayArmedTransition не поддерживается, используйте ArmingTransitionAnimation!", this);
        }

        public void PlayDisarmedTransition()
        {
            if (!enabled || spineController == null || animationStateManager == null)
            {
                Debug.LogError("DisarmingTransitionAnimation: Компонент отключен или SpineAnimationController/AnimationStateManager не инициализированы!", this);
                _onTransitionComplete?.Invoke(false);
                PlayIdle();
                return;
            }

            var animConfig = animationStateManager.GetAnimation(CharacterAnimationType.DisarmedTransition);
            if (animConfig.IsValid() && !string.IsNullOrEmpty(animConfig.Animation))
            {
                if (spineController.CurrentAnimation != animConfig.Animation)
                {
                    spineController.PlayAnimation(animConfig.Animation, animConfig.Loop, animConfig.Speed);
                    currentAnimation = animConfig.Animation;
                    Debug.Log($"DisarmingTransitionAnimation: Воспроизведена анимация DisarmedTransition: {animConfig.Animation}, Loop={animConfig.Loop}, Speed={animConfig.Speed}", this);
                }
            }
            else
            {
                Debug.LogWarning($"DisarmingTransitionAnimation: Не удалось воспроизвести DisarmedTransition, animConfig.IsValid={animConfig.IsValid()}, Animation={(animConfig.Animation ?? "null")}. Переход к Idle.", this);
                _onTransitionComplete?.Invoke(false);
                PlayIdle();
            }
        }

        private void OnAnimationComplete(TrackEntry trackEntry)
        {
            if (spineController == null || animationStateManager == null)
            {
                Debug.LogError("DisarmingTransitionAnimation: SpineAnimationController или AnimationStateManager не инициализированы!", this);
                _onTransitionComplete?.Invoke(false);
                PlayIdle();
                return;
            }

            if (trackEntry == null || trackEntry.Animation == null)
            {
                Debug.LogError($"DisarmingTransitionAnimation: trackEntry или trackEntry.Animation равны null! trackEntry={(trackEntry == null ? "null" : "exists")}, trackEntry.Animation={(trackEntry?.Animation == null ? "null" : "exists")}", this);
                _onTransitionComplete?.Invoke(false);
                PlayIdle();
                return;
            }

            var disarmedAnim = animationStateManager.GetAnimation(CharacterAnimationType.DisarmedTransition);
            string animationName = trackEntry.Animation.Name;

            Debug.Log($"DisarmingTransitionAnimation: OnAnimationComplete, animationName={animationName}, disarmedAnim={(disarmedAnim.IsValid() ? disarmedAnim.Animation : "null")}", this);

            if (disarmedAnim.IsValid() && animationName == disarmedAnim.Animation)
            {
                Debug.Log("DisarmingTransitionAnimation: Завершена анимация DisarmedTransition, переход к Idle", this);
                _onTransitionComplete?.Invoke(false);
                PlayIdle();
            }
        }

        private void PlayIdle()
        {
            if (TryGetComponent<IdleAnimation>(out var idleComponent))
            {
                var idleAnim = animationStateManager.GetAnimation(CharacterAnimationType.Idle);
                if (idleAnim.IsValid() && !string.IsNullOrEmpty(idleAnim.Animation))
                {
                    currentAnimation = idleAnim.Animation;
                    idleComponent.Play();
                    Debug.Log($"DisarmingTransitionAnimation: Воспроизведена анимация Idle, currentAnimation={currentAnimation}", this);
                }
                else
                {
                    Debug.LogError($"DisarmingTransitionAnimation: Не удалось воспроизвести Idle, анимация невалидна: {(idleAnim.Animation ?? "null")}", this);
                }
            }
            else
            {
                Debug.LogError("DisarmingTransitionAnimation: Компонент IdleAnimation не найден!", this);
            }
        }
    }
}