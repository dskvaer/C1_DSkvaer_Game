using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace C1.Player {
    [AddComponentMenu("C1/Player/Player HUD Bridge")]
    public sealed class PlayerHudBridge : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private CanvasGroup messageGroup;
        [SerializeField] private Toggle saveProgressToggle;

        private float hideAtTime;

        private void OnEnable()
        {
            PlayerSystem player = PlayerSystem.GetOrCreate();
            player.OnHudMessage += ShowMessage;

            if (saveProgressToggle != null) {
                saveProgressToggle.isOn = player.SaveProgress;
                saveProgressToggle.onValueChanged.AddListener(SetSaveProgress);
            }

            HideMessage();
        }

        private void OnDisable()
        {
            if (PlayerSystem.Instance != null) {
                PlayerSystem.Instance.OnHudMessage -= ShowMessage;
            }

            if (saveProgressToggle != null) {
                saveProgressToggle.onValueChanged.RemoveListener(SetSaveProgress);
            }
        }

        private void Update()
        {
            if (messageGroup != null && messageGroup.alpha > 0f && Time.unscaledTime >= hideAtTime) {
                HideMessage();
            }
        }

        public void SetSaveProgress(bool enabled)
        {
            PlayerSystem.GetOrCreate().SaveProgress = enabled;
        }

        private void ShowMessage(PlayerHudMessage message)
        {
            if (titleText != null) titleText.text = message.Title;
            if (bodyText != null) bodyText.text = message.Body;

            if (messageGroup != null) {
                messageGroup.alpha = 1f;
                messageGroup.blocksRaycasts = false;
            }

            hideAtTime = Time.unscaledTime + Mathf.Max(0.25f, message.Duration);
        }

        private void HideMessage()
        {
            if (messageGroup != null) {
                messageGroup.alpha = 0f;
                messageGroup.blocksRaycasts = false;
            }
        }
    }
}
