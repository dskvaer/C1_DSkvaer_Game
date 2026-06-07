using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Menu_Journal.Data {
    /// <summary>
    /// База всех предметов проекта для поиска по ID.
    /// </summary>
    [CreateAssetMenu(fileName = "GameItemDatabase", menuName = "Data_bace/Item/Item Database")]
    public class ItemDatabaseSO : ScriptableObject {
        [Header("База предметов")]
        [InspectorLabel("Все предметы")]
        [Tooltip("Список всех ItemDataSO, которые доступны инвентарю, луту и торговле. Можно заполнить через контекстное меню.")]
        public List<ItemDataSO> allItems = new List<ItemDataSO>();

        private Dictionary<string, ItemDataSO> _itemLookup;

        public void Initialize()
        {
            _itemLookup = new Dictionary<string, ItemDataSO>();

            foreach (var item in allItems)
            {
                if (item != null && !string.IsNullOrEmpty(item.ID))
                {
                    if (!_itemLookup.ContainsKey(item.ID))
                    {
                        _itemLookup.Add(item.ID, item);
                    }
                    else
                    {
                        Debug.LogWarning($"[ItemDatabase] Дублирующийся ID предмета: {item.ID}. Предмет: {item.name}");
                    }
                }
            }
            Debug.Log($"[ItemDatabase] База инициализирована. Предметов загружено: {_itemLookup.Count}");
        }

        public ItemDataSO GetItem(string id)
        {
            if (_itemLookup == null) Initialize();

            if (_itemLookup.TryGetValue(id, out ItemDataSO item))
            {
                return item;
            }

            Debug.LogWarning($"[ItemDatabase] Предмет с ID '{id}' не найден!");
            return null;
        }

#if UNITY_EDITOR
        [ContextMenu("Авто-поиск всех предметов в проекте")]
        public void FindAllItemsInProject()
        {
            allItems.Clear();

            string[] guids = AssetDatabase.FindAssets("t:ItemDataSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemDataSO item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);

                if (item != null)
                {
                    allItems.Add(item);
                }
            }

            Debug.Log($"[ItemDatabase] Автоматически найдено и добавлено предметов: {allItems.Count}");
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
