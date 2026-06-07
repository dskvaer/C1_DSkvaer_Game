using UnityEngine;

#pragma warning disable 0414

namespace C1.Platformer.Characters.RPG {
    public enum PlatformerSkillCategory {
        CoreMovement,
        Agility,
        Athletics,
        Interaction,
        Swimming,
        Combat,
        RangedCombat,
        Throwing,
        Survival,
        Social,
        Sailing
    }

    public enum PlatformerSkillKind {
        Passive,
        Active,
        Movement,
        Combat,
        Utility
    }

    public enum PlatformerSkillUsageMetric {
        None,
        DistanceWalked,
        DistanceRun,
        SuccessfulJumps,
        PushDistance,
        EdgeGrabs,
        LadderDistance,
        Interactions,
        SwimDistance,
        DiveTime,
        MeleeHits,
        ShotsFired,
        EnemiesDefeated,
        ThrowsLanded,
        TeacherTraining
    }

    [System.Serializable]
    public sealed class PlatformerSkillStatRequirement {
        public PlatformerCharacterStatId Stat;
        [Min(0)] public int MinimumValue;
        [Min(1)] public int MaxLevelWhenBelow = 1;
        [Min(0)] public int TeacherOverflowLevels = 3;
    }

    [CreateAssetMenu(fileName = "PlatformerSkill", menuName = "C1/Platformer/Characters/RPG/Skill")]
    public sealed class PlatformerSkillDefinition : ScriptableObject {
        [Header("Description")]
        [SerializeField, TextArea(3, 7)] private string inspectorDescription =
            "Data-only skill definition. Runtime progress is stored on PlatformerCharacterProgression.";

        [Header("Identity")]
        [SerializeField] private string skillId = "skill_id";
        [SerializeField] private string displayName = "New Skill";
        [SerializeField, TextArea(2, 5)] private string description;
        [SerializeField] private Sprite icon;

        [Header("Gameplay")]
        [SerializeField] private PlatformerSkillKind kind = PlatformerSkillKind.Passive;
        [SerializeField] private PlatformerSkillCategory category = PlatformerSkillCategory.CoreMovement;
        [SerializeField, Min(0)] private int cost = 1;
        [SerializeField, Min(1)] private int requiredLevel = 1;
        [SerializeField, Min(1)] private int maxSkillLevel = 10;
        [SerializeField] private PlatformerSkillUsageMetric primaryUsageMetric = PlatformerSkillUsageMetric.None;
        [SerializeField, Min(0f)] private float practiceRequiredForFirstLevel = 10f;

        [Header("Requirements")]
        [SerializeField] private PlatformerSkillDefinition[] prerequisites;
        [SerializeField] private PlatformerSkillStatRequirement[] statRequirements;

        [Header("Bonuses")]
        [SerializeField] private PlatformerStatValue[] statBonuses;

        public string SkillId => skillId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public PlatformerSkillKind Kind => kind;
        public PlatformerSkillCategory Category => category;
        public int Cost => cost;
        public int RequiredLevel => requiredLevel;
        public int MaxSkillLevel => maxSkillLevel;
        public PlatformerSkillUsageMetric PrimaryUsageMetric => primaryUsageMetric;
        public float PracticeRequiredForFirstLevel => practiceRequiredForFirstLevel;
        public PlatformerSkillDefinition[] Prerequisites => prerequisites;
        public PlatformerSkillStatRequirement[] StatRequirements => statRequirements;
        public PlatformerStatValue[] StatBonuses => statBonuses;

        public float GetPracticeRequiredForLevel(int targetLevel)
        {
            if (targetLevel <= 0 || practiceRequiredForFirstLevel <= 0f) {
                return 0f;
            }

            return practiceRequiredForFirstLevel * Mathf.Max(1, targetLevel);
        }
    }
}
