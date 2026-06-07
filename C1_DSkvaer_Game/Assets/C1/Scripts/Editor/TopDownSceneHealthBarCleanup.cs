using C1.Scripts.UI.Game_play_UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace C1.EditorTools {
    public static class TopDownSceneHealthBarCleanup {
        private const string DemoSeaPath = "Assets/C1/Scenes/Levels_Sea/DEMO_SEA.unity";

        [MenuItem("C1/UI/Cleanup Empty TopDown Health Bars")]
        public static void CleanupEmptyTopDownHealthBars()
        {
            var scene = EditorSceneManager.OpenScene(DemoSeaPath, OpenSceneMode.Single);
            int removed = 0;

            HealthBar[] healthBars = Object.FindObjectsByType<HealthBar>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < healthBars.Length; i++) {
                SerializedObject serialized = new SerializedObject(healthBars[i]);
                SerializedProperty healthComponent = serialized.FindProperty("healthComponent");
                if (healthComponent == null || healthComponent.objectReferenceValue != null) {
                    continue;
                }

                Object.DestroyImmediate(healthBars[i].gameObject);
                removed++;
            }

            if (removed > 0) {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            Debug.Log($"[TopDownSceneHealthBarCleanup] Removed empty health bars: {removed}");
        }
    }
}
