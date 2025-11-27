using UnityEngine;

namespace NPC.Characters.Player {
    [System.Serializable]
    public struct AnimationComponent {
        [SerializeField] private readonly CharacterAnimationType type;
        [SerializeField] private readonly MonoBehaviour component;

        public CharacterAnimationType Type => type;
        public MonoBehaviour Component => component;
    }
}