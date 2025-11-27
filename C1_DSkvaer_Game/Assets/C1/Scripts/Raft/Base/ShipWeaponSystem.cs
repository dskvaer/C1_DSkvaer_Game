// ====================================================================================================
// ShipWeaponSystem.cs
// ====================================================================================================

using UnityEngine;

namespace Ship {
    /// <summary>
    /// ”правление всеми пушками корабл€ (левый/правый борт). ¬ызывает Fire() у GunWeaponSystem.
    /// </summary>
    /// <remarks>
    /// Inspector:
    /// Х <b>Left Guns</b> Ц массив пушек левого борта.
    /// Х <b>Right Guns</b> Ц массив пушек правого борта.
    /// 
    /// Ћогика:
    /// Х ’ранит таймеры перезар€дки дл€ каждой пушки.
    /// Х FireLeft/Right Ц стрел€ет первой готовой пушкой на борту.
    /// </remarks>
    public sealed class ShipWeaponSystem : MonoBehaviour {
        [SerializeField] private GameObject[] leftGuns;
        [SerializeField] private GameObject[] rightGuns;

        private float[] leftGunTimers;
        private float[] rightGunTimers;

        private void Awake()
        {
            leftGunTimers = new float[leftGuns?.Length ?? 0];
            rightGunTimers = new float[rightGuns?.Length ?? 0];
            Debug.Log($"[ShipWeaponSystem] »нициализирован {name}. Left={leftGuns?.Length ?? 0}, Right={rightGuns?.Length ?? 0}", this);
        }

        private void Update()
        {
            for (int i = 0; i < leftGunTimers.Length; i++)
                if (leftGunTimers[i] > 0f) leftGunTimers[i] -= Time.deltaTime;
            for (int i = 0; i < rightGunTimers.Length; i++)
                if (rightGunTimers[i] > 0f) rightGunTimers[i] -= Time.deltaTime;
        }

        public void FireLeft() => FireGuns(leftGuns, leftGunTimers);
        public void FireRight() => FireGuns(rightGuns, rightGunTimers);

        private void FireGuns(GameObject[] guns, float[] timers)
        {
            if (guns == null || guns.Length == 0) return;

            bool anyFired = false;
            for (int i = 0; i < guns.Length; i++)
            {
                if (guns[i] == null) continue;
                if (timers[i] > 0f) continue;

                var gw = guns[i].GetComponent<GunWeaponSystem>();
                if (gw == null) continue;

                var cfg = gw.GetGunConfig();
                var proj = gw.Fire();
                if (proj != null)
                {
                    if (cfg != null) timers[i] = 1f / Mathf.Max(0.0001f, cfg.FireRate);
                    anyFired = true;
                }
            }

            if (!anyFired)
                Debug.Log($"[ShipWeaponSystem] ¬се пушки {name} на перезар€дке.", this);
        }
    }
}