using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Menu_Journal.Data {
    [CreateAssetMenu(fileName = "ContainerThemeDatabase", menuName = "Inventory/Container Theme Database")]
    public class ContainerThemeDatabaseSO : ScriptableObject {
        [Header("Коллекция тем")]
        [InspectorLabel("Все темы контейнеров")]
        [Tooltip("Список доступных тем контейнеров. Используется для выбора визуала по префиксу ID.")]
        [SerializeField] private List<ContainerThemeSO> _allThemes = new List<ContainerThemeSO>();

        /// <summary>
        /// Возвращает тему по префиксу ID контейнера: например PLAYER, PIRATE или MERCH.
        /// </summary>
        public ContainerThemeSO GetTheme(string idPrefix)
        {
            var theme = _allThemes.FirstOrDefault(t => t.IDPrefix == idPrefix);
            if (theme == null)
            {
                Debug.LogWarning($"[ThemeDatabase] Тема с префиксом '{idPrefix}' не найдена. Используется первая доступная тема.");
                return _allThemes.Count > 0 ? _allThemes[0] : null;
            }
            return theme;
        }

#if UNITY_EDITOR
        [ContextMenu("Auto-Find All Themes")]
        public void FindAllThemes()
        {
            _allThemes.Clear();
            string[] guids = AssetDatabase.FindAssets("t:ContainerThemeSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ContainerThemeSO theme = AssetDatabase.LoadAssetAtPath<ContainerThemeSO>(path);
                if (theme != null && !_allThemes.Contains(theme))
                {
                    _allThemes.Add(theme);
                }
            }
            Debug.Log($"[ThemeDatabase] Найдено {_allThemes.Count} тем контейнеров.");
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
