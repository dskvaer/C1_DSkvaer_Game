using System;
using System.Collections.Generic;
using Menu_Journal.Data;
using Menu_Journal.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Menu_Journal {
    public enum ContainerSize { Small, Medium, Large }

    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SimpleInventory))]
    public class LootContainer : MonoBehaviour {
        [Header("Конфигурация")]
        [InspectorLabel("Тема по умолчанию")]
        [Tooltip("Тема контейнера, если при создании не передана отдельная тема.")]
        [SerializeField] private ContainerThemeSO _defaultTheme;

        [Header("Настройки контейнера")]
        [InspectorLabel("Размер")]
        [Tooltip("Размер контейнера. Обычно рассчитывается автоматически по весу груза.")]
        [SerializeField] private ContainerSize _size = ContainerSize.Small;
        [InspectorLabel("Время жизни")]
        [Tooltip("Сколько секунд контейнер остается в мире до исчезновения.")]
        [SerializeField] private float _maxHealthTime = 300f;

        [Header("Визуал")]
        [InspectorLabel("SpriteRenderer")]
        [Tooltip("Renderer, на котором меняется спрайт контейнера по размеру и теме.")]
        [SerializeField] private SpriteRenderer _renderer;

        [Header("UI взаимодействия")]
        [InspectorLabel("Кнопка взаимодействия")]
        [Tooltip("Объект кнопки/подсказки, который появляется рядом с контейнером при входе игрока в зону.")]
        [SerializeField] private GameObject _interactionUI;
        [InspectorLabel("Смещение кнопки")]
        [Tooltip("Позиция кнопки относительно контейнера в мировых координатах.")]
        [SerializeField] private Vector3 _interactionOffset = new Vector3(0f, 1.4f, 0f);
        [InspectorLabel("Размер кнопки")]
        [Tooltip("Масштаб world-space Canvas кнопки взаимодействия.")]
        [SerializeField, Min(0.001f)] private float _interactionCanvasScale = 0.01f;
        [InspectorLabel("Всплывающее окно")]
        [Tooltip("Единое окно подтверждения открытия контейнера.")]
        [SerializeField] private UniversalPopup _interactionPopup;
        [InspectorLabel("Заголовок окна")]
        [Tooltip("Заголовок всплывающего окна при взаимодействии с контейнером.")]
        [SerializeField] private string _interactionTitle = "Контейнер";
        [InspectorLabel("Текст вопроса")]
        [Tooltip("Основной текст вопроса перед открытием окна лута.")]
        [SerializeField] private string _interactionMessage = "Открыть контейнер?";

        private SimpleInventory _inventory;
        private string _containerID;
        private float _currentHealthTime;
        private bool _isInitialized;
        private bool _isPlayerInRange;
        private ContainerThemeSO _activeTheme;
        private Button _interactionButton;
        private Transform _interactionRoot;

        public static event Action<LootContainer> OnContainerInteracted;

        public string ContainerID => _containerID;
        public SimpleInventory Inventory => _inventory;
        public float HealthPercent => _maxHealthTime > 0f ? _currentHealthTime / _maxHealthTime : 0f;
        public ContainerSize Size => _size;
        public string DisplayName => _activeTheme != null ? _activeTheme.DisplayName : "Контейнер";

        private void Awake()
        {
            _inventory = GetComponent<SimpleInventory>();
            _inventory?.Clear();

            if (_interactionUI != null) {
                PrepareInteractionUI();
            }
        }

        private void Start()
        {
            if (!_isInitialized) {
                Initialize(new List<ItemDataSO>(), _defaultTheme);
            }
        }

        private void Update()
        {
            if (!_isInitialized) {
                return;
            }

            _currentHealthTime -= Time.deltaTime;
            if (_currentHealthTime <= 0f) {
                DestroyContainer();
            }

            UpdateInteractionUIPosition();
        }

        public void Initialize(List<ItemDataSO> items, ContainerThemeSO theme)
        {
            _isInitialized = true;
            _inventory ??= GetComponent<SimpleInventory>();
            _inventory?.Clear();

            _activeTheme = theme != null ? theme : _defaultTheme;

            if (items != null && _inventory != null) {
                foreach (ItemDataSO item in items) {
                    if (item != null) {
                        _inventory.AddItem(item.ID);
                    }
                }

                CalculateSize(items);
            }
            else {
                _size = ContainerSize.Small;
            }

            GenerateID();
            _currentHealthTime = _maxHealthTime;
            UpdateVisuals();
        }

        public void Interact()
        {
            if (!_isInitialized || !_isPlayerInRange) {
                return;
            }

            if (_interactionPopup == null) {
                _interactionPopup = FindFirstObjectByType<UniversalPopup>(FindObjectsInactive.Include);
            }

            if (_interactionPopup != null) {
                _interactionPopup.ShowQuestion(
                    _interactionTitle,
                    $"{_interactionMessage}\n{DisplayName} [{_containerID}]",
                    () => OnContainerInteracted?.Invoke(this),
                    onCancel: null,
                    confirmLabel: "Открыть",
                    cancelLabel: "Нет");
                return;
            }

            OnContainerInteracted?.Invoke(this);
        }

        public void RemoveItem(string itemId, int amount = 1)
        {
            _inventory?.RemoveItem(itemId, amount);
            CheckIfEmpty();
        }

        public void CheckIfEmpty()
        {
            if (_inventory != null && _inventory.GetTotalCount() == 0) {
                DestroyContainer();
            }
        }

        public void DestroyContainer()
        {
            Destroy(gameObject);
        }

        private void GenerateID()
        {
            string prefix = _activeTheme != null ? _activeTheme.IDPrefix : "GEN";
            string sizeSuffix = _size.ToString().Substring(0, 1).ToUpper();
            string uniquePart = UnityEngine.Random.Range(10000, 99999).ToString();

            _containerID = $"{prefix}_{sizeSuffix}_{uniquePart}";
            gameObject.name = $"LootBox_{_containerID}";
        }

        private void CalculateSize(List<ItemDataSO> items)
        {
            float totalWeight = 0f;
            foreach (ItemDataSO item in items) {
                if (item != null) {
                    totalWeight += item.Weight;
                }
            }

            if (totalWeight < 10f) {
                _size = ContainerSize.Small;
            }
            else if (totalWeight < 50f) {
                _size = ContainerSize.Medium;
            }
            else {
                _size = ContainerSize.Large;
            }
        }

        private void UpdateVisuals()
        {
            if (_renderer != null && _activeTheme != null) {
                _renderer.sprite = _activeTheme.GetSprite(_size);
            }
        }

        private void PrepareInteractionUI()
        {
            Canvas canvas = _interactionUI.GetComponent<Canvas>();
            if (canvas == null) {
                Canvas parentCanvas = _interactionUI.GetComponentInParent<Canvas>();
                if (parentCanvas != null && parentCanvas.transform.IsChildOf(transform)) {
                    canvas = parentCanvas;
                }
            }

            if (canvas == null) {
                canvas = GetComponentInChildren<Canvas>(true);
            }
            _interactionRoot = canvas != null ? canvas.transform : _interactionUI.transform;
            _interactionRoot.SetParent(transform, worldPositionStays: false);
            _interactionUI.SetActive(false);

            if (canvas != null) {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = Camera.main;
                canvas.sortingOrder = 100;
                canvas.transform.localScale = Vector3.one * _interactionCanvasScale;
            }
            else {
                _interactionRoot.localScale = Vector3.one;
            }

            _interactionButton = _interactionUI.GetComponentInChildren<Button>(true);
            if (_interactionButton != null) {
                _interactionButton.onClick.RemoveListener(Interact);
                _interactionButton.onClick.AddListener(Interact);
            }

            UpdateInteractionUIPosition();
        }

        private void UpdateInteractionUIPosition()
        {
            if (_interactionUI == null) {
                return;
            }

            _interactionRoot ??= _interactionUI.transform;
            _interactionRoot.position = transform.position + _interactionOffset;
            _interactionRoot.rotation = Quaternion.identity;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && _interactionUI != null) {
                _isPlayerInRange = true;
                UpdateInteractionUIPosition();
                _interactionUI.SetActive(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") && _interactionUI != null) {
                _isPlayerInRange = false;
                _interactionUI.SetActive(false);
            }
        }
    }
}
