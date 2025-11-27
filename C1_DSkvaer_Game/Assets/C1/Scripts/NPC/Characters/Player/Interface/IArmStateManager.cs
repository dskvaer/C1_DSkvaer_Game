using UnityEngine;


public interface IArmStateManager {
    bool IsArmed { get; }
    bool IsTransitioning { get; }
    void TriggerArmTransition(bool toArmed);
}

