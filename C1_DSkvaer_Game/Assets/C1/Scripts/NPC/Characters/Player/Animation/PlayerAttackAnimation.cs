using UnityEngine;
using Spine.Unity;
using Spine;

namespace NPC.Characters.Player {
    [RequireComponent(typeof(SpineAnimationController), typeof(AnimationStateManager))]
    public class PlayerAttackAnimation : MonoBehaviour, IAttackAnimation {
        private SpineAnimationController _animationController;
        private AnimationStateManager _animationStateManager;
        private bool _isAttackAnimationActive;

        public System.Action<int> OnAttackComplete { get; set; }
        public bool IsAttackAnimationActive => _isAttackAnimationActive;

        private void Awake()
        {
            if (!TryGetComponent(out _animationController))
            {
#if DEBUG
                Debug.LogError("PlayerAttackAnimation: SpineAnimationController не найден!", this);
#endif
                enabled = false;
                return;
            }

            if (!TryGetComponent(out _animationStateManager))
            {
#if DEBUG
                Debug.LogError("PlayerAttackAnimation: AnimationStateManager не найден!", this);
#endif
                enabled = false;
                return;
            }

            _animationController.OnAttackComplete += OnAttackCompleteHandler;
#if DEBUG
            Debug.Log("PlayerAttackAnimation: Инициализирован", this);
#endif
        }

        private void OnDestroy()
        {
            if (_animationController != null)
            {
                _animationController.OnAttackComplete -= OnAttackCompleteHandler;
            }
        }

        public void Play()
        {
            if (_animationStateManager.CurrentState != CharacterState.Armed)
            {
#if DEBUG
                Debug.LogWarning($"PlayerAttackAnimation: Невозможно воспроизвести атаку, текущий режим: {_animationStateManager.CurrentState}", this);
#endif
                return;
            }
            PlayAttack(1, 1f); // По умолчанию первая атака
        }

        public void PlayAttack(int comboCount, float attackSpeedMultiplier)
        {
            if (_animationStateManager.CurrentState != CharacterState.Armed)
            {
#if DEBUG
                Debug.LogWarning($"PlayerAttackAnimation: Невозможно воспроизвести атаку, текущий режим: {_animationStateManager.CurrentState}", this);
#endif
                return;
            }

            AnimationConfig attackConfig = _animationStateManager.GetAnimation(CharacterAnimationType.Attack, comboCount - 1);
            if (!attackConfig.IsValid() || string.IsNullOrEmpty(attackConfig.Animation))
            {
#if DEBUG
                Debug.LogError($"PlayerAttackAnimation: Неверная конфигурация атаки {comboCount}, Animation: {(attackConfig.Animation ?? "null")}", this);
#endif
                return;
            }

            if (_animationController.CurrentAnimation != attackConfig.Animation)
            {
                float modifiedSpeed = attackConfig.Speed * attackSpeedMultiplier;
                
                _animationController.PlayAnimation(attackConfig.Animation, false, modifiedSpeed, 0, () =>
                {
                    var idleConfig = _animationStateManager.GetAnimation(CharacterAnimationType.Idle);
                    _animationController.PlayAnimation(idleConfig.Animation, idleConfig.Loop, idleConfig.Speed);
                });
                _isAttackAnimationActive = true;
#if DEBUG
                Debug.Log($"PlayerAttackAnimation: Воспроизведена атака {comboCount}, анимация: {attackConfig.Animation}, speed={modifiedSpeed}, loop=false", this);
#endif
            }
            else
            {
#if DEBUG
                Debug.Log($"PlayerAttackAnimation: Атака {comboCount} уже воспроизводится, пропускаем", this);
#endif
            }
        }

        public float GetAttackDuration(int comboCount, float attackSpeedMultiplier)
        {
            if (_animationStateManager.CurrentState != CharacterState.Armed)
            {
#if DEBUG
                Debug.LogWarning($"PlayerAttackAnimation: Невозможно получить длительность атаки, текущий режим: {_animationStateManager.CurrentState}", this);
#endif
                return 0.5f;
            }

            AnimationConfig attackConfig = _animationStateManager.GetAnimation(CharacterAnimationType.Attack, comboCount - 1);
            if (attackConfig.IsValid() && !string.IsNullOrEmpty(attackConfig.Animation))
            {
                var skeletonAnimation = GetComponent<SkeletonAnimation>();
                if (skeletonAnimation != null && skeletonAnimation.SkeletonDataAsset != null)
                {
                    var animation = skeletonAnimation.SkeletonDataAsset.GetSkeletonData(false).FindAnimation(attackConfig.Animation);
                    if (animation != null)
                    {
                        return animation.Duration / (attackConfig.Speed * attackSpeedMultiplier);
                    }
                }
            }
#if DEBUG
            Debug.LogWarning($"PlayerAttackAnimation: Длительность атаки {comboCount} не найдена, возвращаем 0.5f", this);
#endif
            return 0.5f;
        }

        private void OnAttackCompleteHandler(int comboCount)
        {
            if (_animationStateManager.CurrentState != CharacterState.Armed)
            {
#if DEBUG
                Debug.LogWarning($"PlayerAttackAnimation: Игнорируем завершение атаки, текущий режим: {_animationStateManager.CurrentState}", this);
#endif
                return;
            }

            _isAttackAnimationActive = false;
            OnAttackComplete?.Invoke(comboCount);
#if DEBUG
            Debug.Log($"PlayerAttackAnimation: Атака {comboCount} завершена", this);
#endif
        }
    }
}