using UnityEngine; // Основной namespace Unity
using UnityEngine.Tilemaps; // Поддержка Tilemap

namespace Ship {
    /// <summary>
    /// Контекст для тактик ИИ NPC.
    /// Содержит ссылки на компоненты и данные корабля.
    /// </summary>
    /// <remarks>
    /// Использование:
    /// - Передаётся в тактики (IEnemyTactic) для доступа к данным корабля и игрока.
    /// - Инициализируется в NPCAI.Awake.
    /// Настройка сцены:
    /// - Убедитесь, что все компоненты (ShipMovement, ShipHealth, Rigidbody, WaterTilemap) привязаны в NPCAI.
    /// - ShipRamConfig должен быть ScriptableObject, созданным через меню (Assets > Create > ShipConfigs).
    /// - ShipWeaponSystem должен быть компонентом на объекте NPC (опционально).
    /// Логика работы:
    /// - Хранит ссылки на IShipMovable, IShipHealth, Rigidbody2D, Tilemap, ShipRamConfig, ShipWeaponSystem, Transform игрока и его IShipHealth.
    /// - Обновляется в NPCAI при обнаружении/потере игрока.
    /// </remarks>
    public class EnemyAIContext {
        /// <summary>
        /// Интерфейс движения корабля.
        /// </summary>
        public IShipMovable ShipMovement { get; set; } // Интерфейс движения

        /// <summary>
        /// Интерфейс здоровья корабля.
        /// </summary>
        public IShipHealth ShipHealth { get; set; } // Интерфейс здоровья

        /// <summary>
        /// Rigidbody2D корабля.
        /// </summary>
        public Rigidbody2D Rigidbody { get; set; } // Rigidbody2D

        /// <summary>
        /// Водный тайлмап для ограничения движения.
        /// </summary>
        public Tilemap WaterTilemap { get; set; } // Тайлмап

        /// <summary>
        /// Настройки тарана.
        /// </summary>
        public ShipRamConfig RamConfig { get; set; } // Конфиг тарана

        /// <summary>
        /// Компонент управления оружием.
        /// </summary>
        public ShipWeaponSystem WeaponSystem { get; set; } // Компонент оружия

        /// <summary>
        /// Трансформ игрока.
        /// </summary>
        public Transform Player { get; set; } // Трансформ игрока

        /// <summary>
        /// Здоровье игрока.
        /// </summary>
        public IShipHealth PlayerHealth { get; set; } // Здоровье игрока
    }
}