using UnityEngine;

namespace Ship {
    [RequireComponent(typeof(Collider2D))]
    public class GunAimZone : MonoBehaviour {
        [Header("Обнаружение цели")]
        [InspectorLabel("Угол зоны прицеливания")]
        [Tooltip("Угол, внутри которого цель считается доступной для этой пушки.")]
        [SerializeField, Range(1f, 180f)] private float aimAngle = 60f;
        [InspectorLabel("Тег цели")]
        [Tooltip("Тег объекта, который зона прицеливания должна считать целью.")]
        [SerializeField] private string targetTag = "Player";

        [Header("Отображение прицела")]
        [InspectorLabel("Линия максимального разброса")]
        [Tooltip("LineRenderer для внешнего конуса, показывающего максимальный разброс.")]
        [SerializeField] private LineRenderer maxSpreadRenderer;
        [InspectorLabel("Линия текущего разброса")]
        [Tooltip("LineRenderer для текущего конуса, который сужается при удержании выстрела.")]
        [SerializeField] private LineRenderer currentSpreadRenderer;
        [InspectorLabel("Сегменты дуги")]
        [Tooltip("Количество сегментов для отрисовки дуги прицела. Больше сегментов - плавнее линия.")]
        [SerializeField, Range(4, 32)] private int arcSegments = 12;
        [InspectorLabel("Цвет максимального разброса")]
        [Tooltip("Цвет внешнего конуса максимальной зоны поражения.")]
        [SerializeField] private Color maxSpreadColor = new Color(1f, 0.85f, 0.15f, 0.35f);
        [InspectorLabel("Цвет текущего разброса")]
        [Tooltip("Цвет внутреннего конуса текущего сведения.")]
        [SerializeField] private Color currentSpreadColor = new Color(0.2f, 1f, 0.75f, 0.85f);

        private GunWeaponSystem weaponSystem;
        private ProjectileConfig projectileConfig;
        private Transform target;

        private void Awake()
        {
            weaponSystem = GetComponentInParent<GunWeaponSystem>();
            projectileConfig = weaponSystem != null ? weaponSystem.GetProjectileConfig() : null;

            Collider2D col = GetComponent<Collider2D>();
            col.isTrigger = true;
            if (projectileConfig != null && col is CircleCollider2D circle) {
                circle.radius = projectileConfig.Range;
            }

            EnsureRenderer(ref maxSpreadRenderer, "MaxSpreadPreview", maxSpreadColor);
            EnsureRenderer(ref currentSpreadRenderer, "CurrentSpreadPreview", currentSpreadColor);
            SetAimPreview(false, aimAngle, aimAngle, GetAimRange());
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.CompareTag(targetTag) || target != null) {
                return;
            }

            Vector2 toTarget = other.transform.position - transform.position;
            float angle = Vector2.Angle(transform.up, toTarget.normalized);
            if (angle <= aimAngle * 0.5f) {
                target = other.transform;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.transform == target) {
                target = null;
            }
        }

        public void SetAimPreview(bool visible, float maxSpreadAngle, float currentSpreadAngle, float range)
        {
            if (maxSpreadRenderer != null) {
                maxSpreadRenderer.enabled = visible;
                DrawCone(maxSpreadRenderer, maxSpreadAngle, range);
            }

            if (currentSpreadRenderer != null) {
                currentSpreadRenderer.enabled = visible;
                DrawCone(currentSpreadRenderer, currentSpreadAngle, range);
            }
        }

        public bool CanSeeTarget() => target != null;
        public Transform GetTarget() => target;
        public float GetAimRange() => projectileConfig != null ? projectileConfig.Range : 10f;

        private void DrawCone(LineRenderer renderer, float angle, float range)
        {
            if (renderer == null) {
                return;
            }

            int pointCount = Mathf.Max(4, arcSegments + 3);
            renderer.positionCount = pointCount;
            renderer.SetPosition(0, transform.position);

            float halfAngle = Mathf.Max(0f, angle) * 0.5f;
            for (int i = 0; i <= arcSegments; i++) {
                float t = arcSegments == 0 ? 0f : i / (float)arcSegments;
                float localAngle = Mathf.Lerp(-halfAngle, halfAngle, t);
                Vector3 dir = Quaternion.Euler(0f, 0f, localAngle) * transform.up;
                renderer.SetPosition(i + 1, transform.position + dir * range);
            }

            renderer.SetPosition(pointCount - 1, transform.position);
        }

        private void EnsureRenderer(ref LineRenderer renderer, string objectName, Color color)
        {
            if (renderer == null) {
                Transform existing = transform.Find(objectName);
                if (existing != null) {
                    renderer = existing.GetComponent<LineRenderer>();
                }
            }

            if (renderer == null) {
                GameObject rendererObject = new GameObject(objectName);
                rendererObject.transform.SetParent(transform, false);
                renderer = rendererObject.AddComponent<LineRenderer>();
            }

            renderer.useWorldSpace = true;
            renderer.loop = false;
            renderer.widthMultiplier = 0.04f;
            renderer.startColor = color;
            renderer.endColor = color;
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.enabled = false;
        }
    }
}
