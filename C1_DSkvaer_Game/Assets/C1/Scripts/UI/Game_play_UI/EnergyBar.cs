using UnityEngine;
using UnityEngine.UI;
using NPC.Characters.Player;
using EnergyNS = NPC.Characters.Player.Energy; // Алиас для нового namespace

/// <summary>
/// UI компонент для отображения полоски энергии
/// Следует DIP - зависит от IEnergy абстракции
/// </summary>
public class EnergyBar : MonoBehaviour {
    [Header("UI References")]
    [SerializeField] private Slider energySlider;

    [Header("Energy Manager (optional - auto-find if null)")]
    [SerializeField] private MonoBehaviour energyManagerComponent;

    [Header("Visual Settings")]
    [SerializeField] private bool animateChanges = true;
    [SerializeField] private float animationSpeed = 5f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    private IEnergy energyManager; // Старый интерфейс
    private float targetValue;
    private float currentValue;

    private void Awake()
    {
        if (!ValidateComponents())
        {
            enabled = false;
            return;
        }

        InitializeEnergyManager();
        InitializeSlider();
        SubscribeToEvents();
    }

    private bool ValidateComponents()
    {
        if (energySlider == null)
        {
            Debug.LogError("EnergyBar: Slider not assigned!", this);
            return false;
        }
        return true;
    }

    private void InitializeEnergyManager()
    {
        // Пытаемся получить IEnergy из назначенного компонента
        if (energyManagerComponent != null)
        {
            energyManager = energyManagerComponent as IEnergy;
            if (energyManager == null)
            {
                Debug.LogError("EnergyBar: Assigned component does not implement IEnergy!", this);
            }
        }

        // Если не назначен - ищем в сцене
        if (energyManager == null)
        {
            var manager = FindFirstObjectByType<EnergyNS.EnergyManager>();
            if (manager != null)
            {
                energyManager = manager;
                energyManagerComponent = manager;
                if (enableDebugLogs)
                    Debug.Log("EnergyBar: Found EnergyManager in scene", this);
            }
        }

        if (energyManager == null)
        {
            Debug.LogError("EnergyBar: EnergyManager not found in scene!", this);
            enabled = false;
        }
    }

    private void InitializeSlider()
    {
        if (energyManager == null) return;

        energySlider.minValue = 0f;
        energySlider.maxValue = 1f;

        float initialPercent = (float)energyManager.CurrentEnergy / energyManager.MaxEnergy;
        energySlider.value = initialPercent;
        currentValue = initialPercent;
        targetValue = initialPercent;

        if (enableDebugLogs)
            Debug.Log($"EnergyBar: Initialized with {energyManager.CurrentEnergy}/{energyManager.MaxEnergy} ({initialPercent * 100f}%)", this);
    }

    private void SubscribeToEvents()
    {
        if (energyManager == null) return;

        energyManager.OnEnergyChanged.AddListener(UpdateBar);
        energyManager.OnEnergyEmpty.AddListener(OnEmpty);

        if (enableDebugLogs)
            Debug.Log("EnergyBar: Subscribed to energy events", this);
    }

    private void Update()
    {
        if (animateChanges && Mathf.Abs(currentValue - targetValue) > 0.001f)
        {
            currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * animationSpeed);
            energySlider.value = currentValue;
        }
    }

    private void UpdateBar()
    {
        if (energyManager == null) return;

        targetValue = (float)energyManager.CurrentEnergy / energyManager.MaxEnergy;

        if (!animateChanges)
        {
            currentValue = targetValue;
            energySlider.value = targetValue;
        }

        if (enableDebugLogs)
            Debug.Log($"EnergyBar: Updated to {energyManager.CurrentEnergy}/{energyManager.MaxEnergy} ({targetValue * 100f}%)", this);
    }

    private void OnEmpty()
    {
        targetValue = 0f;

        if (!animateChanges)
        {
            currentValue = 0f;
            energySlider.value = 0f;
        }

        if (enableDebugLogs)
            Debug.Log("EnergyBar: Energy empty, bar set to 0%", this);
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void UnsubscribeFromEvents()
    {
        if (energyManager != null)
        {
            energyManager.OnEnergyChanged.RemoveListener(UpdateBar);
            energyManager.OnEnergyEmpty.RemoveListener(OnEmpty);

            if (enableDebugLogs)
                Debug.Log("EnergyBar: Unsubscribed from energy events", this);
        }
    }

    // ===== PUBLIC API для тестирования =====

    /// <summary>
    /// Принудительное обновление UI (для тестирования)
    /// </summary>
    public void ForceUpdate()
    {
        UpdateBar();
    }

    /// <summary>
    /// Установка EnergyManager программно
    /// </summary>
    public void SetEnergyManager(IEnergy manager)
    {
        if (manager == null)
        {
            Debug.LogError("EnergyBar: Cannot set null energy manager!", this);
            return;
        }

        // Отписываемся от старого
        UnsubscribeFromEvents();

        // Устанавливаем новый
        energyManager = manager;
        energyManagerComponent = manager as MonoBehaviour;

        // Подписываемся и инициализируем
        InitializeSlider();
        SubscribeToEvents();

        if (enableDebugLogs)
            Debug.Log("EnergyBar: Energy manager set programmatically", this);
    }
}
