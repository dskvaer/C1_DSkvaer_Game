using UnityEngine;

public interface IJump {
    // Инициализация параметров прыжка
    void Initialize(float jumpForce, float jumpDelay, float groundCheckDistance);
    // Выполнение прыжка
    void Jump();
    // Проверка, находится ли персонаж на земле
    bool IsGrounded();
    // Проверка, выполняется ли прыжок
    bool IsJumping();
}