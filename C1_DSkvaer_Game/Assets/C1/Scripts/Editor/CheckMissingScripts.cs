#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace C1.EditorTools {
    public static class CheckMissingScripts {
        [MenuItem("C1/Debug/Check Missing Scripts")]
        public static void Check() {
            int missingComponentsCount = 0;
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            foreach (string path in allAssetPaths) {
                if (path.StartsWith("Assets/C1/") && path.EndsWith(".prefab")) {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null) {
                        Component[] components = prefab.GetComponentsInChildren<Component>(true);
                        foreach (Component c in components) {
                            if (c == null) {
                                Debug.LogWarning($"[MissingScript] Prefab '{path}' has a missing script component.");
                                missingComponentsCount++;
                            }
                        }
                    }
                }
            }
            Debug.Log($"[CheckMissingScripts] Finished. Total missing script components found: {missingComponentsCount}");
        }
    }
}
#endif
