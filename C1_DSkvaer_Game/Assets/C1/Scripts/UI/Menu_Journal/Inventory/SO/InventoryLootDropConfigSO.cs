using UnityEngine;

namespace Menu_Journal.Data {
    public enum InventoryLootDropMode {
        InventoryOnly,
        ProfileOnly,
        InventoryAndProfile
    }

    [CreateAssetMenu(fileName = "InventoryLootDropConfig", menuName = "Inventory/Inventory Loot Drop Config")]
    public class InventoryLootDropConfigSO : ScriptableObject {
        [Header("Источник лута")]
        [InspectorLabel("Режим выпадения")]
        [Tooltip("Откуда брать предметы: только из инвентаря юнита, только из профиля или объединить оба источника.")]
        [SerializeField] private InventoryLootDropMode dropMode = InventoryLootDropMode.InventoryOnly;
        [InspectorLabel("Очищать инвентарь")]
        [Tooltip("Если включено, после создания контейнера предметы удаляются из инвентаря погибшего юнита.")]
        [SerializeField] private bool clearSourceInventoryAfterDrop = true;

        [Header("Запасной и дополнительный лут")]
        [InspectorLabel("Профиль лута")]
        [Tooltip("Дополнительный или запасной список предметов, если нужно добавить лут поверх инвентаря.")]
        [SerializeField] private LootProfileSO lootProfile;
        [InspectorLabel("Шанс контейнера")]
        [Tooltip("Вероятность появления контейнера при смерти или ручном сбросе.")]
        [SerializeField, Range(0f, 100f)] private float containerDropChance = 100f;

        [Header("Контейнер")]
        [InspectorLabel("Тема контейнера")]
        [Tooltip("Визуальная тема контейнера. Если пусто, берется тема из профиля лута.")]
        [SerializeField] private ContainerThemeSO containerTheme;
        [InspectorLabel("Смещение появления")]
        [Tooltip("Смещение позиции контейнера относительно юнита, который сбрасывает лут.")]
        [SerializeField] private Vector3 spawnOffset;

        public InventoryLootDropMode DropMode => dropMode;
        public bool ClearSourceInventoryAfterDrop => clearSourceInventoryAfterDrop;
        public LootProfileSO LootProfile => lootProfile;
        public float ContainerDropChance => containerDropChance;
        public ContainerThemeSO ContainerTheme => containerTheme != null ? containerTheme : lootProfile != null ? lootProfile.Theme : null;
        public Vector3 SpawnOffset => spawnOffset;
    }
}
