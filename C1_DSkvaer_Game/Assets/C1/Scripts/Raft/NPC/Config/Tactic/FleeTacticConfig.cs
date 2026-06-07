using UnityEngine;

namespace Ship {
    /// <summary>
    /// Настройки тактики бегства NPC.
    /// </summary>
    [CreateAssetMenu(fileName = "FleeTacticConfig", menuName = "ShipConfigs/TacticConfigs/FleeTacticConfig", order = 10)]
    public class FleeTacticConfig : ScriptableObject {
        [Header("Условие бегства")]
        [InspectorLabel("Порог здоровья")]
        [Tooltip("Процент здоровья от 0 до 1, при котором NPC начинает бежать. По умолчанию 0.15 = 15%.")]
        [SerializeField, Range(0.01f, 1f)] private float healthThreshold = 0.15f;
        public float HealthThreshold => healthThreshold;

        [InspectorLabel("Порог здоровья игрока для последнего удара")]
        [Tooltip("Если здоровье игрока ниже этого процента, уставший NPC может сделать последний рывок тараном.")]
        [SerializeField, Range(0.01f, 1f)] private float playerLastStandHealthThreshold = 0.3f;
        public float PlayerLastStandHealthThreshold => playerLastStandHealthThreshold;

        [Header("Бегство")]
        [InspectorLabel("Скорость бегства")]
        [Tooltip("Начальная скорость NPC при отступлении от игрока.")]
        [SerializeField, Min(0f)] private float fleeSpeed = 1.2f;
        public float FleeSpeed => fleeSpeed;

        [InspectorLabel("Минимальная скорость")]
        [Tooltip("До какой доли от скорости бегства NPC замедляется, если союзников рядом нет.")]
        [SerializeField, Range(0.05f, 1f)] private float exhaustedSpeedMultiplier = 0.25f;
        public float ExhaustedSpeedMultiplier => exhaustedSpeedMultiplier;

        [InspectorLabel("Время усталости")]
        [Tooltip("За сколько секунд скорость без поддержки союзников падает со 100% до минимальной.")]
        [SerializeField, Min(0.1f)] private float exhaustionTime = 10f;
        public float ExhaustionTime => exhaustionTime;

        [InspectorLabel("Дистанция бегства")]
        [Tooltip("На какое расстояние NPC строит точку отступления от игрока.")]
        [SerializeField, Min(0.1f)] private float fleeDistance = 7f;
        public float FleeDistance => fleeDistance;

        [InspectorLabel("Радиус поиска союзников")]
        [Tooltip("В каком радиусе NPC ищет ближайшего союзника для отступления к нему.")]
        [SerializeField, Min(0.1f)] private float allySearchRadius = 16f;
        public float AllySearchRadius => allySearchRadius;

        [InspectorLabel("Слои союзников")]
        [Tooltip("Слои, на которых находятся союзные NPC. Если не настроено, используется тег Enemy.")]
        [SerializeField] private LayerMask allyLayer = -1;
        public LayerMask AllyLayer => allyLayer;

        [Header("Последний удар")]
        [InspectorLabel("Разрешить последний удар")]
        [Tooltip("Если включено, уставший NPC может сделать неуправляемый рывок тараном в сторону слабого игрока.")]
        [SerializeField] private bool allowLastStandRam = true;
        public bool AllowLastStandRam => allowLastStandRam;

        [InspectorLabel("Скорость рывка")]
        [Tooltip("Скорость последнего неуправляемого рывка.")]
        [SerializeField, Min(0f)] private float lastStandRamSpeed = 1.8f;
        public float LastStandRamSpeed => lastStandRamSpeed;

        [InspectorLabel("Длительность рывка")]
        [Tooltip("Сколько секунд NPC летит в выбранном направлении без управления.")]
        [SerializeField, Min(0.1f)] private float lastStandRamDuration = 1.25f;
        public float LastStandRamDuration => lastStandRamDuration;

        [InspectorLabel("Дистанция контакта")]
        [Tooltip("На какой дистанции последний рывок считается попаданием тараном.")]
        [SerializeField, Min(0.1f)] private float lastStandContactDistance = 1.2f;
        public float LastStandContactDistance => lastStandContactDistance;

        [InspectorLabel("Урон последнего удара")]
        [Tooltip("Урон, который наносит последний рывок при контакте с игроком.")]
        [SerializeField, Min(0)] private int lastStandDamage = 25;
        public int LastStandDamage => lastStandDamage;

        [InspectorLabel("Плавность поворота")]
        [Tooltip("Плавность разворота к точке бегства. Значение от 0 до 1.")]
        [SerializeField, Range(0f, 1f)] private float smoothTurnSpeed = 0.3f;
        public float SmoothTurnSpeed => smoothTurnSpeed;
    }
}
