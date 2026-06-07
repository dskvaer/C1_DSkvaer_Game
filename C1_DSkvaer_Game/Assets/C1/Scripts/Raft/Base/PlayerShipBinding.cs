using Menu_Journal;
using Menu_Journal.Systems;
using C1.Player;
using UnityEngine;

namespace Ship {
    [DisallowMultipleComponent]
    [AddComponentMenu("C1/Player/Player Ship Binding")]
    public class PlayerShipBinding : MonoBehaviour {
        [InspectorLabel("Локальный трюм")]
        [Tooltip("Только для старых префабов: если трюм еще висит на корабле, система игрока сможет временно использовать его.")]
        [SerializeField] private ShipInventory legacyCargoInventory;

        private void Awake()
        {
            if (legacyCargoInventory == null) {
                legacyCargoInventory = GetComponent<ShipInventory>() ?? GetComponentInChildren<ShipInventory>(true);
            }
        }

        private void OnEnable()
        {
            RegisterAsActiveShip();
        }

        [ContextMenu("Register As Active Player Ship")]
        public void RegisterAsActiveShip()
        {
            PlayerInventorySystem.RegisterActiveShip(transform, legacyCargoInventory);
            PlayerSystem.GetOrCreate().RegisterActiveShip(transform);
            PlayerSystem.GetOrCreate().BindShipHealth(GetComponent<IHealth>() ?? GetComponentInChildren<IHealth>(true));
        }
    }
}
