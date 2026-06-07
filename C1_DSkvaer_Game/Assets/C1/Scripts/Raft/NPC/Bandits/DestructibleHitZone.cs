using UnityEngine;
using UnityEngine.Events;

namespace Ship {
    [RequireComponent(typeof(Collider2D))]
    public class DestructibleHitZone : MonoBehaviour {
        [Header("Разрушимая зона")]
        [InspectorLabel("ID зоны")]
        [Tooltip("Системный ID зоны поражения.")]
        [SerializeField] private string zoneId = "zone";
        [InspectorLabel("Название зоны")]
        [Tooltip("Название зоны для инспектора и будущего UI.")]
        [SerializeField] private string displayName = "Hit Zone";
        [InspectorLabel("Максимальное здоровье")]
        [Tooltip("Сколько урона зона выдерживает до разрушения.")]
        [SerializeField, Min(1)] private int maxHealth = 25;
        [InspectorLabel("Множитель урона")]
        [Tooltip("Множитель входящего урона по этой зоне.")]
        [SerializeField, Min(0f)] private float damageMultiplier = 1f;
        [InspectorLabel("Отключать коллайдер")]
        [Tooltip("После разрушения отключает коллайдер, открывая урон по корпусу.")]
        [SerializeField] private bool disableColliderWhenDestroyed = true;
        [InspectorLabel("Отключать визуал")]
        [Tooltip("После разрушения отключает указанные визуальные объекты.")]
        [SerializeField] private bool disableVisualsWhenDestroyed = true;
        [InspectorLabel("Визуал для отключения")]
        [Tooltip("Объекты, которые скрываются после разрушения зоны.")]
        [SerializeField] private GameObject[] visualsToDisable;

        private BanditEnemy owner;
        private Collider2D zoneCollider;
        private int currentHealth;
        private bool isDestroyed;

        public UnityEvent OnHealthChanged { get; } = new();
        public UnityEvent OnDestroyed { get; } = new();

        public string ZoneId => zoneId;
        public string DisplayName => displayName;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsDestroyed => isDestroyed;
        public float DamageMultiplier => damageMultiplier;

        private void Awake()
        {
            zoneCollider = GetComponent<Collider2D>();
            zoneCollider.isTrigger = true;
            TrySetTag("HitArea");
            TrySetLayer("HitArea");

            if (currentHealth <= 0) {
                currentHealth = maxHealth;
            }
        }

        public void Configure(BanditEnemy zoneOwner, BanditHitZoneDefinition definition, int configuredHealth)
        {
            owner = zoneOwner;

            if (definition != null) {
                zoneId = definition.ZoneId;
                displayName = definition.DisplayName;
                maxHealth = Mathf.Max(1, configuredHealth);
                damageMultiplier = definition.DamageMultiplier;
                disableColliderWhenDestroyed = definition.DisableColliderWhenDestroyed;
                disableVisualsWhenDestroyed = definition.DisableVisualsWhenDestroyed;
            }

            currentHealth = maxHealth;
            isDestroyed = false;

            zoneCollider = GetComponent<Collider2D>();
            if (zoneCollider != null) {
                zoneCollider.isTrigger = true;
                zoneCollider.enabled = true;
            }

            SetVisualsActive(true);
            TrySetTag("HitArea");
            TrySetLayer("HitArea");
            OnHealthChanged.Invoke();
        }

        public int ApplyDamage(int amount, bool isRam)
        {
            if (isDestroyed || amount <= 0) {
                return 0;
            }

            int previousHealth = currentHealth;
            currentHealth = Mathf.Max(0, currentHealth - amount);
            int appliedDamage = previousHealth - currentHealth;

            owner?.NotifyHitZoneDamaged(this, appliedDamage, isRam);
            OnHealthChanged.Invoke();

            if (currentHealth <= 0) {
                DestroyZone();
            }

            return appliedDamage;
        }

        public void Restore()
        {
            currentHealth = maxHealth;
            isDestroyed = false;

            if (zoneCollider != null) {
                zoneCollider.enabled = true;
            }

            SetVisualsActive(true);
            OnHealthChanged.Invoke();
        }

        private void DestroyZone()
        {
            if (isDestroyed) {
                return;
            }

            isDestroyed = true;

            if (disableColliderWhenDestroyed && zoneCollider != null) {
                zoneCollider.enabled = false;
            }

            if (disableVisualsWhenDestroyed) {
                SetVisualsActive(false);
            }

            owner?.NotifyHitZoneDestroyed(this);
            OnDestroyed.Invoke();
        }

        private void SetVisualsActive(bool value)
        {
            if (visualsToDisable == null) {
                return;
            }

            foreach (GameObject visual in visualsToDisable) {
                if (visual != null) {
                    visual.SetActive(value);
                }
            }
        }

        private void TrySetTag(string tagName)
        {
            try {
                gameObject.tag = tagName;
            }
            catch (UnityException) {
                ;
            }
        }

        private void TrySetLayer(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            gameObject.layer = layer >= 0 ? layer : 0;
        }
    }
}
