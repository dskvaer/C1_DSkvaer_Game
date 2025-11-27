using System;
using UnityEngine;
using static CharacterStateAnimationsConfig;

[Serializable]
public class CharacterAnimation 
{
    [SerializeField] private CharacterAnimationType type;
    [SerializeField] private AnimationConfig[] configs;

    public CharacterAnimationType Type => type;
    public AnimationConfig[] Configs => configs;
}
