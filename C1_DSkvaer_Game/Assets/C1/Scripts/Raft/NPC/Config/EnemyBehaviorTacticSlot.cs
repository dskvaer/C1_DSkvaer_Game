using System;
using UnityEngine;

namespace Ship {
    [Serializable]
    public class EnemyBehaviorTacticSlot {
        [InspectorLabel("Тип тактики")]
        [Tooltip("Какая тактика будет настраиваться этим слотом.")]
        [SerializeField] private EnemyTacticKind kind;
        [InspectorLabel("Приоритет")]
        [Tooltip("Меньшее число означает более высокий приоритет выбора.")]
        [SerializeField, Min(0)] private int priority;
        [InspectorLabel("Вес выбора")]
        [Tooltip("Шанс выбора среди тактик одного приоритета. 0 отключает выбор.")]
        [SerializeField, Min(0f)] private float weight = 1f;
        [InspectorLabel("Перезарядка тактики")]
        [Tooltip("Пауза после выбора тактики, прежде чем она сможет быть выбрана снова.")]
        [SerializeField, Min(0f)] private float cooldown = 0f;
        [InspectorLabel("Включена")]
        [Tooltip("Позволяет временно отключить тактику в профиле без удаления строки.")]
        [SerializeField] private bool enabled = true;

        public EnemyTacticKind Kind => kind;
        public int Priority => priority;
        public float Weight => weight;
        public float Cooldown => cooldown;
        public bool Enabled => enabled;
    }
}
