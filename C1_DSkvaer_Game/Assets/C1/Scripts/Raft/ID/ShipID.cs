using UnityEngine;

/// <summary>
/// Компонент для хранения и инициализации ID корабля.
/// </summary>
public class ShipID : MonoBehaviour {

    [Header("Идентификация (Identity)")]

    [SerializeField]
    [InspectorLabel("Конфигурация ID")]
    [Tooltip("Конфигурация, используемая для генерации или получения уникального ID.")]
    private ShipIDConfig config;

    /// <summary>
    /// Конфигурация, используемая для получения ID.
    /// </summary>
    public ShipIDConfig Config => config;


    [SerializeField]
    [InspectorLabel("Уникальный ID")]
    [Tooltip("Текущий строковый идентификатор корабля.")]
    private string id;

    /// <summary>
    /// Уникальный идентификатор корабля.
    /// </summary>
    public string ID
    {
        get => id;
        set
        {
            id = value;
        }
    }

    /// <summary>
    /// Инициализация ID корабля на основе конфигурации.
    /// </summary>
    private void Awake()
    {
        if (config != null)
        {
            ID = config.GetID();
        }
    }
}