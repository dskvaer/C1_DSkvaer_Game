using UnityEngine;

namespace Ship {
    public static class BanditTacticUtility {
        public static Vector2 PlayerPosition(EnemyAIContext context)
        {
            return context.Player != null ? (Vector2)context.Player.position : context.Rigidbody.position;
        }

        public static Vector2 DirectionToPlayer(EnemyAIContext context)
        {
            Vector2 toPlayer = PlayerPosition(context) - context.Rigidbody.position;
            return toPlayer.sqrMagnitude > 0.001f ? toPlayer.normalized : (Vector2)context.Rigidbody.transform.up;
        }

        public static Vector2 DirectionFromPlayer(EnemyAIContext context)
        {
            Vector2 fromPlayer = context.Rigidbody.position - PlayerPosition(context);
            return fromPlayer.sqrMagnitude > 0.001f ? fromPlayer.normalized : -(Vector2)context.Rigidbody.transform.up;
        }

        public static Vector2 Perpendicular(Vector2 direction, float side)
        {
            return new Vector2(-direction.y, direction.x) * Mathf.Sign(side == 0f ? 1f : side);
        }

        public static Vector2 ClampToWater(EnemyAIContext context, Vector2 point)
        {
            if (context.WaterTilemap == null) {
                return point;
            }

            Bounds bounds = context.WaterTilemap.localBounds;
            Vector3 min = context.WaterTilemap.transform.TransformPoint(bounds.min);
            Vector3 max = context.WaterTilemap.transform.TransformPoint(bounds.max);
            return new Vector2(Mathf.Clamp(point.x, min.x, max.x), Mathf.Clamp(point.y, min.y, max.y));
        }

        public static void FireBroadside(EnemyAIContext context)
        {
            if (context.WeaponSystem == null) {
                return;
            }

            context.WeaponSystem.FireLeft();
            context.WeaponSystem.FireRight();
        }
    }
}
