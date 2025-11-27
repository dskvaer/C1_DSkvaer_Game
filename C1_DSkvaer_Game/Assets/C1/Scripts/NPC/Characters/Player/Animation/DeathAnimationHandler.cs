using UnityEngine;
using NPC.Characters.Player;

[RequireComponent(typeof(HealthManager))]
public class DeathAnimationHandler : MonoBehaviour, IDeathAnimation {
    [SerializeField] private SpineAnimationController animationController;
    [SerializeField] private AnimationStateManager animationStateManager;

    private HealthManager health;

    private void Awake()
    {
        health = GetComponent<HealthManager>();

        if (animationController == null)
            animationController = GetComponent<SpineAnimationController>();

        if (animationStateManager == null)
            animationStateManager = GetComponent<AnimationStateManager>();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDeath.AddListener(PlayDeathAnimation);
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDeath.RemoveListener(PlayDeathAnimation);
        }
    }

    public void PlayDeathAnimation()
    {
        if (animationController == null || animationStateManager == null)
        {
            Debug.LogError("DeathAnimationHandler: Missing components!");
            return;
        }

        var deathAnim = animationStateManager.GetAnimation(CharacterAnimationType.Death);
        if (deathAnim != null && deathAnim.IsValid())
        {
            animationController.PlayAnimation(deathAnim.Animation, false, deathAnim.Speed);
            Debug.Log("DeathAnimationHandler: Playing death animation: " + deathAnim.Animation);
        }
        else
        {
            Debug.LogWarning("DeathAnimationHandler: Death animation not found!");
        }
    }
}
