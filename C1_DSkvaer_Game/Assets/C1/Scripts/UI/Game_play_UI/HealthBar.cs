using UnityEngine;
using UnityEngine.UI;

namespace C1.Scripts.UI.Game_play_UI
{
    [RequireComponent(typeof(Slider))]
    public class HealthBar : MonoBehaviour {
        [SerializeField] private Slider healthSlider;
        [SerializeField] private MonoBehaviour healthComponent;

        private IHealth health;

        private void Awake()
        {
            if (healthSlider == null) {
                healthSlider = GetComponent<Slider>();
                if (healthSlider == null) {
                    Debug.LogError($"Slider is missing for {gameObject.name}.", this);
                    enabled = false;
                    return;
                }
            }

            if (healthComponent != null) {
                SetHealthComponent(healthComponent as IHealth);
            }
            else {
                HideBarVisuals();
            }
        }

        private void Start()
        {
            if (health != null && healthSlider != null) {
                healthSlider.maxValue = 1f;
                UpdateBar();
            }
        }

        public void SetHealthComponent(IHealth healthComp)
        {
            if (healthComp == null && healthComponent != null) {
                healthComp = healthComponent.GetComponent<IHealth>();
            }

            if (healthComp == null) {
                HideBarVisuals();
                return;
            }

            if (health != null) {
                health.OnHealthChanged.RemoveListener(UpdateBar);
                health.OnDeath.RemoveListener(OnDeath);
            }

            health = healthComp;
            if (healthComponent == null && healthComp is Component component) {
                healthComponent = component as MonoBehaviour;
            }

            health.OnHealthChanged.AddListener(UpdateBar);
            health.OnDeath.AddListener(OnDeath);
            ShowBarVisuals();
            UpdateBar();
        }

        private void UpdateBar()
        {
            if (health == null || healthSlider == null) {
                HideBarVisuals();
                return;
            }

            float maxHealth = health.MaxHealth;
            healthSlider.value = maxHealth > 0f ? (float)health.CurrentHealth / maxHealth : 0f;
        }

        private void OnDeath()
        {
            if (healthSlider != null) {
                LeanTween.value(gameObject, v => healthSlider.value = v, healthSlider.value, 0f, 0.5f);
            }
        }

        private void HideBarVisuals()
        {
            if (healthSlider != null) {
                healthSlider.gameObject.SetActive(false);
            }
        }

        private void ShowBarVisuals()
        {
            if (healthSlider != null) {
                healthSlider.gameObject.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            if (health != null) {
                health.OnHealthChanged.RemoveListener(UpdateBar);
                health.OnDeath.RemoveListener(OnDeath);
            }
        }
    }
}
