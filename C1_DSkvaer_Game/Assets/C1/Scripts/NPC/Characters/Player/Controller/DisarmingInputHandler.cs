using UnityEngine;

namespace NPC.Characters.Player {
    [RequireComponent(typeof(Rigidbody2D), typeof(SpineAnimationController))]
    public class DisarmingInputHandler : MonoBehaviour, IArmInputHandler {
        [SerializeField] private SpineAnimationController animationController;
        private IArmStateManager _stateManager;
        private Rigidbody2D _rigidbody;
        private bool _isInitialized;

        public void Initialize(IArmStateManager stateManager)
        {
            _stateManager = stateManager;
            _isInitialized = true;
            Debug.Log("DisarmingInputHandler: Инициализирован", this);
        }

        private void Awake()
        {
            if (!TryGetComponent<Rigidbody2D>(out _rigidbody))
            {
                Debug.LogError("DisarmingInputHandler: Rigidbody2D не найден!", this);
                enabled = false;
                return;
            }

            if (animationController == null)
            {
                Debug.LogError("DisarmingInputHandler: SpineAnimationController не привязан!", this);
                enabled = false;
                return;
            }
        }

        public void ArmHandler()
        {
            if (!_isInitialized || _stateManager == null)
            {
                Debug.LogError("DisarmingInputHandler: IArmStateManager не инициализирован!", this);
                return;
            }

            if (_rigidbody == null || !animationController.enabled || animationController.IsOtherAnimationActive() || _stateManager.IsTransitioning)
            {
                Debug.Log($"DisarmingInputHandler: ArmHandler отклонён - Rigidbody={(_rigidbody != null)}, AnimationControllerEnabled={animationController.enabled}, IsOtherAnimationActive={animationController.IsOtherAnimationActive()}, IsTransitioning={_stateManager.IsTransitioning}", this);
                return;
            }

            bool isGrounded = CheckGround();
            if (!isGrounded)
            {
                Debug.Log("DisarmingInputHandler: ArmHandler отклонён - персонаж не на земле!", this);
                return;
            }

            _stateManager.TriggerArmTransition(false);
            Debug.Log("DisarmingInputHandler: Запуск перехода в состояние Standart", this);
        }

        private bool CheckGround()
        {
            Vector2 rayOrigin = _rigidbody.position + new Vector2(0f, -0.5f);
            LayerMask groundLayer = LayerMask.GetMask("Ground");
            LayerMask pushableLayer = LayerMask.GetMask("Pushable");
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 0.1f, groundLayer | pushableLayer);
            Debug.Log($"DisarmingInputHandler: CheckGround - hit={hit.collider != null}, hitObject={(hit.collider != null ? hit.collider.name : "None")}", this);
            return hit.collider != null;
        }
    }
}