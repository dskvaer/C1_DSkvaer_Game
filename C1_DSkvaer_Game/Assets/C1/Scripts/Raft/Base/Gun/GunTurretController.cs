using UnityEngine;

namespace Ship {
    public sealed class GunTurretController : MonoBehaviour {
        [Header("Башня")]
        [InspectorLabel("Поворотная часть")]
        [Tooltip("Transform, который будет вращаться при наведении турели.")]
        [SerializeField] private Transform turretPivot;
        [InspectorLabel("Направление ствола")]
        [Tooltip("Transform, чей локальный верх считается направлением стрельбы. Если пусто, используется поворотная часть.")]
        [SerializeField] private Transform forwardReference;
        [InspectorLabel("Скорость поворота")]
        [Tooltip("Максимальная скорость наведения башни в градусах в секунду.")]
        [SerializeField, Min(1f)] private float turnSpeed = 360f;

        [Header("Ограничение угла")]
        [InspectorLabel("Ограничить сектор")]
        [Tooltip("Если включено, башня не сможет повернуться дальше заданных локальных углов.")]
        [SerializeField] private bool limitLocalArc;
        [InspectorLabel("Минимальный локальный угол")]
        [Tooltip("Нижняя граница сектора поворота башни относительно родителя.")]
        [SerializeField, Range(-180f, 180f)] private float minLocalAngle = -90f;
        [InspectorLabel("Максимальный локальный угол")]
        [Tooltip("Верхняя граница сектора поворота башни относительно родителя.")]
        [SerializeField, Range(-180f, 180f)] private float maxLocalAngle = 90f;

        private void Awake()
        {
            if (turretPivot == null) {
                turretPivot = transform;
            }

            if (forwardReference == null) {
                forwardReference = turretPivot;
            }
        }

        public float AimAt(Vector2 worldPoint, float deltaTime)
        {
            if (turretPivot == null || forwardReference == null) {
                return 180f;
            }

            Vector2 toTarget = worldPoint - (Vector2)forwardReference.position;
            if (toTarget.sqrMagnitude <= 0.0001f) {
                return 0f;
            }

            float angleToTarget = Vector2.SignedAngle(forwardReference.up, toTarget.normalized);
            float step = Mathf.Clamp(angleToTarget, -turnSpeed * deltaTime, turnSpeed * deltaTime);
            turretPivot.Rotate(0f, 0f, step);

            if (limitLocalArc) {
                ClampLocalAngle();
            }

            return GetAimError(worldPoint);
        }

        public float GetAimError(Vector2 worldPoint)
        {
            if (forwardReference == null) {
                return 180f;
            }

            Vector2 toTarget = worldPoint - (Vector2)forwardReference.position;
            if (toTarget.sqrMagnitude <= 0.0001f) {
                return 0f;
            }

            return Mathf.Abs(Vector2.SignedAngle(forwardReference.up, toTarget.normalized));
        }

        private void ClampLocalAngle()
        {
            float min = Mathf.Min(minLocalAngle, maxLocalAngle);
            float max = Mathf.Max(minLocalAngle, maxLocalAngle);
            Vector3 localEuler = turretPivot.localEulerAngles;
            float clampedZ = Mathf.Clamp(Mathf.DeltaAngle(0f, localEuler.z), min, max);
            turretPivot.localRotation = Quaternion.Euler(localEuler.x, localEuler.y, clampedZ);
        }
    }
}
