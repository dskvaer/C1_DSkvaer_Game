using UnityEngine;
using UnityEngine.InputSystem;

namespace Ship {
    [RequireComponent(typeof(ShipMovement), typeof(ShipID))]
    public class ShipPlayerInputHandler : MonoBehaviour {
        [Header("Компоненты игрока")]
        [InspectorLabel("Движение корабля")]
        [Tooltip("Компонент движения корабля игрока. Если поле пустое, будет найден на этом объекте.")]
        [SerializeField] private ShipMovement movement;

        [InspectorLabel("Система оружия")]
        [Tooltip("Компонент управления бортовыми пушками игрока. Если поле пустое, будет найден на этом объекте.")]
        [SerializeField] private ShipWeaponSystem weaponSystem;

        private InputSystem_Actions controls;
        private Vector2 inputDirection;

        private void Awake()
        {
            controls = new InputSystem_Actions();
            if (movement == null) {
                movement = GetComponent<ShipMovement>();
            }

            if (weaponSystem == null) {
                weaponSystem = GetComponent<ShipWeaponSystem>();
            }

            if (movement == null) {
                Debug.LogError($"ShipMovement is missing on {gameObject.name}.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (controls == null) {
                return;
            }

            controls.ShipControls.Move.performed += OnShipMove;
            controls.ShipControls.Move.canceled += OnShipMove;

            controls.ShipControls.Ship_Gun_L.started += OnAimLeftStarted;
            controls.ShipControls.Ship_Gun_L.canceled += OnAimLeftCanceled;
            controls.ShipControls.Ship_Gun_R.started += OnAimRightStarted;
            controls.ShipControls.Ship_Gun_R.canceled += OnAimRightCanceled;

            controls.ShipControls.Enable();
        }

        private void OnDisable()
        {
            if (controls == null) {
                return;
            }

            controls.ShipControls.Move.performed -= OnShipMove;
            controls.ShipControls.Move.canceled -= OnShipMove;

            controls.ShipControls.Ship_Gun_L.started -= OnAimLeftStarted;
            controls.ShipControls.Ship_Gun_L.canceled -= OnAimLeftCanceled;
            controls.ShipControls.Ship_Gun_R.started -= OnAimRightStarted;
            controls.ShipControls.Ship_Gun_R.canceled -= OnAimRightCanceled;

            controls.ShipControls.Disable();
        }

        private void FixedUpdate()
        {
            if (movement == null) {
                return;
            }

            float inputStrength = inputDirection.magnitude;
            movement.ShipMove(inputDirection, inputStrength);
            movement.ShipRotate(inputDirection, inputStrength);
        }

        private void OnShipMove(InputAction.CallbackContext context)
        {
            inputDirection = context.ReadValue<Vector2>();
        }

        private void OnAimLeftStarted(InputAction.CallbackContext context)
        {
            weaponSystem?.BeginAimingLeft();
        }

        private void OnAimLeftCanceled(InputAction.CallbackContext context)
        {
            weaponSystem?.ReleaseFireLeft();
        }

        private void OnAimRightStarted(InputAction.CallbackContext context)
        {
            weaponSystem?.BeginAimingRight();
        }

        private void OnAimRightCanceled(InputAction.CallbackContext context)
        {
            weaponSystem?.ReleaseFireRight();
        }
    }
}
