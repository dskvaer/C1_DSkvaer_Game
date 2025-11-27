using UnityEngine;

namespace NPC.Characters.Player {
    [RequireComponent(typeof(Rigidbody2D), typeof(SpineAnimationController), typeof(AnimationStateManager))]
    public class ArmStateManager : MonoBehaviour {
        [SerializeField] private SpineAnimationController animationController;
        [SerializeField] private CharacterStateAnimationsConfig defaultStateConfig;
        [SerializeField] private CharacterStateAnimationsConfig armedStateConfig;
        [SerializeField] private ArmingTransitionAnimation armingAnimation;
        [SerializeField] private DisarmingTransitionAnimation disarmingAnimation;
        [SerializeField] private PlayerStateManager playerStateManager;

        private Rigidbody2D _rigidbody;
        private AnimationStateManager _animationStateManager;

        private void Awake()
        {
            if (!TryGetComponent<Rigidbody2D>(out _rigidbody))
            {
                Debug.LogError("ArmStateManager: Rigidbody2D не найден!", this);
                enabled = false;
                return;
            }

            if (animationController == null)
            {
                Debug.LogError("ArmStateManager: SpineAnimationController не привязан!", this);
                enabled = false;
                return;
            }

            if (defaultStateConfig == null || armedStateConfig == null)
            {
                Debug.LogError("ArmStateManager: CharacterStateAnimationsConfig (defaultStateConfig или armedStateConfig) не привязан!", this);
                enabled = false;
                return;
            }

            if (armingAnimation == null)
            {
                Debug.LogError("ArmStateManager: ArmingTransitionAnimation не привязан!", this);
                enabled = false;
                return;
            }

            if (disarmingAnimation == null)
            {
                Debug.LogError("ArmStateManager: DisarmingTransitionAnimation не привязан!", this);
                enabled = false;
                return;
            }

            if (playerStateManager == null)
            {
                playerStateManager = GetComponent<PlayerStateManager>();
                if (playerStateManager == null)
                {
                    Debug.LogError("ArmStateManager: PlayerStateManager не привязан и не найден на объекте!", this);
                    enabled = false;
                    return;
                }
            }

            if (!TryGetComponent<AnimationStateManager>(out _animationStateManager))
            {
                Debug.LogError("ArmStateManager: AnimationStateManager не найден на объекте!", this);
                enabled = false;
                return;
            }

            Debug.Log("ArmStateManager: Инициализирован", this);
        }

        private void OnEnable()
        {
            if (armingAnimation != null)
            {
                armingAnimation.OnTransitionComplete += OnTransitionCompleteHandler;
            }
            if (disarmingAnimation != null)
            {
                disarmingAnimation.OnTransitionComplete += OnTransitionCompleteHandler;
            }
        }

        private void OnDisable()
        {
            if (armingAnimation != null)
            {
                armingAnimation.OnTransitionComplete -= OnTransitionCompleteHandler;
            }
            if (disarmingAnimation != null)
            {
                disarmingAnimation.OnTransitionComplete -= OnTransitionCompleteHandler;
            }
        }

        public void TriggerArmTransition(bool toArmed)
        {
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = new Vector2(0f, _rigidbody.linearVelocity.y);
            }
            if (toArmed)
            {
                armingAnimation.PlayArmedTransition();
            }
            else
            {
                disarmingAnimation.PlayDisarmedTransition();
            }
            Debug.Log($"ArmStateManager: Запуск перехода, toArmed={toArmed}", this);
        }

        private void OnTransitionCompleteHandler(bool isArmed)
        {
            if (_animationStateManager != null)
            {
                _animationStateManager.SetState(isArmed ? CharacterState.Armed : CharacterState.Standart);
            }
            if (playerStateManager != null)
            {
                playerStateManager.OnTransitionComplete(isArmed);
            }
            Debug.Log($"ArmStateManager: Переход завершён, isArmed={isArmed}, state={(isArmed ? armedStateConfig.StateName : defaultStateConfig.StateName)}", this);
        }
    }
}