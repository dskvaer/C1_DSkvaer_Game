using UnityEngine;
using UnityEngine.InputSystem;
using Spine.Unity;
using NPC.Characters.Player;

public class DebugManager : MonoBehaviour {
    [SerializeField] private PlayerController playerController;
    [SerializeField] private SpineAnimationController spineController;
    [SerializeField] private PlayerAnimationManager animationManager;
    [SerializeField] private AnimationStateManager animationStateManager;

    private void Awake()
    {
        CheckComponents();
        CheckInputActions();
        CheckAnimations();
    }

    private void Start()
    {
        CheckPhysics();
    }

    private void CheckComponents()
    {
        if (playerController == null)
        {
            Debug.LogError("PlayerController не привязан в DebugManager", this);
            return;
        }
        if (spineController == null)
        {
            Debug.LogError("SpineAnimationController не привязан в DebugManager", this);
        }
        if (animationManager == null)
        {
            Debug.LogError("PlayerAnimationManager не привязан в DebugManager", this);
        }
        if (animationStateManager == null)
        {
            Debug.LogError("AnimationStateManager не привязан в DebugManager", this);
            return;
        }

        GameObject player = playerController.gameObject;
        if (!player.TryGetComponent<Rigidbody2D>(out _))
        {
            Debug.LogError("Rigidbody2D отсутствует на объекте " + player.name, player);
        }
        if (!player.TryGetComponent<PlayerInput>(out _))
        {
            Debug.LogError("PlayerInput отсутствует на объекте " + player.name, player);
        }
        if (!player.TryGetComponent<SkeletonAnimation>(out SkeletonAnimation skeleton))
        {
            Debug.LogError("SkeletonAnimation отсутствует на объекте " + player.name, player);
        }
        else if (skeleton.SkeletonDataAsset == null)
        {
            Debug.LogError("SkeletonDataAsset равен null в SkeletonAnimation", skeleton);
        }
        if (!player.TryGetComponent<JumpStateController>(out _))
        {
            Debug.LogError("JumpStateController отсутствует на объекте " + player.name, player);
        }
    }

    private void CheckInputActions()
    {
        if (playerController == null) return;

        PlayerInput playerInput = playerController.GetComponent<PlayerInput>();
        if (playerInput == null || playerInput.actions == null)
        {
            Debug.LogError("PlayerInput или его actions равны null", this);
            return;
        }

        if (playerInput.actions.FindAction("Move") == null)
        {
            Debug.LogError("Действие 'Move' не найдено в Input Action Asset", this);
        }

        if (playerInput.actions.FindAction("Jump") == null)
        {
            Debug.LogError("Действие 'Jump' не найдено в Input Action Asset", this);
        }

        if (playerInput.actions.FindAction("Arm") == null)
        {
            Debug.LogError("Действие 'Arm' не найдено в Input Action Asset", this);
        }
    }

    private void CheckAnimations()
    {
        if (animationStateManager == null)
        {
            Debug.LogError("AnimationStateManager не назначен", this);
            return;
        }

        var config = animationStateManager.GetCurrentStateConfig();
        if (config == null)
        {
            Debug.LogError("Текущая конфигурация анимаций не назначена в AnimationStateManager", this);
            return;
        }

        CheckAnimation(config, CharacterAnimationType.Idle, "IdleAnimation");
        CheckAnimation(config, CharacterAnimationType.Run, "RunAnimation");
        CheckAnimation(config, CharacterAnimationType.Jump, "JumpStartAnimation", 0);
        CheckAnimation(config, CharacterAnimationType.Jump, "JumpAirAnimation", 1);
        CheckAnimation(config, CharacterAnimationType.Jump, "JumpLandAnimation", 2);
        CheckAnimation(config, CharacterAnimationType.ArmedTransition, "ArmedTransition");
        CheckAnimation(config, CharacterAnimationType.DisarmedTransition, "DisarmedTransition");
        CheckAnimation(config, CharacterAnimationType.EdgeBalance, "EdgeBalanceAnimation");
    }

    private void CheckAnimation(CharacterStateAnimationsConfig config, CharacterAnimationType type, string name, int index = 0)
    {
        if (type == CharacterAnimationType.DisarmedTransition && config.StateName == "Standart")
        {
            Debug.LogWarning($"Пропущена проверка {name}, так как она необязательна для состояния {config.StateName}", this);
            return;
        }
        var anim = config.GetAnimation(type, index);
        if (!anim.IsValid())
        {
            Debug.LogError($"{name} не назначена в CharacterStateAnimationsConfig (State: {config.StateName})", this);
        }
        else
        {
            Debug.Log($"DebugManager: Анимация {name} валидна: {anim.Animation}", this);
        }
    }

    private void CheckPhysics()
    {
        if (playerController == null) return;

        if (!playerController.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            Debug.LogError("Rigidbody2D отсутствует на объекте " + playerController.name, playerController);
            return;
        }

        if (rb.constraints.HasFlag(RigidbodyConstraints2D.FreezePositionX))
        {
            Debug.LogError("Rigidbody2D заморожен по оси X, движение невозможно", rb);
        }
        if (rb.constraints.HasFlag(RigidbodyConstraints2D.FreezePositionY))
        {
            Debug.LogError("Rigidbody2D заморожен по оси Y, прыжок невозможен", rb);
        }
        if (rb.mass <= 0)
        {
            Debug.LogError("Rigidbody2D имеет нулевую или отрицательную массу", rb);
        }
        if (rb.gravityScale <= 0)
        {
            Debug.LogError("Rigidbody2D имеет нулевую или отрицательную gravityScale, прыжок невозможен", rb);
        }
        if (playerController.Jump == null)
        {
            Debug.LogError("PlayerJump не назначен в PlayerController", playerController);
        }
    }
}