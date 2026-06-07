using UnityEngine;

namespace Menu_Journal {
    /// <summary>
    /// Категория предметов для инвентаря, торговли и фильтров UI.
    /// </summary>
    [CreateAssetMenu(fileName = "New Category", menuName = "Data_bace/Item/Item Category")]
    public class ItemCategorySO : ScriptableObject {
        [Header("Категория")]
        [InspectorLabel("Название категории")]
        [Tooltip("Отображаемое имя категории предметов.")]
        [SerializeField] private string _categoryName;

        [InspectorLabel("Описание")]
        [Tooltip("Короткое описание того, какие предметы входят в эту категорию.")]
        [TextArea]
        [SerializeField] private string _description;

        public string CategoryName => _categoryName;
        public string Description => _description;
    }
}
