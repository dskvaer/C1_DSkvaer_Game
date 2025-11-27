using UnityEngine;
using Spine.Unity;

namespace NPC.Characters.Player {
    [RequireComponent(typeof(PlayerController), typeof(PlayerAttack), typeof(AttackStateController))]
    public class PlayerAttackMovement : MonoBehaviour {
        [SerializeField] private AttackSettings attackSettings;
        [SerializeField] private Transform attackZone;
        [SerializeField] private SkeletonAnimation skeletonAnimation;
        private PlayerController _playerController;
        private AttackStateController _attackStateController;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
            _attackStateController = GetComponent<AttackStateController>();

            if (attackSettings == null || attackZone == null || skeletonAnimation == null || _attackStateController == null)
            {
#if DEBUG
                Debug.LogError($"PlayerAttackMovement: Missing required components (AttackSettings: {attackSettings}, AttackZone: {attackZone}, SkeletonAnimation: {skeletonAnimation}, AttackStateController: {_attackStateController})!", this);
#endif
                enabled = false;
                return;
            }

            _attackStateController.OnAttackStart += OnAttackStartedHandler;
#if DEBUG
            Debug.Log("PlayerAttackMovement: Инициализирован", this);
#endif
        }

        private void OnDestroy()
        {
            if (_attackStateController != null)
            {
                _attackStateController.OnAttackStart -= OnAttackStartedHandler;
            }
        }

        private void Update()
        {
            if (_attackStateController.IsAttacking && _playerController.Movement.MoveInput.x != 0)
            {
                attackZone.localPosition = new Vector3(
                    _playerController.Movement.MoveInput.x > 0 ? attackSettings.AttackZoneOffsetX : -attackSettings.AttackZoneOffsetX,
                    attackZone.localPosition.y,
                    attackZone.localPosition.z);
            }
        }

        private void OnAttackStartedHandler()
        {
            if (_attackStateController.CurrentPhase == AttackPhase.Attack1)
            {
                float stepDirection = skeletonAnimation.Skeleton.ScaleX > 0 ? 1f : -1f;
                _playerController.Movement.Rigidbody.AddForce(new Vector2(stepDirection * attackSettings.AttackStepForce, 0f), ForceMode2D.Impulse);
#if DEBUG
                Debug.Log($"PlayerAttackMovement: Применён шаг для атаки 1, direction={stepDirection}, force={attackSettings.AttackStepForce}", this);
#endif
            }
        }
    }
}