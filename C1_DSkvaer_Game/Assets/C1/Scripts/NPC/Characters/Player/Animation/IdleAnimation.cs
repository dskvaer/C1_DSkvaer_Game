using UnityEngine;
using NPC.Characters.Player;

[RequireComponent(typeof(SpineAnimationController), typeof(AnimationStateManager))]
public class IdleAnimation : MonoBehaviour, IAnimation {
    private SpineAnimationController spineController;
    private AnimationStateManager animationStateManager;

    private void Awake()
    {
        if (!TryGetComponent(out spineController))
        {
            Debug.LogError("IdleAnimation: SpineAnimationController не найден!", this);
            enabled = false;
            return;
        }

        if (!TryGetComponent(out animationStateManager))
        {
            Debug.LogError("IdleAnimation: AnimationStateManager не найден!", this);
            enabled = false;
            return;
        }

        var idleAnim = animationStateManager.GetAnimation(CharacterAnimationType.Idle);
        if (!idleAnim.IsValid())
        {
            Debug.LogError($"IdleAnimation: Idle не найден или невалиден! State: {animationStateManager.CurrentState}");
            enabled = false;
            return;
        }

        Debug.Log($"IdleAnimation: Инициализирован. Idle: {idleAnim.Animation}, Loop={idleAnim.Loop}, Speed={idleAnim.Speed}");
    }

    public void Play()
    {
        if (!enabled) return;

        var idleAnim = animationStateManager.GetAnimation(CharacterAnimationType.Idle);
        if (!idleAnim.IsValid()) return;

        if (spineController.CurrentAnimation != idleAnim.Animation)
        {
            spineController.PlayAnimation(idleAnim.Animation, idleAnim.Loop, idleAnim.Speed);
            Debug.Log($"IdleAnimation: Воспроизведён Idle: {idleAnim.Animation}");
        }
    }
}
