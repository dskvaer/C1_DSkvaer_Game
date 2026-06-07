using UnityEngine;

namespace Ship {
    public class ShipSway : MonoBehaviour {
        [Header("Покачивание")]
        [InspectorLabel("Конфиг покачивания")]
        [Tooltip("ScriptableObject с амплитудой и периодом покачивания визуала корабля.")]
        [SerializeField] private ShipSwayConfig config;

        private void Awake()
        {
            if (config == null)
            {
                Debug.LogError($"ShipSwayConfig не назначен для {gameObject.name}!", this);
                enabled = false;
            }
        }

        private void Start()
        {
            if (config == null)
            {
                return;
            }

            LeanTween.moveLocalX(gameObject, config.SwayAmplitude, config.SwayPeriod / 2f)
                .setLoopPingPong()
                .setEase(LeanTweenType.easeInOutSine);
        }
    }
}
