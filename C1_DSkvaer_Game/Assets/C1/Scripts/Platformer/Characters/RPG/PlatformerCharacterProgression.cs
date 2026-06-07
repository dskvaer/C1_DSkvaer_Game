using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#pragma warning disable 0414

namespace C1.Platformer.Characters.RPG {
    [Serializable]
    public sealed class PlatformerSkillRuntimeState {
        [SerializeField] private string skillId;
        [SerializeField, Min(0)] private int level;
        [SerializeField, Min(0f)] private float practice;
        [SerializeField, Min(0)] private int teacherLevels;

        public string SkillId => skillId;
        public int Level => level;
        public float Practice => practice;
        public int TeacherLevels => teacherLevels;

        public PlatformerSkillRuntimeState(string skillId, int level)
        {
            this.skillId = skillId;
            this.level = Mathf.Max(0, level);
        }

        public void SetLevel(int value)
        {
            level = Mathf.Max(0, value);
        }

        public void AddPractice(float amount)
        {
            if (amount > 0f) {
                practice += amount;
            }
        }

        public void SpendPractice(float amount)
        {
            practice = Mathf.Max(0f, practice - Mathf.Max(0f, amount));
        }

        public void AddTeacherLevel()
        {
            teacherLevels++;
        }
    }

    [DisallowMultipleComponent]
    public sealed class PlatformerCharacterProgression : MonoBehaviour {
        [Header("Description")]
        [SerializeField, TextArea(4, 9)] private string inspectorDescription =
            "RPG progression for the platformer player. Skill definitions are ScriptableObjects; runtime skill levels and practice are stored here.";

        [Header("Class and skill trees")]
        [SerializeField] private PlatformerCharacterClassDefinition characterClass;
        [SerializeField] private PlatformerSkillTreeDefinition[] skillTrees;

        [Header("Progression")]
        [SerializeField, Min(1)] private int level = 1;
        [SerializeField, Min(0)] private int experience;
        [SerializeField, Min(0)] private int skillPoints;
        [SerializeField] private PlatformerStatValue[] baseStats = Array.Empty<PlatformerStatValue>();
        [SerializeField] private List<string> unlockedSkillIds = new List<string>();
        [SerializeField] private List<PlatformerSkillRuntimeState> skillStates = new List<PlatformerSkillRuntimeState>();

        public int Level => level;
        public int Experience => experience;
        public int SkillPoints => skillPoints;
        public PlatformerCharacterClassDefinition CharacterClass => characterClass;
        public IReadOnlyList<string> UnlockedSkillIds => unlockedSkillIds;
        public IReadOnlyList<PlatformerSkillRuntimeState> SkillStates => skillStates;
        public IReadOnlyList<PlatformerSkillTreeDefinition> SkillTrees => skillTrees;

        public UnityEvent OnProgressionChanged { get; } = new UnityEvent();
        public UnityEvent OnSkillUnlocked { get; } = new UnityEvent();

        private void Awake()
        {
            if (characterClass != null && (baseStats == null || baseStats.Length == 0)) {
                ApplyClass(characterClass);
            }
        }

        public void ApplyClass(PlatformerCharacterClassDefinition newClass)
        {
            characterClass = newClass;
            if (characterClass == null) {
                return;
            }

            baseStats = CloneStats(characterClass.BaseStats);
            PlatformerSkillDefinition[] startingSkills = characterClass.StartingSkills;
            for (int i = 0; startingSkills != null && i < startingSkills.Length; i++) {
                UnlockSkillInternal(startingSkills[i], false);
            }

            OnProgressionChanged.Invoke();
        }

        public void ApplyLegacyCharacterCreationPrefs()
        {
            SetBaseStat(PlatformerCharacterStatId.Strength, PlayerPrefs.GetInt("Strength", GetBaseStat(PlatformerCharacterStatId.Strength)));
            SetBaseStat(PlatformerCharacterStatId.Dexterity, PlayerPrefs.GetInt("Dexterity", GetBaseStat(PlatformerCharacterStatId.Dexterity)));
            SetBaseStat(PlatformerCharacterStatId.Vitality, PlayerPrefs.GetInt("Vitality", GetBaseStat(PlatformerCharacterStatId.Vitality)));
            SetBaseStat(PlatformerCharacterStatId.Stamina, PlayerPrefs.GetInt("Stamina", GetBaseStat(PlatformerCharacterStatId.Stamina)));
            SetBaseStat(PlatformerCharacterStatId.Charm, PlayerPrefs.GetInt("Charm", GetBaseStat(PlatformerCharacterStatId.Charm)));
            SetBaseStat(PlatformerCharacterStatId.Luck, PlayerPrefs.GetInt("Luck", GetBaseStat(PlatformerCharacterStatId.Luck)));
            SetBaseStat(PlatformerCharacterStatId.Perception, PlayerPrefs.GetInt("Perception", GetBaseStat(PlatformerCharacterStatId.Perception)));
            OnProgressionChanged.Invoke();
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0) {
                return;
            }

            experience += amount;
            OnProgressionChanged.Invoke();
        }

        public void AddSkillPoints(int amount)
        {
            if (amount <= 0) {
                return;
            }

            skillPoints += amount;
            OnProgressionChanged.Invoke();
        }

        public bool CanUnlock(PlatformerSkillDefinition skill)
        {
            if (skill == null || unlockedSkillIds.Contains(skill.SkillId) || level < skill.RequiredLevel || skillPoints < skill.Cost) {
                return false;
            }

            return HasPrerequisites(skill);
        }

        public bool TryUnlockSkill(PlatformerSkillDefinition skill)
        {
            if (!CanUnlock(skill)) {
                return false;
            }

            skillPoints -= skill.Cost;
            UnlockSkillInternal(skill, true);
            return true;
        }

        public bool RegisterSkillUsage(PlatformerSkillUsageMetric metric, float amount = 1f)
        {
            if (metric == PlatformerSkillUsageMetric.None || amount <= 0f) {
                return false;
            }

            bool changed = false;
            for (int i = 0; skillTrees != null && i < skillTrees.Length; i++) {
                PlatformerSkillTreeNode[] nodes = skillTrees[i] != null ? skillTrees[i].Nodes : null;
                for (int j = 0; nodes != null && j < nodes.Length; j++) {
                    PlatformerSkillDefinition skill = nodes[j].Skill;
                    if (skill == null || skill.PrimaryUsageMetric != metric || !HasPrerequisites(skill)) {
                        continue;
                    }

                    changed |= AddPractice(skill, amount, false);
                }
            }

            if (changed) {
                OnProgressionChanged.Invoke();
            }

            return changed;
        }

        public bool TrainSkillWithTeacher(PlatformerSkillDefinition skill, int levels = 1)
        {
            if (skill == null || levels <= 0 || !HasPrerequisites(skill)) {
                return false;
            }

            PlatformerSkillRuntimeState state = GetOrCreateSkillState(skill.SkillId);
            bool changed = false;
            for (int i = 0; i < levels; i++) {
                int nextLevel = Mathf.Max(1, state.Level + 1);
                if (nextLevel > GetAllowedSkillLevel(skill, true)) {
                    break;
                }

                state.SetLevel(nextLevel);
                state.AddTeacherLevel();
                EnsureUnlocked(skill);
                changed = true;
            }

            if (changed) {
                OnSkillUnlocked.Invoke();
                OnProgressionChanged.Invoke();
            }

            return changed;
        }

        public PlatformerSkillDefinition FindSkillDefinition(string skillId)
        {
            if (string.IsNullOrWhiteSpace(skillId)) {
                return null;
            }

            for (int i = 0; skillTrees != null && i < skillTrees.Length; i++) {
                PlatformerSkillTreeNode[] nodes = skillTrees[i] != null ? skillTrees[i].Nodes : null;
                for (int j = 0; nodes != null && j < nodes.Length; j++) {
                    PlatformerSkillDefinition skill = nodes[j].Skill;
                    if (skill != null && skill.SkillId == skillId) {
                        return skill;
                    }
                }
            }

            return null;
        }

        public int GetSkillLevel(string skillId)
        {
            PlatformerSkillRuntimeState state = FindSkillState(skillId);
            return state != null ? state.Level : 0;
        }

        public float GetSkillPractice(string skillId)
        {
            PlatformerSkillRuntimeState state = FindSkillState(skillId);
            return state != null ? state.Practice : 0f;
        }

        public int GetBaseStat(PlatformerCharacterStatId stat)
        {
            for (int i = 0; baseStats != null && i < baseStats.Length; i++) {
                if (baseStats[i].Stat == stat) {
                    return baseStats[i].Value;
                }
            }

            return 0;
        }

        public int GetTotalStat(PlatformerCharacterStatId stat)
        {
            int value = GetBaseStat(stat);
            for (int i = 0; skillTrees != null && i < skillTrees.Length; i++) {
                PlatformerSkillTreeNode[] nodes = skillTrees[i] != null ? skillTrees[i].Nodes : null;
                for (int j = 0; nodes != null && j < nodes.Length; j++) {
                    PlatformerSkillDefinition skill = nodes[j].Skill;
                    if (skill == null || GetSkillLevel(skill.SkillId) <= 0) {
                        continue;
                    }

                    PlatformerStatValue[] bonuses = skill.StatBonuses;
                    for (int k = 0; bonuses != null && k < bonuses.Length; k++) {
                        if (bonuses[k].Stat == stat) {
                            value += bonuses[k].Value;
                        }
                    }
                }
            }

            return value;
        }

        private bool AddPractice(PlatformerSkillDefinition skill, float amount, bool allowTeacherOverflow)
        {
            PlatformerSkillRuntimeState state = GetOrCreateSkillState(skill.SkillId);
            state.AddPractice(amount);

            while (state.Level < GetAllowedSkillLevel(skill, allowTeacherOverflow)) {
                int targetLevel = Mathf.Max(1, state.Level + 1);
                float required = skill.GetPracticeRequiredForLevel(targetLevel);
                if (required <= 0f || state.Practice < required) {
                    break;
                }

                state.SpendPractice(required);
                state.SetLevel(targetLevel);
                EnsureUnlocked(skill);
            }

            return true;
        }

        private int GetAllowedSkillLevel(PlatformerSkillDefinition skill, bool allowTeacherOverflow)
        {
            int cap = skill != null ? skill.MaxSkillLevel : 0;
            PlatformerSkillStatRequirement[] requirements = skill != null ? skill.StatRequirements : null;
            for (int i = 0; requirements != null && i < requirements.Length; i++) {
                PlatformerSkillStatRequirement requirement = requirements[i];
                if (requirement == null || GetTotalStat(requirement.Stat) >= requirement.MinimumValue) {
                    continue;
                }

                int requirementCap = requirement.MaxLevelWhenBelow;
                if (allowTeacherOverflow) {
                    requirementCap += requirement.TeacherOverflowLevels;
                }

                cap = Mathf.Min(cap, requirementCap);
            }

            return Mathf.Max(0, cap);
        }

        private bool HasPrerequisites(PlatformerSkillDefinition skill)
        {
            PlatformerSkillDefinition[] prerequisites = skill != null ? skill.Prerequisites : null;
            for (int i = 0; prerequisites != null && i < prerequisites.Length; i++) {
                if (prerequisites[i] != null && GetSkillLevel(prerequisites[i].SkillId) <= 0) {
                    return false;
                }
            }

            return true;
        }

        private void EnsureUnlocked(PlatformerSkillDefinition skill)
        {
            if (skill != null && !unlockedSkillIds.Contains(skill.SkillId)) {
                unlockedSkillIds.Add(skill.SkillId);
            }
        }

        private void SetBaseStat(PlatformerCharacterStatId stat, int value)
        {
            if (baseStats == null || baseStats.Length == 0) {
                baseStats = new PlatformerStatValue[Enum.GetValues(typeof(PlatformerCharacterStatId)).Length];
                for (int i = 0; i < baseStats.Length; i++) {
                    baseStats[i] = new PlatformerStatValue { Stat = (PlatformerCharacterStatId)i, Value = 0 };
                }
            }

            for (int i = 0; i < baseStats.Length; i++) {
                if (baseStats[i].Stat == stat) {
                    baseStats[i].Value = value;
                    return;
                }
            }
        }

        private void UnlockSkillInternal(PlatformerSkillDefinition skill, bool invokeEvents)
        {
            if (skill == null) {
                return;
            }

            PlatformerSkillRuntimeState state = GetOrCreateSkillState(skill.SkillId);
            if (state.Level <= 0) {
                state.SetLevel(1);
            }

            EnsureUnlocked(skill);
            if (invokeEvents) {
                OnSkillUnlocked.Invoke();
                OnProgressionChanged.Invoke();
            }
        }

        private PlatformerSkillRuntimeState FindSkillState(string skillId)
        {
            for (int i = 0; skillStates != null && i < skillStates.Count; i++) {
                if (skillStates[i] != null && skillStates[i].SkillId == skillId) {
                    return skillStates[i];
                }
            }

            return null;
        }

        private PlatformerSkillRuntimeState GetOrCreateSkillState(string skillId)
        {
            PlatformerSkillRuntimeState state = FindSkillState(skillId);
            if (state != null) {
                return state;
            }

            state = new PlatformerSkillRuntimeState(skillId, 0);
            skillStates.Add(state);
            return state;
        }

        private static PlatformerStatValue[] CloneStats(PlatformerStatValue[] source)
        {
            if (source == null) {
                return Array.Empty<PlatformerStatValue>();
            }

            PlatformerStatValue[] result = new PlatformerStatValue[source.Length];
            for (int i = 0; i < source.Length; i++) {
                result[i] = new PlatformerStatValue { Stat = source[i].Stat, Value = source[i].Value };
            }

            return result;
        }
    }
}
