using UnityEngine;

namespace Menu_Journal {
    // Позволяет создавать файлы категорий через ПКМ -> Create -> Menu Journal -> Item Category
    [CreateAssetMenu(fileName = "New Category", menuName = "Data_bace/Item/Item Category")]
    public class ItemCategorySO : ScriptableObject {
        [SerializeField] private string _categoryName;
        [TextArea]
        [SerializeField] private string _description;

        public string CategoryName => _categoryName;
    }
}