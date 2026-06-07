using UnityEngine;

namespace Menu_Journal {
    /// <summary>
    /// Описание предмета для инвентаря, лута и торговли.
    /// </summary>
    [CreateAssetMenu(fileName = "New Item", menuName = "Data_bace/Item/Item Data")]
    public class ItemDataSO : ScriptableObject, IItemData {
        [Header("Основная информация")]
        [InspectorLabel("ID предмета")]
        [Tooltip("Уникальный системный ID предмета. Если оставить пустым, будет создан из имени ассета.")]
        [SerializeField] private string _id;

        [InspectorLabel("Название")]
        [Tooltip("Отображаемое имя предмета для UI, торговли и окон лута.")]
        [SerializeField] private string _itemName;

        [InspectorLabel("Описание")]
        [Tooltip("Текстовое описание предмета, которое показывается игроку.")]
        [TextArea(3, 10)]
        [SerializeField] private string _description;

        [InspectorLabel("Иконка")]
        [Tooltip("Спрайт предмета для инвентаря, контейнеров и торгового окна.")]
        [SerializeField] private Sprite _icon;

        [Header("Классификация")]
        [InspectorLabel("Категория")]
        [Tooltip("Категория предмета: еда, ресурс, оружие, валюта и т.п.")]
        [SerializeField] private ItemCategorySO _category;

        [InspectorLabel("Редкость")]
        [Tooltip("Редкость предмета. Используется для фильтров, ценности и будущего баланса лута.")]
        [SerializeField] private ItemRarity _rarity;

        [Header("Торговля и ценность")]
        [InspectorLabel("Ценовая категория")]
        [Tooltip("Группа стоимости предмета для торговой системы.")]
        [SerializeField] private ItemValueCategory _valueCategory;

        [InspectorLabel("Базовая цена")]
        [Tooltip("Цена предмета в базовой валюте до учета рынка, репутации и модификаторов.")]
        [SerializeField, Min(0)] private int _basePrice = 1;

        [InspectorLabel("Это валюта")]
        [Tooltip("Если включено, предмет считается валютой и может использоваться как средство оплаты.")]
        [SerializeField] private bool _isCurrency = false;

        [Header("Физические параметры")]
        [InspectorLabel("Вес")]
        [Tooltip("Вес одной единицы предмета для расчета груза корабля.")]
        [SerializeField, Min(0)] private float _weight = 1.0f;

        [InspectorLabel("Размер стака")]
        [Tooltip("Сколько единиц предмета помещается в один слот инвентаря.")]
        [SerializeField, Min(1)] private int _maxStackSize = 1;

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

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_id))
            {
                _id = name.ToLower().Replace(" ", "_");
            }
        }
    }
}
