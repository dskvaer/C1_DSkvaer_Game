using UnityEngine;

namespace C1.Player {
    [DisallowMultipleComponent]
    [AddComponentMenu("C1/Player/Player Health Binding")]
    public sealed class PlayerHealthBinding : MonoBehaviour {
        [SerializeField] private MonoBehaviour healthSource;
        [SerializeField] private PlayerDeathContext deathContext = PlayerDeathContext.Platformer;

        private void OnEnable()
        {
            IHealth health = ResolveHealth();
            if (deathContext == PlayerDeathContext.Platformer) {
                PlayerSystem.GetOrCreate().BindPlatformerHealth(health);
            }
            else {
                PlayerSystem.GetOrCreate().BindShipHealth(health);
            }
        }

        private IHealth ResolveHealth()
        {
            return healthSource as IHealth
                ?? GetComponent<IHealth>()
                ?? GetComponentInChildren<IHealth>(true)
                ?? GetComponentInParent<IHealth>();
        }
    }
}
