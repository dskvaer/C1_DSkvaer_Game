using UnityEngine;

#pragma warning disable 0414

namespace C1.Platformer.Characters.RPG {
    [CreateAssetMenu(fileName = "PlatformerSkillTree", menuName = "C1/Platformer/Characters/RPG/Skill Tree")]
    public sealed class PlatformerSkillTreeDefinition : ScriptableObject {
        [Header("Описание")]
        [SerializeField, TextArea(3, 7)] private string inspectorDescription =
            "Дерево навыков платформерного персонажа. Сейчас это база для будущего UI: список навыков, связи и координаты узлов. Логику отображения можно строить поверх этих данных.";

        [Header("Дерево")]
        [Tooltip("Внутренний ID дерева. Например: hero_common, sword_master, sailor.")]
        [SerializeField] private string treeId = "hero_common";
        [Tooltip("Название дерева для UI.")]
        [SerializeField] private string displayName = "Общие навыки";
        [Tooltip("Узлы дерева.")]
        [SerializeField] private PlatformerSkillTreeNode[] nodes;

        public string TreeId => treeId;
        public string DisplayName => displayName;
        public PlatformerSkillTreeNode[] Nodes => nodes;
    }

    [System.Serializable]
    public sealed class PlatformerSkillTreeNode {
        [Tooltip("Навык в этом узле.")]
        public PlatformerSkillDefinition Skill;
        [Tooltip("Позиция узла в будущем UI дерева навыков.")]
        public Vector2 UiPosition;
        [Tooltip("Родительские узлы для линий в UI.")]
        public PlatformerSkillDefinition[] Parents;
    }
}
