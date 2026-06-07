using System;
using UnityEngine;

#pragma warning disable 0414

namespace C1.Platformer.Characters {
    [Serializable]
    public sealed class PlatformerWeaponSlot {
        public string Id = "Weapon";
        public GameObject Prefab;
        public Transform SocketOverride;
        public bool StartsEquipped;
    }

    public readonly struct PlatformerWeaponContext {
        public readonly GameObject Owner;
        public readonly Transform Origin;
        public readonly Vector2 Direction;
        public readonly PlatformerCharacterMotor Motor;
        public readonly PlatformerCharacterVitals Vitals;

        public PlatformerWeaponContext(GameObject owner, Transform origin, Vector2 direction, PlatformerCharacterMotor motor, PlatformerCharacterVitals vitals)
        {
            Owner = owner;
            Origin = origin;
            Direction = direction;
            Motor = motor;
            Vitals = vitals;
        }
    }

    public interface IPlatformerWeapon {
        void Equip(PlatformerWeaponContext context);
        void Holster(PlatformerWeaponContext context);
        void PrimaryAttack(PlatformerWeaponContext context);
        void Fire(PlatformerWeaponContext context);
        void Throw(PlatformerWeaponContext context);
    }

    [DisallowMultipleComponent]
    public sealed class PlatformerWeaponController : MonoBehaviour {
        [Header("Описание")]
        [SerializeField, TextArea(3, 8)] private string inspectorDescription =
            "Контроллер оружия платформерного персонажа. Оружие и предметы задаются prefab-слотами. Prefab может реализовать IPlatformerWeapon или принимать сообщения OnPlatformerEquip/Fire/Throw.";

        [Header("Ссылки")]
        [Tooltip("Мотор персонажа.")]
        [SerializeField] private PlatformerCharacterMotor motor;
        [Tooltip("Ресурсы персонажа: патроны, метательные предметы, стамина.")]
        [SerializeField] private PlatformerCharacterVitals vitals;
        [Tooltip("Источник ввода персонажа.")]
        [SerializeField] private MonoBehaviour inputSourceComponent;
        [Tooltip("Кость/объект, куда будет создан prefab оружия.")]
        [SerializeField] private Transform defaultWeaponSocket;
        [Tooltip("Точка вылета снарядов/метательных предметов.")]
        [SerializeField] private Transform projectileOrigin;
        [Tooltip("Prefab предмета для броска, например камень или бомба.")]
        [SerializeField] private GameObject throwablePrefab;
        [Tooltip("Скорость бросаемого предмета.")]
        [SerializeField] private float throwVelocity = 11f;
        [Tooltip("Список оружия/предметов персонажа.")]
        [SerializeField] private PlatformerWeaponSlot[] weaponSlots;

        private IPlatformerInputSource inputSource;
        private GameObject activeWeaponObject;
        private IPlatformerWeapon activeWeapon;
        private int activeIndex = -1;
        private bool armed;

        public bool IsArmed => armed;
        public int ActiveIndex => activeIndex;
        public GameObject ActiveWeaponObject => activeWeaponObject;

        private void Awake()
        {
            if (motor == null)
            {
                motor = GetComponent<PlatformerCharacterMotor>();
            }

            if (vitals == null)
            {
                vitals = GetComponent<PlatformerCharacterVitals>();
            }

            inputSource = inputSourceComponent as IPlatformerInputSource;
            if (inputSource == null)
            {
                inputSource = GetComponent<IPlatformerInputSource>();
            }
        }

        private void Start()
        {
            for (int i = 0; weaponSlots != null && i < weaponSlots.Length; i++)
            {
                if (weaponSlots[i] != null && weaponSlots[i].StartsEquipped)
                {
                    Equip(i);
                    Holster();
                    break;
                }
            }
        }

        private void Update()
        {
            if (inputSource == null)
            {
                return;
            }

            PlatformerCharacterIntent intent = inputSource.CurrentIntent;

            if (intent.ArmPressed)
            {
                ToggleArmed();
            }

            if (intent.AttackPressed)
            {
                PrimaryAttack();
            }

            if (intent.FirePressed)
            {
                Fire();
            }

            if (intent.ThrowPressed)
            {
                Throw();
            }
        }

        public void ToggleArmed()
        {
            if (armed)
            {
                Holster();
            }
            else
            {
                Arm();
            }
        }

        public void Arm()
        {
            if (activeIndex < 0 && weaponSlots != null && weaponSlots.Length > 0)
            {
                Equip(0);
            }

            armed = true;
            activeWeaponObject?.SetActive(true);
            activeWeapon?.Equip(BuildContext());
            activeWeaponObject?.SendMessage("OnPlatformerEquip", BuildContext(), SendMessageOptions.DontRequireReceiver);
        }

        public void Holster()
        {
            armed = false;
            activeWeapon?.Holster(BuildContext());
            activeWeaponObject?.SendMessage("OnPlatformerHolster", BuildContext(), SendMessageOptions.DontRequireReceiver);
            activeWeaponObject?.SetActive(false);
        }

        public void Equip(int index)
        {
            if (weaponSlots == null || index < 0 || index >= weaponSlots.Length || weaponSlots[index] == null)
            {
                return;
            }

            DestroyActiveWeapon();
            PlatformerWeaponSlot slot = weaponSlots[index];
            if (slot.Prefab == null)
            {
                activeIndex = index;
                return;
            }

            Transform socket = slot.SocketOverride != null ? slot.SocketOverride : defaultWeaponSocket;
            if (socket == null)
            {
                socket = transform;
            }

            activeWeaponObject = Instantiate(slot.Prefab, socket);
            activeWeaponObject.transform.localPosition = Vector3.zero;
            activeWeaponObject.transform.localRotation = Quaternion.identity;
            activeWeapon = activeWeaponObject.GetComponent<IPlatformerWeapon>();
            activeIndex = index;
            activeWeaponObject.SetActive(armed);
        }

        public void PrimaryAttack()
        {
            if (!armed)
            {
                activeWeaponObject?.SendMessage("OnPlatformerUnarmedAttack", BuildContext(), SendMessageOptions.DontRequireReceiver);
                return;
            }

            PlatformerWeaponContext context = BuildContext();
            activeWeapon?.PrimaryAttack(context);
            activeWeaponObject?.SendMessage("OnPlatformerPrimaryAttack", context, SendMessageOptions.DontRequireReceiver);
        }

        public void Fire()
        {
            if (!armed)
            {
                return;
            }

            if (vitals != null && !vitals.TrySpendAmmo())
            {
                return;
            }

            PlatformerWeaponContext context = BuildContext();
            activeWeapon?.Fire(context);
            activeWeaponObject?.SendMessage("OnPlatformerFire", context, SendMessageOptions.DontRequireReceiver);
        }

        public void Throw()
        {
            if (vitals != null && !vitals.TrySpendThrowable())
            {
                return;
            }

            PlatformerWeaponContext context = BuildContext();
            activeWeapon?.Throw(context);
            activeWeaponObject?.SendMessage("OnPlatformerThrow", context, SendMessageOptions.DontRequireReceiver);

            if (throwablePrefab == null)
            {
                return;
            }

            Transform origin = projectileOrigin != null ? projectileOrigin : transform;
            GameObject thrown = Instantiate(throwablePrefab, origin.position, Quaternion.identity);
            if (thrown.TryGetComponent(out Rigidbody2D thrownBody))
            {
                thrownBody.linearVelocity = context.Direction * throwVelocity;
            }
        }

        private PlatformerWeaponContext BuildContext()
        {
            Transform origin = projectileOrigin != null ? projectileOrigin : defaultWeaponSocket != null ? defaultWeaponSocket : transform;
            int facing = motor != null ? motor.FacingDirection : transform.localScale.x >= 0f ? 1 : -1;
            return new PlatformerWeaponContext(gameObject, origin, Vector2.right * facing, motor, vitals);
        }

        private void DestroyActiveWeapon()
        {
            if (activeWeaponObject != null)
            {
                Destroy(activeWeaponObject);
            }

            activeWeaponObject = null;
            activeWeapon = null;
            activeIndex = -1;
        }
    }
}
