using UnityEngine;

namespace C1.Platformer.Characters {
    public enum PlatformerMovementMode {
        Grounded,
        Airborne,
        Crouching,
        Pushing,
        LedgeHang,
        Ladder,
        SwimmingSurface,
        SwimmingUnderwater
    }

    [System.Serializable]
    public struct PlatformerCharacterIntent {
        public Vector2 Move;
        public Vector2 Look;
        public bool RunHeld;
        public bool JumpPressed;
        public bool JumpHeld;
        public bool JumpReleased;
        public bool CrouchHeld;
        public bool LookUpHeld;
        public bool InteractPressed;
        public bool InteractHeld;
        public bool ArmPressed;
        public bool AttackPressed;
        public bool FirePressed;
        public bool ThrowPressed;
        public bool BlockHeld;
        public bool DiveHeld;

        public static PlatformerCharacterIntent Neutral => default;

        public void ClearOneFrameButtons()
        {
            JumpPressed = false;
            JumpReleased = false;
            InteractPressed = false;
            ArmPressed = false;
            AttackPressed = false;
            FirePressed = false;
            ThrowPressed = false;
        }
    }

    public interface IPlatformerInputSource {
        PlatformerCharacterIntent CurrentIntent { get; }
    }

    public sealed class ScriptedPlatformerInputSource : MonoBehaviour, IPlatformerInputSource {
        [SerializeField] private PlatformerCharacterIntent currentIntent;
        [SerializeField] private bool clearOneFrameButtonsInLateUpdate = true;

        public PlatformerCharacterIntent CurrentIntent => currentIntent;

        public void SetIntent(PlatformerCharacterIntent intent)
        {
            currentIntent = intent;
        }

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
