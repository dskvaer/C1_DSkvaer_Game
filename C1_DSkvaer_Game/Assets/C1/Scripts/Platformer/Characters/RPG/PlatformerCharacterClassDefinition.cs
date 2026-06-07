using UnityEngine;

#pragma warning disable 0414

namespace C1.Platformer.Characters.RPG {
    [System.Serializable]
    public sealed class PlatformerStatValue {
        [Tooltip("Какой параметр меняем.")]
        public PlatformerCharacterStatId Stat;
        [Tooltip("Значение параметра.")]
        public int Value;
    }

    [CreateAssetMenu(fileName = "PlatformerCharacterClass", menuName = "C1/Platformer/Characters/RPG/Character Class")]
    public sealed class PlatformerCharacterClassDefinition : ScriptableObject {
        [Header("Описание")]
        [SerializeField, TextArea(3, 7)] private string inspectorDescription =
            "Описание класса персонажа для платформерной части. Позже сюда можно привязать стартовые навыки, стартовое оружие, иконку и модификаторы анимаций.";

        [Header("Данные класса")]
        [Tooltip("Внутренний ID класса. Можно связать со старым CharacterCreationManager через PlayerPrefs.")]
        [SerializeField] private string classId = "Adventurer";
        [Tooltip("Название класса, которое показываем игроку.")]
        [SerializeField] private string displayName = "Авантюрист";
        [Tooltip("Описание класса для UI.")]
        [SerializeField, TextArea(2, 5)] private string description;
        [Tooltip("Иконка класса для UI.")]
        [SerializeField] private Sprite icon;

        [Header("Стартовые параметры")]
        [SerializeField] private PlatformerStatValue[] baseStats = {
            new PlatformerStatValue { Stat = PlatformerCharacterStatId.Strength, Value = 1 },
            new PlatformerStatValue { Stat = PlatformerCharacterStatId.Dexterity, Value = 1 },
            new PlatformerStatValue { Stat = PlatformerCharacterStatId.Vitality, Value = 1 },
            new PlatformerStatValue { Stat = PlatformerCharacterStatId.Stamina, Value = 1 },
            new PlatformerStatValue { Stat = PlatformerCharacterStatId.Charm, Value = 1 },
            new PlatformerStatValue { Stat = PlatformerCharacterStatId.Luck, Value = 1 },
            new PlatformerStatValue { Stat = PlatformerCharacterStatId.Perception, Value = 1 }
        };

        [Header("Стартовые навыки")]
        [Tooltip("Навыки, которые персонаж получает сразу после выбора класса.")]
        [SerializeField] private PlatformerSkillDefinition[] startingSkills;

        public string ClassId => classId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public PlatformerStatValue[] BaseStats => baseStats;
        public PlatformerSkillDefinition[] StartingSkills => startingSkills;
    }
}
