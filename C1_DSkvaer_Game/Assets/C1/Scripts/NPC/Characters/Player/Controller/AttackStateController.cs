using UnityEngine;
using System;

namespace NPC.Characters.Player {
    public enum AttackPhase {
        None,
        Attack1,
        Attack2,
        Attack3
    }

    [RequireComponent(typeof(PlayerAttack), typeof(PlayerAttackAnimation), typeof(AnimationStateManager))]
    public class AttackStateController : MonoBehaviour {
        [SerializeField] private AttackSettings attackSettings;
        [SerializeField] private PlayerAttackAnimation attackAnimation;
        private AnimationStateManager _animationStateManager;
        private AttackPhase _currentPhase = AttackPhase.None;
        private float _attackTimer;
        private float _comboTimer;
        private bool _isAttacking;
        private bool _canAttack = true;
        private int _comboCount = 1;
        private const float ComboTimeout = 1.2f;

        public Action OnAttackStart { get; set; }
        public Action OnAttackComplete { get; set; }

        public AttackPhase CurrentPhase => _currentPhase;
        public bool IsAttacking => _isAttacking;
        public bool CanAttack => _canAttack;

        private void Awake()
        {
            if (attackSettings == null)
            {
#if DEBUG
                Debug.LogError("AttackStateController: AttackSettings не назначен в инспекторе!", this);
#endif
                enabled = false;
                return;
            }

            if (attackAnimation == null && !TryGetComponent(out attackAnimation))
            {
#if DEBUG
                Debug.LogError("AttackStateController: PlayerAttackAnimation не найден!", this);
#endif
                enabled = false;
                return;
            }

            if (!TryGetComponent(out _animationStateManager))
            {
#if DEBUG
                Debug.LogError("AttackStateController: AnimationStateManager не найден!", this);
#endif
                enabled = false;
                return;
            }

            attackAnimation.OnAttackComplete += OnAttackCompleteHandler;
#if DEBUG
            Debug.Log("AttackStateController: Инициализирован", this);
#endif
        }

        private void OnDestroy()
        {
            if (attackAnimation != null)
            {
                attackAnimation.OnAttackComplete -= OnAttackCompleteHandler;
            }
        }

        private void FixedUpdate()
        {
            if (_animationStateManager.CurrentState != CharacterState.Armed)
            {
                if (_isAttacking)
                {
                    ResetAttackPhase();
#if DEBUG
                    Debug.Log($"AttackStateController: Сброс атаки, так как текущий режим: {_animationStateManager.CurrentState}", this);
#endif
                }
                return;
            }

            if (_isAttacking)
            {
                _attackTimer -= Time.fixedDeltaTime;
                if (_attackTimer <= 0)
                {
                    ResetAttackPhase();
#if DEBUG
                    Debug.Log($"AttackStateController: Атака завершена, phase={_currentPhase}, isAttacking={_isAttacking}, canAttack={_canAttack}, comboCount={_comboCount}", this);
#endif
                }
            }

            if (_comboTimer > 0)
            {
                _comboTimer -= Time.fixedDeltaTime;
                if (_comboTimer <= 0)
                {
                    _comboCount = 1;
#if DEBUG
                    Debug.Log($"AttackStateController: Комбо таймер истёк, сброс comboCount={_comboCount}", this);
#endif
                }
            }
        }

        public void StartAttack()
        {
            if (_animationStateManager.CurrentState != CharacterState.Armed)
            {
#if DEBUG
                Debug.LogWarning($"AttackStateController: Атака невозможна, текущий режим: {_animationStateManager.CurrentState}", this);
#endif
                return;
            }

            if (!_canAttack || _isAttacking)
            {
#if DEBUG
                Debug.LogWarning($"AttackStateController: Атака невозможна, canAttack={_canAttack}, isAttacking={_isAttacking}", this);
#endif
                return;
            }

            _isAttacking = true;
            _canAttack = false;

            if (attackSettings.EnableCombo && _comboTimer > 0)
            {
                _comboCount = Mathf.Clamp(_comboCount + 1, 1, 3);
            }
            else
            {
                _comboCount = 1;
            }

            _currentPhase = _comboCount switch
            {
                1 => AttackPhase.Attack1,
                2 => AttackPhase.Attack2,
                3 => AttackPhase.Attack3,
                _ => AttackPhase.Attack1
            };

            _attackTimer = attackAnimation.GetAttackDuration(_comboCount, 1f);
            _comboTimer = ComboTimeout;
            attackAnimation.PlayAttack(_comboCount, 1f);
            OnAttackStart?.Invoke();
#if DEBUG
            Debug.Log($"AttackStateController: Атака начата, phase={_currentPhase}, comboCount={_comboCount}, attackTimer={_attackTimer}", this);
#endif
        }

        public void ResetAttackPhase()
        {
            _currentPhase = AttackPhase.None;
            _isAttacking = false;
            _canAttack = true;
            OnAttackComplete?.Invoke();
#if DEBUG
            Debug.Log($"AttackStateController: Сброс фазы атаки, phase={_currentPhase}, isAttacking={_isAttacking}, canAttack={_canAttack}", this);
#endif
        }

        private void OnAttackCompleteHandler(int comboCount)
        {
            if (_animationStateManager.CurrentState != CharacterState.Armed)
            {
#if DEBUG
                Debug.LogWarning($"AttackStateController: Игнорируем завершение атаки, текущий режим: {_animationStateManager.CurrentState}", this);
#endif
                return;
            }

            ResetAttackPhase();
#if DEBUG
            Debug.Log($"AttackStateController: Получено завершение атаки {comboCount}, phase={_currentPhase}", this);
#endif
        }
    }
}