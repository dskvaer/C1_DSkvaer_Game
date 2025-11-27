using UnityEngine;

namespace NPC.Characters.Player {
    public interface IAttackAnimation : IAnimation {
        void PlayAttack(int comboCount, float attackSpeedMultiplier);
        float GetAttackDuration(int comboCount, float attackSpeedMultiplier);
        System.Action<int> OnAttackComplete { get; set; }
    }
}