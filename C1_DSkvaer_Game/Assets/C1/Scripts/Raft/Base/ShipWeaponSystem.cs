using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ship {
    public sealed class ShipWeaponSystem : MonoBehaviour {
        [Header("Орудия")]
        [InspectorLabel("Левые пушки")]
        [Tooltip("Пушки левого борта. При удержании левой кнопки все они показывают прицеливание и стреляют очередью.")]
        [SerializeField] private GameObject[] leftGuns;

        [InspectorLabel("Правые пушки")]
        [Tooltip("Пушки правого борта. При удержании правой кнопки все они показывают прицеливание и стреляют очередью.")]
        [SerializeField] private GameObject[] rightGuns;

        [Header("Очередь выстрелов")]
        [InspectorLabel("Задержка по умолчанию")]
        [Tooltip("Пауза между выстрелами нескольких пушек, если у самой пушки нет своего GunConfig.")]
        [SerializeField, Min(0f)] private float defaultSequentialFireDelay = 0.12f;

        private readonly AimState leftAim = new();
        private readonly AimState rightAim = new();
        private Coroutine leftFireRoutine;
        private Coroutine rightFireRoutine;

        private sealed class AimState {
            public bool IsAiming;
            public float HoldTime;
        }

        private void Update()
        {
            UpdateAim(leftGuns, leftAim);
            UpdateAim(rightGuns, rightAim);
        }

        public void BeginAimingLeft() => BeginAiming(leftGuns, leftAim);
        public void BeginAimingRight() => BeginAiming(rightGuns, rightAim);
        public void ReleaseFireLeft() => ReleaseFire(leftGuns, leftAim, ref leftFireRoutine);
        public void ReleaseFireRight() => ReleaseFire(rightGuns, rightAim, ref rightFireRoutine);

        public void FireLeft()
        {
            BeginAimingLeft();
            ReleaseFireLeft();
        }

        public void FireRight()
        {
            BeginAimingRight();
            ReleaseFireRight();
        }

        private void BeginAiming(GameObject[] guns, AimState state)
        {
            state.IsAiming = true;
            state.HoldTime = 0f;
            UpdateAimPreview(guns, state, true);
        }

        private void ReleaseFire(GameObject[] guns, AimState state, ref Coroutine routine)
        {
            float holdTime = Mathf.Max(0f, state.HoldTime);
            state.IsAiming = false;
            UpdateAimPreview(guns, state, false);

            if (routine != null) {
                StopCoroutine(routine);
            }

            routine = StartCoroutine(FireSequentially(guns, holdTime));
        }

        private void UpdateAim(GameObject[] guns, AimState state)
        {
            if (!state.IsAiming) {
                return;
            }

            state.HoldTime += Time.deltaTime;
            UpdateAimPreview(guns, state, true);
        }

        private IEnumerator FireSequentially(GameObject[] guns, float holdTime)
        {
            List<GunWeaponSystem> readyGuns = CollectReadyGuns(guns);
            for (int i = 0; i < readyGuns.Count; i++) {
                GunWeaponSystem gun = readyGuns[i];
                if (gun == null) {
                    continue;
                }

                float spread = gun.GetCurrentSpreadForHold(holdTime);
                gun.FireWithSpread(spread);

                if (i < readyGuns.Count - 1) {
                    yield return new WaitForSeconds(GetDelay(gun));
                }
            }
        }

        private List<GunWeaponSystem> CollectReadyGuns(GameObject[] gunObjects)
        {
            var result = new List<GunWeaponSystem>();
            if (gunObjects == null) {
                return result;
            }

            foreach (GameObject gunObject in gunObjects) {
                if (gunObject == null) {
                    continue;
                }

                GunWeaponSystem gun = gunObject.GetComponent<GunWeaponSystem>();
                if (gun != null && gun.CanFire()) {
                    result.Add(gun);
                }
            }

            return result;
        }

        private void UpdateAimPreview(GameObject[] gunObjects, AimState state, bool visible)
        {
            if (gunObjects == null) {
                return;
            }

            foreach (GameObject gunObject in gunObjects) {
                if (gunObject == null) {
                    continue;
                }

                GunWeaponSystem gun = gunObject.GetComponent<GunWeaponSystem>();
                GunAimZone aimZone = gunObject.GetComponentInChildren<GunAimZone>(true);
                if (gun == null || aimZone == null) {
                    continue;
                }

                float maxSpread = gun.GetMaxAimSpread();
                float currentSpread = gun.GetCurrentSpreadForHold(state.HoldTime);
                aimZone.SetAimPreview(visible, maxSpread, currentSpread, aimZone.GetAimRange());
            }
        }

        private float GetDelay(GunWeaponSystem gun)
        {
            return gun != null ? Mathf.Max(defaultSequentialFireDelay, gun.GetSequentialFireDelay()) : defaultSequentialFireDelay;
        }
    }
}
