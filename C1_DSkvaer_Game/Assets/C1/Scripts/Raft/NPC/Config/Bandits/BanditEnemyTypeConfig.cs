using System.Collections.Generic;
using UnityEngine;

namespace Ship {
    [CreateAssetMenu(fileName = "BanditEnemyTypeConfig", menuName = "ShipConfigs/Bandits/Bandit Enemy Type", order = 20)]
    public class BanditEnemyTypeConfig : ScriptableObject {

        [Header("Идентификация")]

        [SerializeField]
        [InspectorLabel("ID типа")]
        [Tooltip("Короткий системный идентификатор типа врага. Используется в ID экземпляра и логике конструктора.")]
        private string typeId = "raider";

        [SerializeField]
        [InspectorLabel("Название типа")]
        [Tooltip("Человекочитаемое имя врага для редактора и будущего UI.")]
        private string displayName = "Raider";

        [SerializeField]
        [InspectorLabel("Ранг бандита")]
        [Tooltip("Старшинство врага. Влияет на здоровье, броневые зоны и итоговую опасность.")]
        private BanditEnemyRank rank = BanditEnemyRank.Junior;

        [SerializeField]
        [InspectorLabel("Префикс ID")]
        [Tooltip("Префикс, который добавляется в начало уникального ID корабля.")]
        private string idPrefix = "RB";

        [SerializeField]
        [InspectorLabel("Случайный номер ID")]
        [Tooltip("Если включено, номер в ID будет случайным. Если выключено, используется фиксированный номер ниже.")]
        private bool randomizeInstanceId = true;

        [SerializeField]
        [InspectorLabel("Фиксированный номер")]
        [Tooltip("Номер экземпляра для ID, когда случайная генерация отключена.")]
        private int fixedInstanceNumber;


        [Header("Боевые параметры")]

        [SerializeField, Min(1)]
        [InspectorLabel("Здоровье корпуса")]
        [Tooltip("Основное здоровье корабля. У младшего первого типа это главный и почти единственный запас прочности.")]
        private int baseShipHealth = 100;

        [SerializeField, Min(0f)]
        [InspectorLabel("Множитель здоровья брони")]
        [Tooltip("Общий множитель здоровья всех броневых зон этого типа врага.")]
        private float zoneHealthMultiplier = 1f;

        [SerializeField, Min(0f)]
        [InspectorLabel("Множитель урона по корпусу")]
        [Tooltip("Множитель урона, который получает основная зона корпуса после пробития защиты.")]
        private float coreDamageMultiplier = 1f;

        [SerializeField]
        [InspectorLabel("Позиция корпуса")]
        [Tooltip("Локальная позиция основной зоны поражения корпуса.")]
        private Vector2 coreLocalPosition = Vector2.zero;

        [SerializeField]
        [InspectorLabel("Размер корпуса")]
        [Tooltip("Локальный размер основной зоны поражения корпуса.")]
        private Vector2 coreLocalSize = new Vector2(1.2f, 1.2f);

        [SerializeField]
        [InspectorLabel("Слоты брони")]
        [Tooltip("Список разрушимых броневых зон. Живые зоны блокируют урон по основному корпусу.")]
        private List<BanditHitZoneDefinition> armorSlots = new();


        public string TypeId => typeId;
        public string DisplayName => displayName;
        public BanditEnemyRank Rank => rank;
        public string IdPrefix => idPrefix;
        public int BaseShipHealth => baseShipHealth;
        public float ZoneHealthMultiplier => zoneHealthMultiplier;
        public float CoreDamageMultiplier => coreDamageMultiplier;
        public Vector2 CoreLocalPosition => coreLocalPosition;
        public Vector2 CoreLocalSize => coreLocalSize;
        public IReadOnlyList<BanditHitZoneDefinition> ArmorSlots => armorSlots;

        /// <summary>
        /// Создает уникальный ID для конкретного экземпляра врага.
        /// </summary>
        public string CreateEnemyId()
        {
            int number = randomizeInstanceId ? Random.Range(1000, 9999) : fixedInstanceNumber;
            return $"{idPrefix}_{RankCode}_{typeId}_{number}";
        }

        /// <summary>
        /// Возвращает итоговое здоровье зоны с учетом ранга и множителей конфига.
        /// </summary>
        public int GetZoneHealth(BanditHitZoneDefinition zone)
        {
            if (zone == null)
            {
                return 1;
            }

            float rankMultiplier = rank switch
            {
                BanditEnemyRank.Junior => 1f,
                BanditEnemyRank.Middle => 1.35f,
                BanditEnemyRank.Senior => 1.75f,
                _ => 1f
            };

            return Mathf.Max(1, Mathf.RoundToInt(zone.MaxHealth * zoneHealthMultiplier * rankMultiplier));
        }

        private string RankCode => rank switch
        {
            BanditEnemyRank.Junior => "J",
            BanditEnemyRank.Middle => "M",
            BanditEnemyRank.Senior => "S",
            _ => "U"
        };

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(typeId))
            {
                typeId = name;
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = typeId;
            }

            baseShipHealth = Mathf.Max(1, baseShipHealth);
            zoneHealthMultiplier = Mathf.Max(0f, zoneHealthMultiplier);
            EnsureDefaultZones();
        }

        private void EnsureDefaultZones()
        {
            armorSlots ??= new List<BanditHitZoneDefinition>();

            int desiredCount = rank switch
            {
                BanditEnemyRank.Junior => 0,
                BanditEnemyRank.Middle => 2,
                BanditEnemyRank.Senior => 4,
                _ => 0
            };

            while (armorSlots.Count < desiredCount)
            {
                int index = armorSlots.Count;
                var zone = new BanditHitZoneDefinition();
                zone.SetDefaults(
                    $"armor_{index + 1}",
                    $"Armor {index + 1}",
                    25 + index * 10,
                    GetDefaultZonePosition(index, desiredCount)
                );
                armorSlots.Add(zone);
            }
        }

        private static Vector2 GetDefaultZonePosition(int index, int count)
        {
            if (count <= 1)
            {
                return Vector2.zero;
            }

            float spacing = 1.4f;
            float offset = (count - 1) * spacing * 0.5f;
            return new Vector2(index * spacing - offset, 0f);
        }
    }
}
