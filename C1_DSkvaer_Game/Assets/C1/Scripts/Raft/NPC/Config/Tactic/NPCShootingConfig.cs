using UnityEngine;

namespace Ship {
    [CreateAssetMenu(fileName = "ShootingConfig", menuName = "ShipConfigs/TacticConfigs/Shooting Config")]
    public class NPCShootingConfig : ScriptableObject {
        [Header("Точность")]
        [Tooltip("Если угол между дулом и целью меньше этого значения (в градусах), пушка выстрелит.")]
        [Range(1f, 45f)] public float FiringAngleThreshold = 10f;

        [Header("Настройки Поворота")]
        [Tooltip("При каком угле отклонения включается максимальная скорость (градусы).")]
        [Range(10f, 1800f)] public float FastTurnAngle = 90f;

        [Tooltip("Погрешность остановки вращения (градусы). Чтобы корабль не дрожал.")]
        [Range(0.1f, 10f)] public float RotationAimTolerance = 2f;

        [Space(10)]
        [Header("Минимальная Скорость (Доворот)")]
        [Tooltip("Базовая скорость (градусы/сек) для точного наведения.")]
        [Range(10f, 3600f)] public float MinTurnSpeed = 30f;

        [Tooltip("Множитель мин. скорости (x1, x2...).")]
        [Range(1f, 100f)] public float MinTurnSpeedMultiplier = 1f;

        [Space(10)]
        [Header("Максимальная Скорость (Разворот)")]
        [Tooltip("Базовая скорость (градусы/сек) для быстрого разворота.")]
        [Range(10f, 3600f)] public float MaxTurnSpeed = 360f;

        [Tooltip("Множитель макс. скорости (x1, x2...).")]
        [Range(1f, 100f)] public float MaxTurnSpeedMultiplier = 1f;
    }
}