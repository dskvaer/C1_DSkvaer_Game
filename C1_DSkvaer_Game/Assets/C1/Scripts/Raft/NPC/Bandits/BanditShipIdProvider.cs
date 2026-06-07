using UnityEngine;

namespace Ship {
    [DisallowMultipleComponent]
    public class BanditShipIdProvider : MonoBehaviour, IShipIdProvider {
        [Header("ID бандита")]
        [InspectorLabel("Компонент бандита")]
        [Tooltip("BanditEnemy, из которого берется конфиг и текущий уникальный ID.")]
        [SerializeField] private BanditEnemy banditEnemy;

        [InspectorLabel("Переопределение конфига")]
        [Tooltip("Если указано, ID будет строиться по этому BanditEnemyTypeConfig вместо конфига BanditEnemy.")]
        [SerializeField] private BanditEnemyTypeConfig configOverride;

        [InspectorLabel("Подсказка тега")]
        [Tooltip("Тег корабля для систем, которым нужно быстро понять тип владельца ID.")]
        [SerializeField] private string tagHint = "Enemy";

        public string TagHint => tagHint;

        public string CreateId()
        {
            BanditEnemyTypeConfig sourceConfig = configOverride;
            if (sourceConfig == null) {
                sourceConfig = banditEnemy != null ? banditEnemy.Config : GetComponent<BanditEnemy>()?.Config;
            }

            return sourceConfig != null ? sourceConfig.CreateEnemyId() : string.Empty;
        }
    }
}
