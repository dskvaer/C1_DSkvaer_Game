using System.Collections.Generic;
using C1.Platformer.Characters.RPG;
using UnityEditor;
using UnityEngine;

namespace C1.EditorTools {
    public static class PlayerSkillPackBuilder {
        private const string SettingsFolder = "Assets/C1/Prefabs/Player/Platformer/Settings";
        private const string SkillsFolder = SettingsFolder + "/Skills";
        private const string DefaultTreePath = SettingsFolder + "/Hero_CommonSkillTree.asset";
        private const string DefaultClassPath = SettingsFolder + "/Hero_DefaultClass.asset";

        [MenuItem("C1/Player/Create Base RPG Skill Pack")]
        public static void CreateBaseRpgSkillPack()
        {
            EnsureFolder("Assets/C1/Prefabs");
            EnsureFolder("Assets/C1/Prefabs/Player");
            EnsureFolder("Assets/C1/Prefabs/Player/Platformer");
            EnsureFolder(SettingsFolder);
            EnsureFolder(SkillsFolder);

            Dictionary<string, PlatformerSkillDefinition> skills = new Dictionary<string, PlatformerSkillDefinition>();
            SkillSeed[] seeds = CreateSeeds();
            for (int i = 0; i < seeds.Length; i++) {
                SkillSeed seed = seeds[i];
                PlatformerSkillDefinition skill = LoadOrCreateSkill(seed);
                skills[seed.Id] = skill;
            }

            for (int i = 0; i < seeds.Length; i++) {
                ApplySkillSeed(skills[seeds[i].Id], seeds[i], skills);
            }

            PlatformerSkillTreeDefinition tree = LoadOrCreateAsset<PlatformerSkillTreeDefinition>(DefaultTreePath);
            ApplyTree(tree, seeds, skills);

            PlatformerCharacterClassDefinition defaultClass = LoadOrCreateAsset<PlatformerCharacterClassDefinition>(DefaultClassPath);
            ApplyDefaultClass(defaultClass, skills);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[PlayerSkillPackBuilder] Base RPG skill pack created: {seeds.Length} skills.");
        }

        private static SkillSeed[] CreateSeeds()
        {
            return new[] {
                new SkillSeed("skill.walking", "Walking", "Basic control of calm movement on platforms and in towns.", PlatformerSkillKind.Movement, PlatformerSkillCategory.CoreMovement, PlatformerSkillUsageMetric.DistanceWalked, 20f, new Vector2(-600f, 120f)),
                new SkillSeed("skill.running", "Running", "Faster ground movement, sprint control, and safer stopping.", PlatformerSkillKind.Movement, PlatformerSkillCategory.CoreMovement, PlatformerSkillUsageMetric.DistanceRun, 35f, new Vector2(-420f, 120f), "skill.walking")
                    .Require(PlatformerCharacterStatId.Stamina, 2, 4),
                new SkillSeed("skill.jumping", "Jumping", "Stable jumps, landing control, and basic aerial movement.", PlatformerSkillKind.Movement, PlatformerSkillCategory.Agility, PlatformerSkillUsageMetric.SuccessfulJumps, 18f, new Vector2(-240f, 160f), "skill.running")
                    .Require(PlatformerCharacterStatId.Dexterity, 2, 4),
                new SkillSeed("skill.crouching", "Crouching", "Low movement, ducking under hazards, and preparing stealth routes.", PlatformerSkillKind.Movement, PlatformerSkillCategory.Agility, PlatformerSkillUsageMetric.None, 10f, new Vector2(-240f, 40f), "skill.walking"),
                new SkillSeed("skill.look_up", "Looking Up", "Awareness for vertical routes, ledges, ladders, and overhead threats.", PlatformerSkillKind.Utility, PlatformerSkillCategory.Survival, PlatformerSkillUsageMetric.None, 8f, new Vector2(-600f, -40f))
                    .Require(PlatformerCharacterStatId.Perception, 1, 5),
                new SkillSeed("skill.pushing", "Pushing", "Moving crates, heavy props, and blocked objects.", PlatformerSkillKind.Movement, PlatformerSkillCategory.Athletics, PlatformerSkillUsageMetric.PushDistance, 25f, new Vector2(-60f, -80f), "skill.walking")
                    .Require(PlatformerCharacterStatId.Strength, 2, 4),
                new SkillSeed("skill.edge_hanging", "Edge Hanging", "Hanging on cliffs and platform edges without falling.", PlatformerSkillKind.Movement, PlatformerSkillCategory.Agility, PlatformerSkillUsageMetric.EdgeGrabs, 20f, new Vector2(-60f, 220f), "skill.jumping")
                    .Require(PlatformerCharacterStatId.Dexterity, 3, 3),
                new SkillSeed("skill.ledge_grab", "Jump Edge Grab", "Catching platform edges during a jump.", PlatformerSkillKind.Movement, PlatformerSkillCategory.Agility, PlatformerSkillUsageMetric.EdgeGrabs, 30f, new Vector2(120f, 220f), "skill.edge_hanging")
                    .Require(PlatformerCharacterStatId.Dexterity, 3, 3),
                new SkillSeed("skill.platform_climb", "Platform Climb", "Pulling yourself from a ledge onto a platform.", PlatformerSkillKind.Movement, PlatformerSkillCategory.Athletics, PlatformerSkillUsageMetric.EdgeGrabs, 35f, new Vector2(300f, 220f), "skill.ledge_grab")
                    .Require(PlatformerCharacterStatId.Strength, 2, 4),
                new SkillSeed("skill.ladder_climbing", "Ladder Climbing", "Moving on ladders and vertical climb routes.", PlatformerSkillKind.Movement, PlatformerSkillCategory.Athletics, PlatformerSkillUsageMetric.LadderDistance, 18f, new Vector2(-60f, 60f), "skill.walking"),
                new SkillSeed("skill.interaction", "Interaction", "Using items, buttons, levers, doors, and world objects.", PlatformerSkillKind.Utility, PlatformerSkillCategory.Interaction, PlatformerSkillUsageMetric.Interactions, 12f, new Vector2(-600f, -200f)),
                new SkillSeed("skill.swimming", "Swimming", "Movement on the water surface and stamina control in water.", PlatformerSkillKind.Movement, PlatformerSkillCategory.Swimming, PlatformerSkillUsageMetric.SwimDistance, 30f, new Vector2(-60f, -260f), "skill.walking")
                    .Require(PlatformerCharacterStatId.Stamina, 2, 4),
                new SkillSeed("skill.diving", "Diving", "Going underwater and staying oriented below the surface.", PlatformerSkillKind.Movement, PlatformerSkillCategory.Swimming, PlatformerSkillUsageMetric.DiveTime, 45f, new Vector2(120f, -260f), "skill.swimming")
                    .Require(PlatformerCharacterStatId.Stamina, 3, 3),
                new SkillSeed("skill.weapon_handling", "Weapon Handling", "Drawing, hiding, and safely carrying weapons.", PlatformerSkillKind.Utility, PlatformerSkillCategory.Combat, PlatformerSkillUsageMetric.None, 12f, new Vector2(-600f, -380f)),
                new SkillSeed("skill.melee", "Melee Attack", "Close combat attacks, timing, and control of short weapons.", PlatformerSkillKind.Combat, PlatformerSkillCategory.Combat, PlatformerSkillUsageMetric.MeleeHits, 28f, new Vector2(-420f, -380f), "skill.weapon_handling")
                    .Require(PlatformerCharacterStatId.Strength, 2, 4),
                new SkillSeed("skill.shooting", "Shooting", "Ranged weapon accuracy, reload rhythm, and target focus.", PlatformerSkillKind.Combat, PlatformerSkillCategory.RangedCombat, PlatformerSkillUsageMetric.ShotsFired, 40f, new Vector2(-240f, -380f), "skill.weapon_handling")
                    .Require(PlatformerCharacterStatId.Perception, 3, 3),
                new SkillSeed("skill.throwing", "Throwing", "Throwing stones, bombs, and improvised objects at enemies.", PlatformerSkillKind.Combat, PlatformerSkillCategory.Throwing, PlatformerSkillUsageMetric.ThrowsLanded, 32f, new Vector2(-60f, -380f), "skill.weapon_handling")
                    .Require(PlatformerCharacterStatId.Dexterity, 2, 4),
                new SkillSeed("skill.sailing", "Sailing", "Top-down ship handling, navigation habits, and sea travel mastery.", PlatformerSkillKind.Utility, PlatformerSkillCategory.Sailing, PlatformerSkillUsageMetric.DistanceWalked, 50f, new Vector2(120f, -520f))
                    .Require(PlatformerCharacterStatId.Perception, 2, 4)
            };
        }

        private static PlatformerSkillDefinition LoadOrCreateSkill(SkillSeed seed)
        {
            string path = $"{SkillsFolder}/{seed.AssetName}.asset";
            return LoadOrCreateAsset<PlatformerSkillDefinition>(path);
        }

        private static void ApplySkillSeed(PlatformerSkillDefinition skill, SkillSeed seed, Dictionary<string, PlatformerSkillDefinition> skills)
        {
            SerializedObject serialized = new SerializedObject(skill);
            SetString(serialized, "skillId", seed.Id);
            SetString(serialized, "displayName", seed.DisplayName);
            SetString(serialized, "description", seed.Description);
            SetEnum(serialized, "kind", (int)seed.Kind);
            SetEnum(serialized, "category", (int)seed.Category);
            SetInt(serialized, "cost", seed.Cost);
            SetInt(serialized, "requiredLevel", seed.RequiredLevel);
            SetInt(serialized, "maxSkillLevel", seed.MaxSkillLevel);
            SetEnum(serialized, "primaryUsageMetric", (int)seed.UsageMetric);
            SetFloat(serialized, "practiceRequiredForFirstLevel", seed.PracticeRequiredForFirstLevel);
            SetObjectArray(serialized, "prerequisites", ResolvePrerequisites(seed, skills));
            SetRequirements(serialized, seed.Requirements);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(skill);
        }

        private static void ApplyTree(PlatformerSkillTreeDefinition tree, SkillSeed[] seeds, Dictionary<string, PlatformerSkillDefinition> skills)
        {
            SerializedObject serialized = new SerializedObject(tree);
            SetString(serialized, "treeId", "hero_common");
            SetString(serialized, "displayName", "Hero Common Skills");

            SerializedProperty nodes = serialized.FindProperty("nodes");
            nodes.arraySize = seeds.Length;
            for (int i = 0; i < seeds.Length; i++) {
                SerializedProperty node = nodes.GetArrayElementAtIndex(i);
                node.FindPropertyRelative("Skill").objectReferenceValue = skills[seeds[i].Id];
                node.FindPropertyRelative("UiPosition").vector2Value = seeds[i].Position;
                SetRelativeObjectArray(node, "Parents", ResolvePrerequisites(seeds[i], skills));
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(tree);
        }

        private static void ApplyDefaultClass(PlatformerCharacterClassDefinition defaultClass, Dictionary<string, PlatformerSkillDefinition> skills)
        {
            SerializedObject serialized = new SerializedObject(defaultClass);
            SetString(serialized, "classId", "Adventurer");
            SetString(serialized, "displayName", "Adventurer");

            SerializedProperty baseStats = serialized.FindProperty("baseStats");
            baseStats.arraySize = 7;
            for (int i = 0; i < 7; i++) {
                SerializedProperty stat = baseStats.GetArrayElementAtIndex(i);
                stat.FindPropertyRelative("Stat").enumValueIndex = i;
                stat.FindPropertyRelative("Value").intValue = 1;
            }

            SetObjectArray(serialized, "startingSkills", new Object[] {
                skills["skill.walking"],
                skills["skill.look_up"],
                skills["skill.interaction"],
                skills["skill.weapon_handling"]
            });

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(defaultClass);
        }

        private static Object[] ResolvePrerequisites(SkillSeed seed, Dictionary<string, PlatformerSkillDefinition> skills)
        {
            Object[] result = new Object[seed.Prerequisites.Length];
            for (int i = 0; i < seed.Prerequisites.Length; i++) {
                result[i] = skills.TryGetValue(seed.Prerequisites[i], out PlatformerSkillDefinition skill) ? skill : null;
            }

            return result;
        }

        private static void SetRequirements(SerializedObject serialized, List<StatRequirementSeed> requirements)
        {
            SerializedProperty property = serialized.FindProperty("statRequirements");
            property.arraySize = requirements.Count;
            for (int i = 0; i < requirements.Count; i++) {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("Stat").enumValueIndex = (int)requirements[i].Stat;
                element.FindPropertyRelative("MinimumValue").intValue = requirements[i].MinimumValue;
                element.FindPropertyRelative("MaxLevelWhenBelow").intValue = requirements[i].MaxLevelWhenBelow;
                element.FindPropertyRelative("TeacherOverflowLevels").intValue = requirements[i].TeacherOverflowLevels;
            }
        }

        private static T LoadOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) {
                return;
            }

            int slash = path.LastIndexOf('/');
            string parent = path.Substring(0, slash);
            string folder = path.Substring(slash + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folder);
        }

        private static void SetString(SerializedObject serialized, string propertyName, string value)
        {
            serialized.FindProperty(propertyName).stringValue = value;
        }

        private static void SetInt(SerializedObject serialized, string propertyName, int value)
        {
            serialized.FindProperty(propertyName).intValue = value;
        }

        private static void SetFloat(SerializedObject serialized, string propertyName, float value)
        {
            serialized.FindProperty(propertyName).floatValue = value;
        }

        private static void SetEnum(SerializedObject serialized, string propertyName, int value)
        {
            serialized.FindProperty(propertyName).enumValueIndex = value;
        }

        private static void SetObjectArray(SerializedObject serialized, string propertyName, Object[] values)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static void SetRelativeObjectArray(SerializedProperty parent, string propertyName, Object[] values)
        {
            SerializedProperty property = parent.FindPropertyRelative(propertyName);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private sealed class SkillSeed {
            public readonly string Id;
            public readonly string AssetName;
            public readonly string DisplayName;
            public readonly string Description;
            public readonly PlatformerSkillKind Kind;
            public readonly PlatformerSkillCategory Category;
            public readonly PlatformerSkillUsageMetric UsageMetric;
            public readonly float PracticeRequiredForFirstLevel;
            public readonly Vector2 Position;
            public readonly string[] Prerequisites;
            public readonly List<StatRequirementSeed> Requirements = new List<StatRequirementSeed>();
            public int Cost = 1;
            public int RequiredLevel = 1;
            public int MaxSkillLevel = 10;

            public SkillSeed(string id, string displayName, string description, PlatformerSkillKind kind, PlatformerSkillCategory category, PlatformerSkillUsageMetric usageMetric, float practiceRequiredForFirstLevel, Vector2 position, params string[] prerequisites)
            {
                Id = id;
                AssetName = id.Replace("skill.", "Skill_").Replace(".", "_");
                DisplayName = displayName;
                Description = description;
                Kind = kind;
                Category = category;
                UsageMetric = usageMetric;
                PracticeRequiredForFirstLevel = practiceRequiredForFirstLevel;
                Position = position;
                Prerequisites = prerequisites ?? new string[0];
            }

            public SkillSeed Require(PlatformerCharacterStatId stat, int minimumValue, int maxLevelWhenBelow, int teacherOverflowLevels = 3)
            {
                Requirements.Add(new StatRequirementSeed(stat, minimumValue, maxLevelWhenBelow, teacherOverflowLevels));
                return this;
            }
        }

        private readonly struct StatRequirementSeed {
            public readonly PlatformerCharacterStatId Stat;
            public readonly int MinimumValue;
            public readonly int MaxLevelWhenBelow;
            public readonly int TeacherOverflowLevels;

            public StatRequirementSeed(PlatformerCharacterStatId stat, int minimumValue, int maxLevelWhenBelow, int teacherOverflowLevels)
            {
                Stat = stat;
                MinimumValue = minimumValue;
                MaxLevelWhenBelow = maxLevelWhenBelow;
                TeacherOverflowLevels = teacherOverflowLevels;
            }
        }
    }
}
