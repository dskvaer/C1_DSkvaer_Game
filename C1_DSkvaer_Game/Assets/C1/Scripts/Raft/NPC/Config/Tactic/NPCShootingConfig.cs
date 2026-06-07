using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ship {
    [CreateAssetMenu(fileName = "ShootingConfig", menuName = "ShipConfigs/TacticConfigs/Shooting Config")]
    public class NPCShootingConfig : ScriptableObject {

        [Header("Стрельба (Fire)")]

        [Tooltip("Если угол между направлением орудия и целью меньше этого значения, орудие может выстрелить.")]
        [InspectorLabel("Порог угла стрельбы")]
        [Range(1f, 45f)] public float FiringAngleThreshold = 10f;

        [Tooltip("Разброс выстрелов NPC. 0 означает абсолютно точные выстрелы.")]
        [InspectorLabel("Угол разброса выстрела")]
        [Range(0f, 45f)] public float ShotSpreadAngle = 6f;

        [Tooltip("Задержка между выстрелами орудий NPC, когда готовы сразу несколько.")]
        [InspectorLabel("Задержка серии выстрелов")]
        [Range(0f, 2f)] public float SequentialGunDelay = 0.15f;

        [Tooltip("Переключаться на другой борт только в том случае, если угол прицеливания его орудия лучше на это значение.")]
        [InspectorLabel("Порог смены борта (угол)")]
        [Range(0f, 90f)] public float SideSwitchAngleThreshold = 30f;

        [Tooltip("Сколько времени NPC помнит цель после того, как сонар её теряет.")]
        [InspectorLabel("Время памяти о цели")]
        [Range(0f, 3f)] public float TargetMemoryTime = 0.75f;


        [Header("Вращение корпуса (Body Rotation)")]

        [InspectorLabel("Угол быстрого поворота")]
        [Range(10f, 1800f)] public float FastTurnAngle = 90f;

        [Tooltip("Допустимая погрешность при прицеливании вращением.")]
        [InspectorLabel("Погрешность прицеливания")]
        [Range(0.1f, 10f)] public float RotationAimTolerance = 2f;


        [Header("Мин. скорость поворота (Min Turn Speed)")]

        [InspectorLabel("Мин. скорость поворота")]
        [Range(10f, 3600f)] public float MinTurnSpeed = 30f;

        [InspectorLabel("Множитель мин. скорости")]
        [Range(1f, 100f)] public float MinTurnSpeedMultiplier = 1f;


        [Header("Макс. скорость поворота (Max Turn Speed)")]

        [InspectorLabel("Макс. скорость поворота")]
        [Range(10f, 3600f)] public float MaxTurnSpeed = 360f;

        [InspectorLabel("Множитель макс. скорости")]
        [Range(1f, 100f)] public float MaxTurnSpeedMultiplier = 1f;
    }
}