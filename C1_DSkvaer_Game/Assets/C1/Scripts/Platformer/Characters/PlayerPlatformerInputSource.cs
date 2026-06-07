using UnityEngine;
using UnityEngine.InputSystem;

#pragma warning disable 0414

namespace C1.Platformer.Characters {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInput))]
    public sealed class PlayerPlatformerInputSource : MonoBehaviour, IPlatformerInputSource {
        [Header("Описание")]
        [SerializeField, TextArea(3, 7)] private string inspectorDescription =
            "Источник ввода для игрока через Unity Input System. Подходит для клавиатуры, геймпада и Android, если экранные кнопки отправляют действия в PlayerInput. Для ручной привязки UI-кнопок используйте PlatformerTouchInputSource.";

        [Header("Player Input")]
        [Tooltip("Компонент Unity PlayerInput с actions asset.")]
        [SerializeField] private PlayerInput playerInput;
        [Tooltip("Включать/выключать actions вместе с этим компонентом.")]
        [SerializeField] private bool enableActionsOnEnable = true;

        [Header("Имена действий Input System")]
        [SerializeField] private string moveActionName = "Move";
        [SerializeField] private string lookActionName = "Look";
        [SerializeField] private string runActionName = "Sprint";
        [SerializeField] private string jumpActionName = "Jump";
        [SerializeField] private string crouchActionName = "Crouch";
        [SerializeField] private string interactActionName = "Interact";
        [SerializeField] private string armActionName = "Arm";
        [SerializeField] private string attackActionName = "Attack";
        [SerializeField] private string fireActionName = "Fire";
        [SerializeField] private string throwActionName = "Throw";
        [SerializeField] private string blockActionName = "Block";
        [SerializeField] private string diveActionName = "Dive";

        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction runAction;
        private InputAction jumpAction;
        private InputAction crouchAction;
        private InputAction interactAction;
        private InputAction armAction;
        private InputAction attackAction;
        private InputAction fireAction;
        private InputAction throwAction;
        private InputAction blockAction;
        private InputAction diveAction;
        private PlatformerCharacterIntent currentIntent;

        public PlatformerCharacterIntent CurrentIntent => currentIntent;

        private void Awake()
        {
            if (playerInput == null && !TryGetComponent(out playerInput))
            {
                Debug.LogError("PlayerPlatformerInputSource: PlayerInput not found.", this);
                enabled = false;
                return;
            }

            CacheActions();
        }

        private void OnEnable()
        {
            if (enableActionsOnEnable && playerInput != null && playerInput.actions != null)
            {
                playerInput.actions.Enable();
            }
        }

        private void OnDisable()
        {
            if (enableActionsOnEnable && playerInput != null && playerInput.actions != null)
            {
                playerInput.actions.Disable();
            }
        }

        private void Update()
        {
            Vector2 move = ReadVector2(moveAction);
            Vector2 look = lookAction != null ? ReadVector2(lookAction) : move;

            currentIntent.Move = Vector2.ClampMagnitude(move, 1f);
            currentIntent.Look = Vector2.ClampMagnitude(look, 1f);
            currentIntent.RunHeld = IsHeld(runAction);
            currentIntent.JumpPressed = WasPressed(jumpAction);
            currentIntent.JumpHeld = IsHeld(jumpAction);
            currentIntent.JumpReleased = WasReleased(jumpAction);
            currentIntent.CrouchHeld = IsHeld(crouchAction) || move.y < -0.55f;
            currentIntent.LookUpHeld = move.y > 0.55f || look.y > 0.55f;
            currentIntent.InteractPressed = WasPressed(interactAction);
            currentIntent.InteractHeld = IsHeld(interactAction);
            currentIntent.ArmPressed = WasPressed(armAction);
            currentIntent.AttackPressed = WasPressed(attackAction);
            currentIntent.FirePressed = WasPressed(fireAction);
            currentIntent.ThrowPressed = WasPressed(throwAction);
            currentIntent.BlockHeld = IsHeld(blockAction);
            currentIntent.DiveHeld = IsHeld(diveAction) || currentIntent.CrouchHeld;
        }

        private void CacheActions()
        {
            moveAction = FindAction(moveActionName);
            lookAction = FindAction(lookActionName);
            runAction = FindAction(runActionName);
            jumpAction = FindAction(jumpActionName);
            crouchAction = FindAction(crouchActionName);
            interactAction = FindAction(interactActionName);
            armAction = FindAction(armActionName);
            attackAction = FindAction(attackActionName);
            fireAction = FindAction(fireActionName);
            throwAction = FindAction(throwActionName);
            blockAction = FindAction(blockActionName);
            diveAction = FindAction(diveActionName);
        }

        private InputAction FindAction(string actionName)
        {
            if (string.IsNullOrWhiteSpace(actionName) || playerInput == null || playerInput.actions == null)
            {
                return null;
            }

            return playerInput.actions.FindAction(actionName, false);
        }

        private static Vector2 ReadVector2(InputAction action)
        {
            return action != null ? action.ReadValue<Vector2>() : Vector2.zero;
        }

        private static bool IsHeld(InputAction action)
        {
            return action != null && action.IsPressed();
        }

        private static bool WasPressed(InputAction action)
        {
            return action != null && action.WasPerformedThisFrame();
        }

        private static bool WasReleased(InputAction action)
        {
            return action != null && action.WasReleasedThisFrame();
        }
    }
}
