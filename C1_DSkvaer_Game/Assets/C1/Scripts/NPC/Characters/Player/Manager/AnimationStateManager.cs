using UnityEngine;

public class AnimationStateManager : MonoBehaviour {
    [SerializeField] private CharacterStateAnimationsConfig defaultStateConfig;
    [SerializeField] private CharacterStateAnimationsConfig armedStateConfig;
    private CharacterState currentState = CharacterState.Standart;

    public CharacterState CurrentState => currentState;

    private void Awake()
    {
        if (defaultStateConfig == null || string.IsNullOrEmpty(defaultStateConfig.StateName) || defaultStateConfig.GetAnimation(CharacterAnimationType.Idle).IsValid() == false)
        {
            Debug.LogError("AnimationStateManager: DefaultStateConfig не привязан или недействителен!", this);
            enabled = false;
            return;
        }
        if (armedStateConfig == null || string.IsNullOrEmpty(armedStateConfig.StateName) || armedStateConfig.GetAnimation(CharacterAnimationType.Idle).IsValid() == false)
        {
            Debug.LogError("AnimationStateManager: ArmedStateConfig не привязан или недействителен!", this);
            enabled = false;
            return;
        }
    }

    public AnimationConfig GetAnimation(CharacterAnimationType type, int index = 0)
    {
        var config = GetCurrentStateConfig();
        return config.GetAnimation(type, index);
    }

    public string GetAnimationName(CharacterAnimationType type, int index = 0)
    {
        var animationConfig = GetAnimation(type, index);
        if (animationConfig.IsValid())
        {
            var field = typeof(AnimationConfig).GetField("animation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                return (string)field.GetValue(animationConfig);
            }
            Debug.LogError($"AnimationStateManager: Не удалось получить имя анимации для типа {type}, index {index}", this);
            return null;
        }
        return null;
    }

    public void SetState(CharacterState state)
    {
        currentState = state;
    }

    public CharacterStateAnimationsConfig GetCurrentStateConfig()
    {
        return currentState == CharacterState.Standart ? defaultStateConfig : armedStateConfig;
    }
}

public enum CharacterState {
    Standart,
    Armed
}