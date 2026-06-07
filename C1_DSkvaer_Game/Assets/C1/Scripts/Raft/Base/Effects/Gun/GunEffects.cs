using UnityEngine;

namespace Ship {
    public class GunEffects : MonoBehaviour {
        [Header("Визуальные эффекты пушки")]
        [InspectorLabel("Вспышка выстрела")]
        [Tooltip("ParticleSystem вспышки у дула. Желательно выключить Play On Awake.")]
        [SerializeField] private ParticleSystem muzzleFlash;

        private void Awake()
        {
            if (muzzleFlash == null)
            {
                return;
            }

            var renderer = muzzleFlash.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.sortingLayerName = "Default";
                renderer.sortingOrder = 6;
            }
        }

        public void PlayMuzzleFlash(Vector3 position, Quaternion rotation)
        {
            if (muzzleFlash == null)
            {
                return;
            }

            muzzleFlash.transform.position = position;
            muzzleFlash.transform.rotation = rotation;
            muzzleFlash.Play();
        }

        public void PlayTrailEffect(GameObject projectile)
        {
            if (projectile == null)
            {
                return;
            }

            ProjectileEffects projectileEffects = projectile.GetComponent<ProjectileEffects>();
            if (projectileEffects != null)
            {
                projectileEffects.PlayTrailEffect();
            }
        }
    }
}
