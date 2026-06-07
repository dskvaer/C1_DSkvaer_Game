using System;
using UnityEngine;

namespace Ship {
    [Serializable]
    public class BanditHitZoneDefinition {
        [InspectorLabel("ID зоны")]
        [Tooltip("Системный ID броневой зоны. Должен быть уникальным внутри одного врага.")]
        [SerializeField] private string zoneId = "zone";
        [InspectorLabel("Название зоны")]
        [Tooltip("Название зоны для инспектора и будущего UI.")]
        [SerializeField] private string displayName = "Hit Zone";
        [InspectorLabel("Здоровье зоны")]
        [Tooltip("Базовое здоровье этой броневой зоны до множителей ранга и типа врага.")]
        [SerializeField, Min(1)] private int maxHealth = 25;
        [InspectorLabel("Множитель урона")]
        [Tooltip("Множитель входящего урона по этой зоне.")]
        [SerializeField, Min(0f)] private float damageMultiplier = 1f;
        [InspectorLabel("Префаб зоны")]
        [Tooltip("Готовый префаб зоны брони. Если пусто, конструктор создаст простую BoxCollider2D-зону.")]
        [SerializeField] private GameObject zonePrefab;
        [InspectorLabel("Локальная позиция")]
        [Tooltip("Позиция зоны относительно корня броневых зон врага.")]
        [SerializeField] private Vector2 localPosition;
        [InspectorLabel("Локальный размер")]
        [Tooltip("Размер простой зоны, если не указан отдельный префаб.")]
        [SerializeField] private Vector2 localSize = new Vector2(1.5f, 1.5f);
        [InspectorLabel("Отключать коллайдер")]
        [Tooltip("Отключать коллайдер после разрушения зоны, чтобы открыть корпус для урона.")]
        [SerializeField] private bool disableColliderWhenDestroyed = true;
        [InspectorLabel("Отключать визуал")]
        [Tooltip("Отключать визуальные объекты зоны после разрушения.")]
        [SerializeField] private bool disableVisualsWhenDestroyed = true;

        public string ZoneId => zoneId;
        public string DisplayName => displayName;
        public int MaxHealth => maxHealth;
        public float DamageMultiplier => damageMultiplier;
        public GameObject ZonePrefab => zonePrefab;
        public Vector2 LocalPosition => localPosition;
        public Vector2 LocalSize => localSize;
        public bool DisableColliderWhenDestroyed => disableColliderWhenDestroyed;
        public bool DisableVisualsWhenDestroyed => disableVisualsWhenDestroyed;

        public void SetDefaults(string id, string label, int health, Vector2 position)
        {
            zoneId = id;
            displayName = label;
            maxHealth = Mathf.Max(1, health);
            localPosition = position;
        }
    }
}
