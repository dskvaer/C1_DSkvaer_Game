using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStateAnimationsConfig", menuName = "Character/StateAnimationsConfig")]
public class CharacterStateAnimationsConfig : ScriptableObject {
    [SerializeField] private string stateName = "Default";
    [SerializeField] private CharacterAnimation[] animations;
    [SerializeField] private bool canPushObjects;
    [SerializeField] private bool canAttack;

    public string StateName => stateName;
    public bool CanPushObjects => canPushObjects;
    public bool CanAttack => canAttack;

    public AnimationConfig GetAnimation(CharacterAnimationType type, int index = 0)
    {
        foreach (var anim in animations)
        {
            if (anim.Type == type && anim.Configs != null && anim.Configs.Length > index)
            {
                var config = anim.Configs[index];
                Debug.Log($"CharacterStateAnimationsConfig: GetAnimation type={type}, index={index}, Animation={(config.Animation ?? "null")}, IsValid={config.IsValid()}, State={stateName}", this);
                return config;
            }
        }
        Debug.Log($"CharacterStateAnimationsConfig: Animation of type {type} not found in state {stateName}", this);
        return default;
    }

    public bool HasAnimation(CharacterAnimationType type)
    {
        foreach (var anim in animations)
        {
            if (anim.Type == type && anim.Configs != null && anim.Configs.Length > 0 && anim.Configs[0].IsValid())
            {
                return true;
            }
        }
        return false;
    }
}