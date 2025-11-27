using UnityEngine;

namespace NPC.Characters.Player {
    [CreateAssetMenu(fileName = "AttackSettings", menuName = "Character/AttackSettings", order = 1)]
    public class AttackSettings : ScriptableObject {
        [SerializeField] private float attackStepForce = 5f;
        [SerializeField] private float attackZoneOffsetX = 0.5f;
        [SerializeField] private bool enableCombo = false;

        public float AttackStepForce => attackStepForce;
        public float AttackZoneOffsetX => attackZoneOffsetX;
        public bool EnableCombo => enableCombo;
    }
}