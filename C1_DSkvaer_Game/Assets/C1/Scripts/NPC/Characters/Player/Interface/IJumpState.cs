using UnityEngine;

public interface IJumpState {
    bool IsGrounded();
    bool IsJumping();
    bool CanJump();
    bool WasInAir();
    JumpPhase CurrentPhase();
    void SetGrounded(bool grounded);
    void SetJumping(bool jumping);
    void SetCanJump(bool canJump);
    void SetWasInAir(bool wasInAir);
    void SetFailedJump(bool failed);
    void SetCurrentPhase(JumpPhase phase);
    bool CheckGround(Vector2 position, float distance, LayerMask groundLayer, LayerMask pushableLayer);
    bool IsLocked { get; } // Добавлено свойство IsLocked
    void SetPreJumpVelocity(float velocityX); // Добавлено для сохранения горизонтальной скорости
}