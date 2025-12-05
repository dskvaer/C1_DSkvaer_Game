using System.Collections.Generic;
using UnityEngine;
using Menu_Journal;
using System;

namespace Menu_Journal {
    // Размер контейнера
    public enum ContainerSize { Small, Medium, Large }
    public enum ContainerSource { Player, NPC, Dead, WorldGenerated }

    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SimpleInventory))]
    public class LootContainer : MonoBehaviour {
        [Header("Data Source")]
        [SerializeField] private LootSettings _lootSettings;

        [Header("Container Settings")]
        [SerializeField] private ContainerSize _size = ContainerSize.Small;
        [SerializeField] private float _maxHealthTime = 300f;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Sprite _smallSprite;
        [SerializeField] private Sprite _mediumSprite;
        [SerializeField] private Sprite _largeSprite;

        // Внутренний инвентарь
        private SimpleInventory _inventory;
        private string _containerID;
        private float _currentHealthTime;
        private bool _isInitialized = false;

        // Событие, на которое подпишется UI Manager или PlayerController
        // Передает ссылку на ЭТОТ контейнер, чтобы UI знал, что открывать
        public static event Action<LootContainer> OnContainerInteracted;

        public string ContainerID => _containerID;
        public SimpleInventory Inventory => _inventory;
        public float HealthPercent => _currentHealthTime / _maxHealthTime;
        public ContainerSize Size => _size; // Нужно для UI, чтобы понимать вместимость

        private void Awake()
        {
            _inventory = GetComponent<SimpleInventory>();
        }

        private void Start()
        {
            if (!_isInitialized && _lootSettings != null)
            {
                InitializeFromSettings(_lootSettings);
            }
        }

        private void Update()
        {
            if (!_isInitialized) return;

            // Если открыто окно лутания именно этого ящика, таймер можно ставить на паузу (опционально)
            _currentHealthTime -= Time.deltaTime;

            if (_currentHealthTime <= 0)
            {
                // Тут нужно добавить проверку: если игрок сейчас смотрит в этот ящик,
                // нужно принудительно закрыть UI перед уничтожением.
                DestroyContainer();
            }
        }

        // --- ГЕНЕРАЦИЯ (Остается без изменений) ---
        public void InitializeFromSettings(LootSettings settings)
        {
            List<ItemDataSO> generatedItems = new List<ItemDataSO>();
            int itemsCount = UnityEngine.Random.Range(settings.minItems, settings.maxItems + 1);

            for (int i = 0; i < itemsCount; i++)
            {
                foreach (var loot in settings.possibleLoot)
                {
                    if (UnityEngine.Random.Range(0f, 100f) <= loot.dropChance)
                    {
                        if (loot.itemData != null)
                        {
                            generatedItems.Add(loot.itemData);
                            _inventory.AddItem(loot.itemData.ID);
                        }
                        break;
                    }
                }
            }

            CalculateSize(generatedItems);
            GenerateID(ContainerSource.WorldGenerated);
            _currentHealthTime = _maxHealthTime;
            UpdateVisuals();
            _isInitialized = true;
        }

        public void Initialize(ContainerSource source, List<ItemDataSO> items)
        {
            foreach (var item in items)
            {
                if (item != null) _inventory.AddItem(item.ID);
            }
            CalculateSize(items);
            GenerateID(source);
            _currentHealthTime = _maxHealthTime;
            if (source == ContainerSource.Dead) _currentHealthTime *= 0.3f;
            UpdateVisuals();
            _isInitialized = true;
        }

        // --- ЛОГИКА (Вспомогательная) ---
        private void GenerateID(ContainerSource source)
        {
            string prefix = source.ToString().Substring(0, 1).ToUpper();
            string sizeSuffix = _size.ToString().Substring(0, 1).ToUpper();
            string uniquePart = UnityEngine.Random.Range(1000, 9999).ToString();
            _containerID = $"{prefix}_DROP_{sizeSuffix}_{uniquePart}";
            gameObject.name = $"Loot_{_containerID}";
        }

        private void CalculateSize(List<ItemDataSO> items)
        {
            // Здесь нужна логика веса. Пока упрощенно.
            float totalWeight = items.Count * 1.0f;
            if (totalWeight < 10f) _size = ContainerSize.Small;
            else if (totalWeight < 50f) _size = ContainerSize.Medium;
            else _size = ContainerSize.Large;
        }

        private void UpdateVisuals()
        {
            if (_renderer == null) return;
            switch (_size)
            {
                case ContainerSize.Small: if (_smallSprite) _renderer.sprite = _smallSprite; break;
                case ContainerSize.Medium: if (_mediumSprite) _renderer.sprite = _mediumSprite; break;
                case ContainerSize.Large: if (_largeSprite) _renderer.sprite = _largeSprite; break;
            }
        }

        // --- НОВАЯ ЛОГИКА ВЗАИМОДЕЙСТВИЯ ---

        /// <summary>
        /// Вызывается, когда игрок нажал кнопку действия над контейнером.
        /// </summary>
        public void Interact()
        {
            if (!_isInitialized) return;

            Debug.Log($"[LootContainer] Игрок открыл контейнер {_containerID}");

            // Мы больше НЕ передаем сюда инвентарь игрока и НЕ перекладываем вещи.
            // Мы просто кричим: "Меня открыли! Вот я!"
            // UI Manager подпишется на это событие и откроет окно.
            OnContainerInteracted?.Invoke(this);
        }

        /// <summary>
        /// Вызывается ИЗ UI, когда предмет забирают.
        /// </summary>
        public void RemoveItem(string itemId, int amount = 1)
        {
            // Логика удаления из SimpleInventory
            // Так как SimpleInventory это список строк, удаляем первые найденные вхождения
            for (int i = 0; i < amount; i++)
            {
                if (_inventory.items.Contains(itemId))
                {
                    _inventory.items.Remove(itemId);
                }
            }

            CheckIfEmpty();
        }

        public void CheckIfEmpty()
        {
            if (_inventory.items.Count == 0)
            {
                Debug.Log($"[LootContainer] Контейнер {_containerID} пуст. Уничтожаем.");
                // Тут можно добавить анимацию закрытия/исчезновения перед удалением
                DestroyContainer();
            }
        }

        public void DestroyContainer()
        {
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // Тут можно включить подсветку или иконку кнопки "E" над ящиком
                // Например: InteractionUI.ShowButton(transform.position);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // Скрыть кнопку "E"
                // Если окно лута открыто - принудительно закрыть его (реализуется в UI Manager)
            }
        }
    }
}