using UnityEngine;

public interface IArmInputHandler {
    void Initialize(IArmStateManager stateManager);
    void ArmHandler();
}

