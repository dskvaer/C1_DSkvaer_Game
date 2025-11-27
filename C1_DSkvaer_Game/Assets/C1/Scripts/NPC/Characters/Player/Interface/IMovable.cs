using UnityEngine;

namespace NPC.Characters.Player {
    public interface IMovable {
        void Move(Vector2 input);
        void SetSpeed(float speed);
        Vector2 MoveInput { get; }
        Rigidbody2D Rigidbody { get; }
    }
}