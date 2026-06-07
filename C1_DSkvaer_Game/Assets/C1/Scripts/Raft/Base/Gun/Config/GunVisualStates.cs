using UnityEngine;

namespace Ship {
    [RequireComponent(typeof(SpriteRenderer))]
    public class GunVisualStates : MonoBehaviour {
        [Header("Спрайты состояний")]
        [InspectorLabel("Готова к выстрелу")]
        [Tooltip("Спрайт пушки, когда она заряжена и готова стрелять.")]
        [SerializeField] private Sprite readySprite;

        [InspectorLabel("Перезарядка")]
        [Tooltip("Спрайт пушки во время перезарядки.")]
        [SerializeField] private Sprite reloadingSprite;

        [InspectorLabel("Нет боеприпасов")]
        [Tooltip("Спрайт пустой пушки, если в будущем появится режим без боеприпасов.")]
        [SerializeField] private Sprite emptySprite;

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError($"[GunVisualStates] SpriteRenderer не найден на {name}");
                enabled = false;
                return;
            }

            if (readySprite == null || reloadingSprite == null)
            {
                Debug.LogError($"[GunVisualStates] Не назначены основные спрайты на {name}");
                enabled = false;
                return;
            }

            SetReadyState();
        }

        public void SetReadyState()
        {
            if (spriteRenderer && readySprite)
                spriteRenderer.sprite = readySprite;
        }

        public void SetReloadingState()
        {
            if (spriteRenderer && reloadingSprite)
                spriteRenderer.sprite = reloadingSprite;
        }

        public void SetEmptyState()
        {
            if (spriteRenderer && emptySprite)
                spriteRenderer.sprite = emptySprite;
        }
    }
}
