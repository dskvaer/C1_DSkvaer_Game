using UnityEngine;

namespace Menu_Journal.Systems {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerCargoInventory))]
    [RequireComponent(typeof(PlayerPersonalInventory))]
    [AddComponentMenu("C1/Player/Player Inventory System")]
    public class PlayerInventorySystem : MonoBehaviour {
        public static PlayerInventorySystem Instance { get; private set; }

        [Header("Инвентари игрока")]
        [InspectorLabel("Трюм игрока")]
        [Tooltip("Постоянный трюм игрока. Он не обязан висеть на текущем корабле.")]
        [SerializeField] private ShipInventory cargoInventory;

        [InspectorLabel("Инвентарь персонажа")]
        [Tooltip("Личный инвентарь персонажа для будущей экипировки и предметов вне трюма.")]
        [SerializeField] private SimpleInventory personalInventory;

        [Header("Активный корабль")]
        [InspectorLabel("Текущий корабль")]
        [Tooltip("Физический корабль игрока в сцене. Используется как точка сброса груза и взаимодействий.")]
        [SerializeField] private Transform activeShip;

        [InspectorLabel("Тег игрока")]
        [Tooltip("Fallback-поиск активного корабля, если PlayerShipBinding не назначил его явно.")]
        [SerializeField] private string playerTag = "Player";

        [InspectorLabel("Смещение сброса")]
        [Tooltip("Где относительно активного корабля появится контейнер сброшенного груза.")]
        [SerializeField] private Vector3 dropOffset = Vector3.right * 1.5f;

        public ShipInventory CargoInventory => ResolveCargoInventory();
        public SimpleInventory PersonalInventory => ResolvePersonalInventory();
        public Transform ActiveShip => ResolveActiveShip();

        private void Awake()
        {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            ResolveCargoInventory();
            ResolvePersonalInventory();
            ResolveActiveShip();
        }

        public static ShipInventory FindCargoInventory()
        {
            PlayerInventorySystem system = FindExistingSystem();
            if (system != null && system.CargoInventory != null) {
                return system.CargoInventory;
            }

            ShipInventory legacyInventory = FindLegacyPlayerCargoInventory();
            if (legacyInventory != null) {
                return legacyInventory;
            }

            return GetOrCreate().CargoInventory;
        }

        public static SimpleInventory FindPersonalInventory()
        {
            PlayerInventorySystem system = FindExistingSystem();
            if (system != null && system.PersonalInventory != null) {
                return system.PersonalInventory;
            }

            PlayerPersonalInventory personal = FindFirstObjectByType<PlayerPersonalInventory>();
            if (personal != null) {
                return personal;
            }

            return GetOrCreate().PersonalInventory;
        }

        public static Vector3 GetDropPosition(Vector3 fallbackWorldPosition)
        {
            PlayerInventorySystem system = FindExistingSystem();
            if (system != null) {
                return system.GetDropPosition();
            }

            Transform legacyShip = FindLegacyPlayerTransform();
            if (legacyShip != null) {
                return legacyShip.position + Vector3.right * 1.5f;
            }

            return fallbackWorldPosition + Vector3.right * 1.5f;
        }

        public static Transform GetOrCreateActiveShip()
        {
            PlayerInventorySystem system = FindExistingSystem();
            if (system != null && system.ActiveShip != null) {
                return system.ActiveShip;
            }

            Transform legacyShip = FindLegacyPlayerTransform();
            if (legacyShip != null) {
                return legacyShip;
            }

            return system != null ? system.ResolveActiveShip() : null;
        }

        public static void RegisterActiveShip(Transform shipTransform, ShipInventory legacyCargoInventory = null)
        {
            PlayerInventorySystem system = FindExistingSystem();
            bool createdSystem = system == null;
            system ??= GetOrCreate();
            system.SetActiveShip(shipTransform, legacyCargoInventory, createdSystem);
        }

        public Vector3 GetDropPosition()
        {
            Transform origin = ResolveActiveShip();
            if (origin != null) {
                return origin.position + dropOffset;
            }

            ShipInventory cargo = ResolveCargoInventory();
            if (cargo != null) {
                return cargo.transform.position + dropOffset;
            }

            return transform.position + dropOffset;
        }

        public void SetActiveShip(Transform shipTransform, ShipInventory legacyCargoInventory = null, bool adoptLegacyCargo = false)
        {
            if (shipTransform != null) {
                activeShip = shipTransform;
            }

            if (legacyCargoInventory != null && (adoptLegacyCargo || cargoInventory == null)) {
                cargoInventory = legacyCargoInventory;
            }
        }

        private ShipInventory ResolveCargoInventory()
        {
            if (cargoInventory != null) {
                return cargoInventory;
            }

            cargoInventory = GetComponent<PlayerCargoInventory>();
            if (cargoInventory == null) {
                cargoInventory = GetComponent<ShipInventory>();
            }

            if (cargoInventory == null) {
                cargoInventory = GetComponentInChildren<ShipInventory>(true);
            }

            if (cargoInventory == null && Application.isPlaying) {
                cargoInventory = gameObject.AddComponent<PlayerCargoInventory>();
            }

            return cargoInventory;
        }

        private SimpleInventory ResolvePersonalInventory()
        {
            if (personalInventory != null) {
                return personalInventory;
            }

            personalInventory = GetComponent<PlayerPersonalInventory>();
            if (personalInventory == null) {
                personalInventory = GetComponent<SimpleInventory>();
            }

            if (personalInventory == null) {
                personalInventory = GetComponentInChildren<SimpleInventory>(true);
            }

            if (personalInventory == null && Application.isPlaying) {
                personalInventory = gameObject.AddComponent<PlayerPersonalInventory>();
            }

            return personalInventory;
        }

        private Transform ResolveActiveShip()
        {
            if (activeShip != null) {
                return activeShip;
            }

            Transform legacyShip = FindLegacyPlayerTransform(playerTag);
            if (legacyShip != null) {
                activeShip = legacyShip;
                return activeShip;
            }

            Ship.PlayerShipBinding binding = FindFirstObjectByType<Ship.PlayerShipBinding>();
            if (binding != null) {
                activeShip = binding.transform;
            }

            return activeShip;
        }

        private static PlayerInventorySystem FindExistingSystem()
        {
            if (Instance != null) {
                return Instance;
            }

            Instance = FindFirstObjectByType<PlayerInventorySystem>();
            return Instance;
        }

        private static PlayerInventorySystem GetOrCreate()
        {
            PlayerInventorySystem system = FindExistingSystem();
            if (system != null) {
                return system;
            }

            GameObject systemObject = new GameObject("PlayerInventorySystem");
            system = systemObject.AddComponent<PlayerInventorySystem>();
            return system;
        }

        private static ShipInventory FindLegacyPlayerCargoInventory()
        {
            Transform playerTransform = FindLegacyPlayerTransform();
            if (playerTransform != null) {
                ShipInventory inventory = playerTransform.GetComponent<ShipInventory>() ?? playerTransform.GetComponentInChildren<ShipInventory>(true);
                if (inventory != null) {
                    return inventory;
                }
            }

            return FindFirstObjectByType<ShipInventory>();
        }

        private static Transform FindLegacyPlayerTransform(string tag = "Player")
        {
            if (string.IsNullOrWhiteSpace(tag)) {
                return null;
            }

            try {
                GameObject player = GameObject.FindGameObjectWithTag(tag);
                return player != null ? player.transform : null;
            }
            catch (UnityException) {
                return null;
            }
        }
    }
}
