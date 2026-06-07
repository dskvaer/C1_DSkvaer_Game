using UnityEngine;

#pragma warning disable 0414

namespace C1.Platformer.Characters {
    [DisallowMultipleComponent]
    public sealed class PlatformerTouchInputSource : MonoBehaviour, IPlatformerInputSource {
        [Header("Описание")]
        [SerializeField, TextArea(3, 7)] private string inspectorDescription =
            "Источник ввода для Android и UI-кнопок. Привяжите методы SetMove, PressJump, SetRun, SetCrouch, PressAttack и другие к виртуальному стику/кнопкам Canvas. Компонент выдаёт те же команды, что и PlayerPlatformerInputSource, поэтому мотор персонажа не зависит от типа управления.";

        [Header("Настройки")]
        [Tooltip("Мёртвая зона виртуального стика. Маленькие случайные касания будут игнорироваться.")]
        [SerializeField, Range(0f, 0.5f)] private float moveDeadZone = 0.12f;
        [Tooltip("Если включено, одноразовые нажатия очищаются в LateUpdate.")]
        [SerializeField] private bool clearOneFrameButtonsInLateUpdate = true;

        private PlatformerCharacterIntent currentIntent;

        public PlatformerCharacterIntent CurrentIntent => currentIntent;

        public void SetMove(Vector2 move)
        {
            currentIntent.Move = move.magnitude >= moveDeadZone ? Vector2.ClampMagnitude(move, 1f) : Vector2.zero;
            currentIntent.Look = currentIntent.Move;
        }

        public void SetLook(Vector2 look)
        {
            currentIntent.Look = look.magnitude >= moveDeadZone ? Vector2.ClampMagnitude(look, 1f) : Vector2.zero;
        }

        public void SetRun(bool held) => currentIntent.RunHeld = held;
        public void SetCrouch(bool held) => currentIntent.CrouchHeld = held;
        public void SetLookUp(bool held) => currentIntent.LookUpHeld = held;
        public void SetInteract(bool held) => currentIntent.InteractHeld = held;
        public void SetBlock(bool held) => currentIntent.BlockHeld = held;
        public void SetDive(bool held) => currentIntent.DiveHeld = held;

        public void PressJump()
        {
            currentIntent.JumpPressed = true;
            currentIntent.JumpHeld = true;
        }

        public void ReleaseJump()
        {
            currentIntent.JumpHeld = false;
            currentIntent.JumpReleased = true;
        }

        public void PressInteract()
        {
            currentIntent.InteractPressed = true;
            currentIntent.InteractHeld = true;
        }

        public void ReleaseInteract()
        {
            currentIntent.InteractHeld = false;
        }

        public void PressArm() => currentIntent.ArmPressed = true;
        public void PressAttack() => currentIntent.AttackPressed = true;
        public void PressFire() => currentIntent.FirePressed = true;
        public void PressThrow() => currentIntent.ThrowPressed = true;

        public void ClearOneFrameButtons()
        {
            currentIntent.ClearOneFrameButtons();
        }

        private void LateUpdate()
        {
            if (clearOneFrameButtonsInLateUpdate)
            {
                ClearOneFrameButtons();
            }
        }
    }
}
