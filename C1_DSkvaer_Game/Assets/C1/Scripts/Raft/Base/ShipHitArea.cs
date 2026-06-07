using UnityEngine;

namespace Ship {
    /// <summary>
    /// Зона корпуса, которая принимает урон от тарана или других попаданий.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ShipHitArea : MonoBehaviour {
        [Header("Связи зоны урона")]
        [InspectorLabel("Компонент здоровья")]
        [Tooltip("Компонент корабля, который реализует IShipHealth. Обычно это ShipHealth на родительском объекте.")]
        [SerializeField] private MonoBehaviour healthComponent;

        [InspectorLabel("Конфиг тарана")]
        [Tooltip("Настройки множителя урона для этой зоны. Если пусто, используется множитель 1.")]
        [SerializeField] private ShipRamConfig ramConfig;

        private IShipHealth shipHealth;

        public float DamageMultiplier => ramConfig != null ? ramConfig.DamageMultiplier : 1.0f;

        private void Awake()
        {
            shipHealth = healthComponent as IShipHealth;
            if (shipHealth == null && healthComponent != null)
            {
                shipHealth = healthComponent.GetComponent<IShipHealth>();
            }

            if (shipHealth == null)
            {
                Debug.LogError($"IShipHealth не найден для {gameObject.name} (ID={GetComponentInParent<ShipID>()?.ID ?? "Unknown"})!", this);
                enabled = false;
                return;
            }

            Collider2D collider = GetComponent<Collider2D>();
            collider.isTrigger = true;
            collider.gameObject.layer = LayerMask.NameToLayer("Default");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("AttackArea"))
            {
                if (ramConfig == null)
                {
                    ;
                }

                ShipRamAttack attacker = other.GetComponentInParent<ShipRamAttack>();
                if (attacker != null)
                {
                    float damage = attacker.CalculateDamage() * DamageMultiplier;
                    shipHealth.TakeShipDamage(Mathf.RoundToInt(damage), true);
                }
            }
        }
    }
}
