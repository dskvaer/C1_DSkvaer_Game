// Файл: ICharacterState.cs
using Spine.Unity;
using UnityEngine;

public interface ICharacterState {
    AnimationReferenceAsset RunAnimation { get; }
    bool RunLoop { get; }
    float RunSpeed { get; }
    AnimationReferenceAsset IdleAnimation { get; }
    bool IdleLoop { get; }
    float IdleSpeed { get; }
    AnimationReferenceAsset JumpStartAnimation { get; }
    bool JumpStartLoop { get; }
    float JumpStartSpeed { get; }
    AnimationReferenceAsset JumpAirAnimation { get; }
    bool JumpAirLoop { get; }
    float JumpAirSpeed { get; }
    AnimationReferenceAsset JumpLandAnimation { get; }
    bool JumpLandLoop { get; }
    float JumpLandSpeed { get; }

    bool CanPushObjects { get; } // Толкание предметов
    bool CanAttack { get; } // Атака
}