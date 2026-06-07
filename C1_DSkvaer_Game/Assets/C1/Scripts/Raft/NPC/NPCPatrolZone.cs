using UnityEngine;
using UnityEngine.Tilemaps;

namespace Ship {
    [DisallowMultipleComponent]
    public class NPCPatrolZone : MonoBehaviour {
        [Header("Зона патруля и спавна")]
        [InspectorLabel("Размер зоны")]
        [Tooltip("Размер прямоугольной зоны в локальных координатах объекта.")]
        [SerializeField] private Vector2 size = new Vector2(12f, 12f);

        [InspectorLabel("Смещение зоны")]
        [Tooltip("Локальное смещение центра зоны относительно объекта.")]
        [SerializeField] private Vector2 offset;

        [InspectorLabel("Ограничивать тайлмапом воды")]
        [Tooltip("Если включено, выбранная точка дополнительно зажимается в границах water tilemap.")]
        [SerializeField] private bool clampToWaterTilemap = true;

        public Vector2 GetRandomPoint(Tilemap waterTilemap)
        {
            Vector2 half = size * 0.5f;
            Vector2 localPoint = offset + new Vector2(
                Random.Range(-half.x, half.x),
                Random.Range(-half.y, half.y)
            );

            Vector2 worldPoint = transform.TransformPoint(localPoint);
            return clampToWaterTilemap && waterTilemap != null ? ClampToTilemap(waterTilemap, worldPoint) : worldPoint;
        }

        public bool Contains(Vector2 worldPoint)
        {
            Vector2 local = transform.InverseTransformPoint(worldPoint) - (Vector3)offset;
            Vector2 half = size * 0.5f;
            return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y;
        }

        private static Vector2 ClampToTilemap(Tilemap tilemap, Vector2 point)
        {
            Bounds bounds = tilemap.localBounds;
            Vector3 min = tilemap.transform.TransformPoint(bounds.min);
            Vector3 max = tilemap.transform.TransformPoint(bounds.max);

            return new Vector2(
                Mathf.Clamp(point.x, min.x, max.x),
                Mathf.Clamp(point.y, min.y, max.y)
            );
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.1f, 0.8f, 1f, 0.35f);
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(offset, size);
            Gizmos.color = new Color(0.1f, 0.8f, 1f, 0.9f);
            Gizmos.DrawWireCube(offset, size);
            Gizmos.matrix = previousMatrix;
        }
    }
}
