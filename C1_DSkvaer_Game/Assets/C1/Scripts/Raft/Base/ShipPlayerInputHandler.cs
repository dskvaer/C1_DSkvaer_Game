using UnityEngine; // Основной namespace Unity
using UnityEngine.InputSystem; // Поддержка Input System

namespace Ship {
    /// <summary>
    /// Компонент для обработки ввода игрока.
    /// Передаёт команды движения, поворота и стрельбы в ShipMovement и ShipWeaponSystem.
    /// </summary>
    /// <remarks>
    /// Привязка в Unity Inspector:
    /// - Movement: Компонент ShipMovement (IShipMovable).
    /// - WeaponSystem: Компонент ShipWeaponSystem для управления стрельбой.
    /// Настройка сцены:
    /// - Убедитесь, что Movement ссылается на ShipMovement на объекте игрока (Player_Ship).
    /// - Убедитесь, что WeaponSystem ссылается на ShipWeaponSystem на объекте игрока.
    /// - Требуется InputSystem_Actions, настроенный через Input System Package (ShipControls) с действиями Move, Ship_Gun_L, Ship_Gun_R.
    /// Логика работы:
    /// - Awake: Инициализирует InputSystem_Actions, проверяет наличие ShipMovement и ShipWeaponSystem.
    /// - OnEnable/OnDisable: Включает/отключает систему ввода.
    /// - FixedUpdate: Передаёт вектор ввода (Horizontal, Vertical) в ShipMove и ShipRotate.
    /// - OnShipMove: Обрабатывает ввод движения.
    /// - OnFireLeft/OnFireRight: Обрабатывает ввод стрельбы с левого/правого борта.
    /// </remarks>
    [RequireComponent(typeof(ShipMovement), typeof(ShipID))]
    public class ShipPlayerInputHandler : MonoBehaviour {
        [SerializeField] private ShipMovement movement; // Компонент движения
        [SerializeField] private ShipWeaponSystem weaponSystem; // Компонент управления оружием
        private InputSystem_Actions controls; // Система ввода
        private Vector2 inputDirection; // Вектор направления ввода

        // Инициализация при старте
        private void Awake()
        {
            controls = new InputSystem_Actions(); // Создаём экземпляр InputSystem_Actions
            if (movement == null) // Проверяем наличие ShipMovement
            {
                movement = GetComponent<ShipMovement>(); // Пытаемся получить ShipMovement
                if (movement == null) // Проверяем успешность получения
                {
                    Debug.LogError($"ShipMovement не привязан для {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"})!", this); // Логируем ошибку
                    enabled = false; // Отключаем компонент
                    return;
                }
            }
            if (weaponSystem == null) // Проверяем наличие ShipWeaponSystem
            {
                weaponSystem = GetComponent<ShipWeaponSystem>(); // Пытаемся получить ShipWeaponSystem
                if (weaponSystem == null) // Проверяем успешность получения
                {
                    Debug.LogWarning($"ShipWeaponSystem не привязан для {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"}). Стрельба отключена.", this); // Логируем предупреждение
                }
            }
            if (controls.ShipControls.Move == null || controls.ShipControls.Ship_Gun_L == null || controls.ShipControls.Ship_Gun_R == null) // Проверяем наличие действий
            {
                Debug.LogError($"Действия Move, Ship_Gun_L или Ship_Gun_R отсутствуют в InputSystem_Actions для {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"})!", this); // Логируем ошибку
            }
            inputDirection = Vector2.zero; // Сбрасываем вектор ввода
            Debug.Log($"ShipPlayerInputHandler инициализирован для {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"})"); // Логируем инициализацию
        }

        // Включает систему ввода
        private void OnEnable()
        {
            if (controls == null) // Проверяем наличие системы ввода
            {
                Debug.LogError($"InputSystem_Actions не инициализирован для {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"})!", this); // Логируем ошибку
                return;
            }
            if (controls.ShipControls.Move != null) // Проверяем наличие действия Move
            {
                controls.ShipControls.Move.performed += OnShipMove; // Подписываемся на событие движения
                controls.ShipControls.Move.canceled += OnShipMove; // Подписываемся на событие остановки
            }
            if (controls.ShipControls.Ship_Gun_L != null) // Проверяем наличие действия Ship_Gun_L
            {
                controls.ShipControls.Ship_Gun_L.performed += OnFireLeft; // Подписываемся на стрельбу с левого борта
            }
            if (controls.ShipControls.Ship_Gun_R != null) // Проверяем наличие действия Ship_Gun_R
            {
                controls.ShipControls.Ship_Gun_R.performed += OnFireRight; // Подписываемся на стрельбу с правого борта
            }
            controls.ShipControls.Enable(); // Активируем систему ввода
            Debug.Log($"Система ввода включена для {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"})"); // Логируем включение
        }

        // Отключает систему ввода
        private void OnDisable()
        {
            if (controls == null) // Проверяем наличие системы ввода
            {
                Debug.LogError($"InputSystem_Actions не инициализирован для {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"})!", this); // Логируем ошибку
                return;
            }
            if (controls.ShipControls.Move != null) // Проверяем наличие действия Move
            {
                controls.ShipControls.Move.performed -= OnShipMove; // Отписываемся от события движения
                controls.ShipControls.Move.canceled -= OnShipMove; // Отписываемся от события остановки
            }
            if (controls.ShipControls.Ship_Gun_L != null) // Проверяем наличие действия Ship_Gun_L
            {
                controls.ShipControls.Ship_Gun_L.performed -= OnFireLeft; // Отписываемся от стрельбы с левого борта
            }
            if (controls.ShipControls.Ship_Gun_R != null) // Проверяем наличие действия Ship_Gun_R
            {
                controls.ShipControls.Ship_Gun_R.performed -= OnFireRight; // Отписываемся от стрельбы с правого борта
            }
            controls.ShipControls.Disable(); // Деактивируем систему ввода
            Debug.Log($"Система ввода отключена для {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"})"); // Логируем отключение
        }

        // Обрабатывает ввод движения
        private void OnShipMove(InputAction.CallbackContext context)
        {
            inputDirection = context.ReadValue<Vector2>(); // Считываем вектор ввода
            Debug.Log($"Ввод корабля: X={inputDirection.x:F2}, Y={inputDirection.y:F2} для {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"})"); // Логируем ввод
        }

        // Обрабатывает ввод стрельбы с левого борта
        private void OnFireLeft(InputAction.CallbackContext context)
        {
            if (weaponSystem == null) // Проверяем наличие ShipWeaponSystem
            {
                Debug.LogWarning($"ShipWeaponSystem отсутствует для {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"})!", this); // Логируем предупреждение
                return;
            }
            weaponSystem.FireLeft(); // Вызываем стрельбу с левого борта
            Debug.Log($"Ввод стрельбы с левого борта для {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"})"); // Логируем ввод
        }

        // Обрабатывает ввод стрельбы с правого борта
        private void OnFireRight(InputAction.CallbackContext context)
        {
            if (weaponSystem == null) // Проверяем наличие ShipWeaponSystem
            {
                Debug.LogWarning($"ShipWeaponSystem отсутствует для {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"})!", this); // Логируем предупреждение
                return;
            }
            weaponSystem.FireRight(); // Вызываем стрельбу с правого борта
            Debug.Log($"Ввод стрельбы с правого борта для {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"})"); // Логируем ввод
        }

        // Передаёт ввод в ShipMovement
        private void FixedUpdate()
        {
            if (movement == null) // Проверяем наличие ShipMovement
            {
                Debug.LogWarning($"ShipMovement отсутствует для {gameObject.name} (ID={GetComponent<ShipID>()?.ID ?? "Unknown"})", this); // Логируем предупреждение
                return;
            }
            float inputStrength = inputDirection.magnitude; // Рассчитываем силу ввода
            movement.ShipMove(inputDirection, inputStrength); // Передаём вектор и силу для движения
            movement.ShipRotate(inputDirection, inputStrength); // Передаём вектор и силу для поворота
        }
    }
}