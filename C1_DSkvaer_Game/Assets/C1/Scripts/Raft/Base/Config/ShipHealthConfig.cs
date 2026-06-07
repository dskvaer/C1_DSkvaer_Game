using UnityEngine;

namespace Ship {
    /// <summary>
    /// Базовые настройки здоровья корабля.
    /// </summary>
    [CreateAssetMenu(fileName = "ShipHealthConfig", menuName = "ShipConfigs/ShipHealthConfig", order = 2)]
    public class ShipHealthConfig : ScriptableObject {
        [Header("Здоровье")]
        [InspectorLabel("Максимальное здоровье")]
        [Tooltip("Базовый запас прочности корабля до учета улучшений и временных модификаторов.")]
        [SerializeField] private int maxHealth = 100;
        public int MaxHealth => maxHealth;
    }
}
