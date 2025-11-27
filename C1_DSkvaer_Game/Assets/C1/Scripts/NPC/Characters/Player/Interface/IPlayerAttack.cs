using UnityEngine;
using UnityEngine.InputSystem;

namespace NPC.Characters.Player {
    public interface IPlayerAttack {
        void Attack(InputAction.CallbackContext context);
        void EndAttack();
        bool IsAttacking { get; }
        void UpdateAttack();
    }
}