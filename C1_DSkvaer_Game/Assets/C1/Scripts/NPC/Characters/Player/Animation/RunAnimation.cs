using UnityEngine;
using NPC.Characters.Player;

[RequireComponent(typeof(SpineAnimationController), typeof(AnimationStateManager))]
public class RunAnimation : MonoBehaviour, IAnimation {
    private SpineAnimationController spineController;
    private AnimationStateManager animationStateManager;

    private void Awake()
    {
        if (!TryGetComponent(out spineController))
        {
            Debug.LogError("RunAnimation: SpineAnimationController не найден!", this);
            enabled = false;
            return;
        }

        if (!TryGetComponent(out animationStateManager))
        {
            Debug.LogError("RunAnimation: AnimationStateManager не найден!", this);
            enabled = false;
            return;
        }

        var runAnim = animationStateManager.GetAnimation(CharacterAnimationType.Run);
        if (!runAnim.IsValid())
        {
            Debug.LogError($"RunAnimation: Run не найден или невалиден! State: {animationStateManager.CurrentState}");
            enabled = false;
            return;
        }

        Debug.Log($"RunAnimation: Инициализирован. Run: {runAnim.Animation}, Loop={runAnim.Loop}, Speed={runAnim.Speed}");
    }

    public void Play()
    {
        if (!enabled) return;

        var runAnim = animationStateManager.GetAnimation(CharacterAnimationType.Run);
        if (!runAnim.IsValid()) return;

        if (spineController.CurrentAnimation != runAnim.Animation)
        {
            spineController.PlayAnimation(runAnim.Animation, runAnim.Loop, runAnim.Speed);
            Debug.Log($"RunAnimation: Воспроизведён Run: {runAnim.Animation}");
        }
    }
}
