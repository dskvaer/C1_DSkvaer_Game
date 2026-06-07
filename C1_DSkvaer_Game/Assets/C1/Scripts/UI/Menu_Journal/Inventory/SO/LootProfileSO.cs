using System.Collections.Generic;
using UnityEngine;

namespace Menu_Journal.Data {
    [System.Serializable]
    public class LootDropItem {
        [InspectorLabel("Предмет")]
        [Tooltip("Предмет, который может выпасть из этого профиля.")]
        public ItemDataSO Item;

        [InspectorLabel("Шанс выпадения")]
        [Tooltip("Вероятность выпадения предмета в процентах.")]
        [Range(0, 100)] public float DropChance = 50f;

        [InspectorLabel("Мин. количество")]
        [Tooltip("Минимальное количество этого предмета при выпадении.")]
        public int MinAmount = 1;

        [InspectorLabel("Макс. количество")]
        [Tooltip("Максимальное количество этого предмета при выпадении.")]
        public int MaxAmount = 1;
    }

    [CreateAssetMenu(fileName = "NewLootProfile", menuName = "Inventory/Loot Profile")]
    public class LootProfileSO : ScriptableObject {
        [Header("Визуализация")]
        [InspectorLabel("Тема контейнера")]
        [Tooltip("Тема контейнера: спрайты, ID и отображаемое имя.")]
        public ContainerThemeSO Theme;

        [Header("Настройки выпадения")]
        [InspectorLabel("Шанс контейнера")]
        [Tooltip("Шанс того, что контейнер вообще появится: от 0 до 100 процентов.")]
        [Range(0, 100)] public float ContainerDropChance = 100f;

        [InspectorLabel("Мин. разных предметов")]
        [Tooltip("Минимальное количество разных позиций в контейнере.")]
        public int MinItemsCount = 1;

        [InspectorLabel("Макс. разных предметов")]
        [Tooltip("Максимальное количество разных позиций в контейнере.")]
        public int MaxItemsCount = 5;

        [Header("Таблица лута")]
        [InspectorLabel("Возможный лут")]
        [Tooltip("Список предметов, которые могут выпасть из этого профиля.")]
        public List<LootDropItem> PossibleLoot;
    }
}
