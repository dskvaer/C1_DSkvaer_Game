using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace NPC.Characters.Player {
    [RequireComponent(typeof(PlayerController), typeof(PlayerAttackAnimation))]
    [RequireComponent(typeof(AttackStateController), typeof(AnimationStateManager))]
    public class PlayerAttack : MonoBehaviour, IPlayerAttack {
        [SerializeField] private AttackSettings attackSettings;
        [SerializeField] private Transform attackZone;
        private PlayerController _playerController;
        private AttackStateController _attackStateController;
        private AnimationStateManager _animationStateManager;

        public bool IsAttacking => _attackStateController.IsAttacking;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
            _attackStateController = GetComponent<AttackStateController>();
            _animationStateManager = GetComponent<AnimationStateManager>();

            if (attackSettings == null || attackZone == null || _attackStateController == null || _animationStateManager == null)
            {
#if DEBUG
                Debug.LogError($"PlayerAttack: Missing required components (AttackSettings: {attackSettings}, AttackZone: {attackZone}, AttackStateController: {_attackStateController}, AnimationStateManager: {_animationStateManager})!", this);
#endif
                enabled = false;
                return;
            }

#if DEBUG
            Debug.Log("PlayerAttack: Инициализирован", this);
#endif
        }

        private void OnEnable()
        {
            var inputSystem = _playerController.InputSystem;
            if (inputSystem != null)
            {
                inputSystem.Player.Attack.performed += Attack;
            }
        }

        private void OnDisable()
        {
            var inputSystem = _playerController.InputSystem;
            if (inputSystem != null)
            {
                inputSystem.Player.Attack.performed -= Attack;
            }
        }

        public void UpdateAttack()
        {
            // Логика обновления в AttackStateController.FixedUpdate
        }

        public void Attack(InputAction.CallbackContext _context)
        {
            if (_animationStateManager.CurrentState != CharacterState.Armed)
            {
#if DEBUG
                Debug.LogWarning($"PlayerAttack: Атака отклонена, текущий режим: {_animationStateManager.CurrentState}", this);
#endif
                return;
            }

            if (_playerController.ArmState.IsArmed &&
                _playerController.Jump.IsGrounded() &&
                !_playerController.Jump.IsJumping() &&
                !_playerController.ArmState.IsTransitioning &&
                _attackStateController.CanAttack)
            {
                _attackStateController.StartAttack();
                attackZone.gameObject.SetActive(true);
#if DEBUG
                Debug.Log($"PlayerAttack: Атака инициирована, phase={_attackStateController.CurrentPhase}", this);
#endif
            }
            else
            {
#if DEBUG
                Debug.LogWarning($"PlayerAttack: Атака отклонена, ArmState.IsArmed={_playerController.ArmState.IsArmed}, IsGrounded={_playerController.Jump.IsGrounded()}, IsJumping={_playerController.Jump.IsJumping()}, IsTransitioning={_playerController.ArmState.IsTransitioning}, CanAttack={_attackStateController.CanAttack}", this);
#endif
            }
        }

        public void EndAttack()
        {
            attackZone.gameObject.SetActive(false);
#if DEBUG
            Debug.Log("PlayerAttack: Атака завершена", this);
#endif
        }
    }
}