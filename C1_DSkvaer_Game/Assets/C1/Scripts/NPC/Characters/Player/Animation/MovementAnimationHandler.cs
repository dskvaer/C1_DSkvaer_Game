using UnityEngine;

[RequireComponent(typeof(RunAnimation), typeof(IdleAnimation))]
public class MovementAnimationHandler : MonoBehaviour {
    private RunAnimation runAnim;
    private IdleAnimation idleAnim;

    private void Awake()
    {
        runAnim = GetComponent<RunAnimation>();
        idleAnim = GetComponent<IdleAnimation>();

        if (runAnim == null || idleAnim == null)
        {
            Debug.LogError("MovementAnimationHandler: Требуются RunAnimation и IdleAnimation!");
            enabled = false;
        }
    }

    public void PlayRun()
    {
        if (runAnim != null) runAnim.Play();
    }

    public void PlayIdle()
    {
        if (idleAnim != null) idleAnim.Play();
    }
}
