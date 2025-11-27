using UnityEngine;

namespace Ship {
    /// <summary>
    /// Настройки здоровья корабля.
    /// Универсальны для игрока, врагов и торговцев.
    /// </summary>
    /// <remarks>
    /// Привязка в Unity Inspector:
    /// - Привязать к компоненту ShipHealth на объекте корабля (Player_Ship, Enemy_Ship, Trader_Ship).
    /// - Устанавливает базовое максимальное здоровье корабля.
    /// Настройка сцены:
    /// - Создайте ScriptableObject через меню (File > Create > ShipConfigs > ShipHealthConfig).
    /// </remarks>
    [CreateAssetMenu(fileName = "ShipHealthConfig", menuName = "ShipConfigs/ShipHealthConfig", order = 2)]
    public class ShipHealthConfig : ScriptableObject {
        [SerializeField] private int maxHealth = 100; // Максимальное здоровье
        /// <summary>
        /// Максимальное здоровье корабля.
        /// Используется в ShipHealth для инициализации здоровья.
        /// </summary>
        public int MaxHealth => maxHealth;
    }
}