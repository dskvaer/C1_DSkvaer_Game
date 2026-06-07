using UnityEngine;
using UnityEngine.UI;

namespace C1.Platformer.Characters {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Slider))]
    public sealed class PlatformerWorldHealthBar : MonoBehaviour {
        [SerializeField] private Slider slider;
        [SerializeField] private MonoBehaviour healthComponent;
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.4f, 0f);
        [SerializeField] private bool hideWhenFull = true;
        [SerializeField] private bool faceCamera = true;

        private global::IHealth health;
        private Camera mainCamera;

        private void Awake()
        {
            if (slider == null)
            {
                slider = GetComponent<Slider>();
            }

            if (target == null)
            {
                target = transform.parent;
            }

            global::IHealth foundHealth = healthComponent as global::IHealth;
            if (foundHealth == null && target != null)
            {
                foundHealth = target.GetComponentInParent<global::IHealth>();
                healthComponent = foundHealth as MonoBehaviour;
            }

            BindHealth(foundHealth);
            mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (target != null)
            {
                transform.position = target.position + worldOffset;
            }

            if (faceCamera)
            {
                if (mainCamera == null)
                {
                    mainCamera = Camera.main;
                }

                if (mainCamera != null)
                {
                    transform.forward = mainCamera.transform.forward;
                }
            }
        }

        public void BindHealth(global::IHealth newHealth)
        {
            if (health != null)
            {
                health.OnHealthChanged.RemoveListener(UpdateBar);
                health.OnDeath.RemoveListener(UpdateBar);
            }

            health = newHealth;
            if (health == null)
            {
                return;
            }

            health.OnHealthChanged.AddListener(UpdateBar);
            health.OnDeath.AddListener(UpdateBar);
            UpdateBar();
        }

        private void UpdateBar()
        {
            if (health == null || slider == null)
            {
                return;
            }

            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = health.MaxHealth > 0 ? (float)health.CurrentHealth / health.MaxHealth : 0f;
            gameObject.SetActive(!hideWhenFull || slider.value < 0.999f);
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.OnHealthChanged.RemoveListener(UpdateBar);
                health.OnDeath.RemoveListener(UpdateBar);
            }
        }
    }
}
