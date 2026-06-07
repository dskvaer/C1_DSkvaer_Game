using UnityEngine;
using Menu_Journal.Data;
using Menu_Journal.Systems;

namespace Gameplay {
    public class LootDropper : MonoBehaviour {
        [Header("Лут")]
        [InspectorLabel("Профиль лута")]
        [Tooltip("ScriptableObject со списком предметов, которые может выбросить этот объект.")]
        [SerializeField] private LootProfileSO _lootProfile;

        public void DropLoot()
        {
            if (LootSpawner.Instance != null)
            {
                LootSpawner.Instance.SpawnFromProfile(_lootProfile, transform.position);
            }
            else
            {
                Debug.LogWarning("LootSpawner не найден на сцене! Лут не выпал.", this);
            }
        }
    }
}
