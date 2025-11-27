using System;
using UnityEngine;

public interface IArmTransitionAnimation : IAnimation {
    void PlayArmedTransition();
    void PlayDisarmedTransition();
    event Action<bool> OnTransitionComplete;
}