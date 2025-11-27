using UnityEngine;

namespace Ship {
    [RequireComponent(typeof(SpriteRenderer))]
    public class GunVisualStates : MonoBehaviour {
        [Header("Sprites")]
        [SerializeField] private Sprite readySprite;      // Натянутая тетива (готов к выстрелу)
        [SerializeField] private Sprite reloadingSprite;  // Спущенная тетива (перезарядка)
        [SerializeField] private Sprite emptySprite;      // Пустая пушка (нет боеприпасов)

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError($"[GunVisualStates] SpriteRenderer не найден у {name}");
                enabled = false;
                return;
            }

            if (readySprite == null || reloadingSprite == null)
            {
                Debug.LogError($"[GunVisualStates] Не назначены все спрайты у {name}");
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
