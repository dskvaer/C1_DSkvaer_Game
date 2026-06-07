using UnityEngine;

#pragma warning disable 0414

namespace C1.Platformer.Characters {
    [DisallowMultipleComponent]
    public sealed class PlatformerInteractionSensor : MonoBehaviour {
        [Header("Описание")]
        [SerializeField, TextArea(3, 7)] private string inspectorDescription =
            "Сенсор взаимодействия. Ищет ближайший объект с IPlatformerInteractable и вызывает Interact по кнопке взаимодействия. Подходит для дверей, кнопок, рычагов, сундуков и NPC.";

        [Header("Ссылки")]
        [Tooltip("Мотор персонажа. Нужен, чтобы не взаимодействовать во время толкания, если это запрещено.")]
        [SerializeField] private PlatformerCharacterMotor motor;
        [Tooltip("Источник ввода персонажа.")]
        [SerializeField] private MonoBehaviour inputSourceComponent;
        [Tooltip("Точка поиска интерактивных объектов. Если не указана, используется позиция персонажа.")]
        [SerializeField] private Transform origin;
        [Tooltip("Радиус поиска интерактивных объектов.")]
        [SerializeField] private float radius = 0.75f;
        [Tooltip("Слои интерактивных объектов.")]
        [SerializeField] private LayerMask interactableLayers;
        [Tooltip("Если включено, кнопка взаимодействия во время толкания не активирует рычаги/двери.")]
        [SerializeField] private bool ignoreWhilePushing = true;

        private IPlatformerInputSource inputSource;
        private IPlatformerInteractable currentInteractable;
        private Collider2D currentCollider;

        public IPlatformerInteractable CurrentInteractable => currentInteractable;
        public Collider2D CurrentCollider => currentCollider;

        private void Awake()
        {
            if (motor == null)
            {
                motor = GetComponent<PlatformerCharacterMotor>();
            }

            inputSource = inputSourceComponent as IPlatformerInputSource;
            if (inputSource == null)
            {
                inputSource = GetComponent<IPlatformerInputSource>();
            }
        }

        private void Update()
        {
            FindCurrentInteractable();

            if (currentInteractable == null || inputSource == null)
            {
                return;
            }

            PlatformerCharacterIntent intent = inputSource.CurrentIntent;
            if (ignoreWhilePushing && motor != null && motor.IsPushing)
            {
                return;
            }

            if (intent.InteractPressed && currentInteractable.CanInteract(gameObject))
            {
                currentInteractable.Interact(gameObject);
            }
        }

        private void FindCurrentInteractable()
        {
            currentInteractable = null;
            currentCollider = null;

            Vector2 center = origin != null ? origin.position : transform.position;
            int mask = interactableLayers.value != 0 ? interactableLayers.value : Physics2D.DefaultRaycastLayers;
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, mask);
            float bestDistance = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hit = hits[i];
                if (hit == null || hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                IPlatformerInteractable interactable = hit.GetComponentInParent<IPlatformerInteractable>();
                if (interactable == null || !interactable.CanInteract(gameObject))
                {
                    continue;
                }

                float distance = Vector2.Distance(center, hit.ClosestPoint(center));
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    currentInteractable = interactable;
                    currentCollider = hit;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(origin != null ? origin.position : transform.position, radius);
        }
    }
}
