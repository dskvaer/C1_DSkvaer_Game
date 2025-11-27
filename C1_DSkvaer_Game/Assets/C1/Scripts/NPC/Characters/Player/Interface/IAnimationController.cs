using System;
using UnityEngine;

public interface IAnimationController {
    void PlayAnimation(string animation, bool loop, float speed = 1f, int trackIndex = 0, Action onComplete = null);
    void StopAnimation();
    void FlipSkeleton(float direction);
    string CurrentAnimation { get; }
    bool IsLooping { get; }
    bool IsJumpStartOrEnd { get; }
    bool IsOtherAnimationActive();
    Action OnJumpEndComplete { get; set; }
}