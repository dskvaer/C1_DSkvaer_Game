using UnityEngine;
using UnityEngine.InputSystem;

public interface IInputProvider {
    Vector2 GetMoveInput();
    bool IsJumpPressed();
    bool IsAttackPressed();
    bool IsArmPressed();
    bool IsDisarmPressed();
    bool IsBlockPressed();
    bool IsBlockHeld();
    bool IsPushHeld();
    bool IsTestDamagePressed();
}

public class PlayerInputHandler : MonoBehaviour, IInputProvider {
    private PlayerInput inputSystem;

    private void Awake()
    {
        inputSystem = GetComponent<PlayerInput>();
        if (inputSystem == null)
        {
            Debug.LogError("PlayerInputHandler: PlayerInput not found!", this);
            enabled = false;
        }
    }

    public Vector2 GetMoveInput()
    {
        return inputSystem.actions["Move"].ReadValue<Vector2>();
    }

    public bool IsJumpPressed()
    {
        return inputSystem.actions["Jump"].WasPerformedThisFrame();
    }

    public bool IsAttackPressed()
    {
        return inputSystem.actions["Attack"].WasPerformedThisFrame();
    }

    public bool IsArmPressed()
    {
        return inputSystem.actions["Arm"].WasPerformedThisFrame();
    }

    public bool IsDisarmPressed()
    {
        return inputSystem.actions["Arm"].WasPerformedThisFrame();
    }

    public bool IsBlockPressed()
    {
        return inputSystem.actions["Block"].WasPerformedThisFrame();
    }

    public bool IsBlockHeld()
    {
        return inputSystem.actions["Block"].IsPressed();
    }

    public bool IsPushHeld()
    {
        return inputSystem.actions["Interact"].IsPressed();
    }

    public bool IsTestDamagePressed()
    {
        return inputSystem.actions["TestDamage"].WasPerformedThisFrame();
    }
}