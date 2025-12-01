using UnityEngine;

namespace Menu_Journal {
    // Позволяет создавать предметы через ПКМ -> Create -> Menu Journal -> Item Data
    [CreateAssetMenu(fileName = "New Item", menuName = "Data_bace/Item/Item Data")]
    public class ItemDataSO : ScriptableObject, IItemData {
        [Header("Main Info")]
        [SerializeField] private string _id;
        [SerializeField] private string _itemName;
        [TextArea(3, 10)]
        [SerializeField] private string _description;
        [SerializeField] private Sprite _icon;

        [Header("Classification")]
        // Сюда перетаскиваем созданный ассет категории (например, Seafood.asset)
        [SerializeField] private ItemCategorySO _category;
        [SerializeField] private ItemRarity _rarity;

        [Header("Trading & Value")]
        [SerializeField] private ItemValueCategory _valueCategory;
        [SerializeField, Min(0)] private int _basePrice = 1; // Цена в минимальной валюте (медь)
        [Tooltip("Если галочка стоит, предмет считается валютой (отображается в кошельке)")]
        [SerializeField] private bool _isCurrency = false;

        [Header("Physical Stats")]
        [SerializeField, Min(0)] private float _weight = 1.0f;
        [SerializeField, Min(1)] private int _maxStackSize = 1;

        // --- РЕАЛИЗАЦИЯ ИНТЕРФЕЙСА IItemData ---
        public string ID => _id;
        public string Name => _itemName;
        public Sprite Icon => _icon;
        public string Description => _description;

        public ItemCategorySO Category => _category;
        public ItemRarity Rarity => _rarity;

        public ItemValueCategory ValueCategory => _valueCategory;
        public int BasePrice => _basePrice;
        public bool IsCurrency => _isCurrency;

        public float Weight => _weight;
        public int MaxStackSize => _maxStackSize;

        // Авто-генерация ID при изменении в редакторе
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_id))
            {
                _id = name.ToLower().Replace(" ", "_");
            }
        }
    }
}