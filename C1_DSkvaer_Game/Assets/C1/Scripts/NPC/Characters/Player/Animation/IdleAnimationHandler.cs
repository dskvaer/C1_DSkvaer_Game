using UnityEngine;
using Spine.Unity;

namespace NPC.Characters.Player {
    [RequireComponent(typeof(SpineAnimationController), typeof(AnimationStateManager))]
    public class IdleAnimationHandler : MonoBehaviour, IAnimation {
        [SerializeField] private SpineAnimationController spineController;
        [SerializeField] private AnimationStateManager animationStateManager;

        private void Awake()
        {
            if (spineController == null && !TryGetComponent(out spineController))
            {
                Debug.LogError("IdleAnimationHandler: SpineAnimationController not found!", this);
                enabled = false;
                return;
            }

            if (animationStateManager == null && !TryGetComponent(out animationStateManager))
            {
                Debug.LogError("IdleAnimationHandler: AnimationStateManager not found!", this);
                enabled = false;
                return;
            }

            var config = animationStateManager.GetCurrentStateConfig();
            if (config == null)
            {
                Debug.LogError("IdleAnimationHandler: Current state config not found in AnimationStateManager!", this);
                enabled = false;
                return;
            }

            var idleAnim = config.GetAnimation(CharacterAnimationType.Idle);
            if (idleAnim == null || !idleAnim.IsValid())
            {
                Debug.LogError($"IdleAnimationHandler: Idle animation not found or invalid in state {config.StateName}!", this);
                enabled = false;
                return;
            }

            Debug.Log($"IdleAnimationHandler: Initialized for state {config.StateName}, Idle animation: {idleAnim.Animation}", this);
        }

        public void Play()
        {
            if (!enabled || spineController == null || animationStateManager == null)
            {
                Debug.LogError("IdleAnimationHandler: Component disabled or SpineAnimationController/AnimationStateManager not initialized!", this);
                return;
            }

            var config = animationStateManager.GetCurrentStateConfig();
            if (config == null)
            {
                Debug.LogError("IdleAnimationHandler: Current state config not found!", this);
                return;
            }

            var idleAnim = config.GetAnimation(CharacterAnimationType.Idle);
            if (idleAnim == null || !idleAnim.IsValid())
            {
                Debug.LogError($"IdleAnimationHandler: Cannot play Idle, animation invalid in state {config.StateName}!", this);
                return;
            }

            spineController.PlayAnimation(idleAnim.Animation, idleAnim.Loop, idleAnim.Speed);
            Debug.Log($"IdleAnimationHandler: Playing Idle animation: {idleAnim.Animation}, State: {config.StateName}", this);
        }
    }
}