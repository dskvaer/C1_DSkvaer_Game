using UnityEngine;
using System;

namespace NPC.Characters.Player {
    public interface IArmAnimation {
        void PlayArmedTransition();
        void PlayDisarmedTransition();
        Action<bool> OnTransitionComplete { get; set; }
    }
}