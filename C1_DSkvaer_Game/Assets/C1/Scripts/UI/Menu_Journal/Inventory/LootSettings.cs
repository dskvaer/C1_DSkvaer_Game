using Menu_Journal;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLootSettings", menuName = "Inventory/Loot Settings")]
public class LootSettings : ScriptableObject {
    [Header("Настройки генерации")]
    [Tooltip("Минимальное количество предметов")]
    public int minItems = 1;
    [Tooltip("Максимальное количество предметов")]
    public int maxItems = 3;

    [Header("Список возможного лута")]
    public List<LootItem> possibleLoot;
}

[System.Serializable]
public class LootItem {
    public ItemDataSO itemData;    // Теперь ссылаемся на ScriptableObject предмета
    [Range(0, 100)]
    public float dropChance;     // Шанс выпадения
}