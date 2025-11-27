using UnityEngine;

/// <summary>
/// Компонент для хранения и управления ID корабля.
/// </summary>
public class ShipID : MonoBehaviour {
    [Header("Конфигурация ID")]
    [SerializeField] private ShipIDConfig config; // Конфигурация для генерации ID
    /// <summary>
    /// Конфигурация, определяющая префикс и номер ID.
    /// </summary>
    public ShipIDConfig Config => config;

    [SerializeField] private string id; // Итоговый ID
    /// <summary>
    /// Уникальный идентификатор корабля.
    /// </summary>
    public string ID
    {
        get => id;
        set
        {
            id = value;
            Debug.Log($"ShipID: Set ID={id} for {gameObject.name}");
        }
    }

    /// <summary>
    /// Инициализирует ID корабля на основе конфигурации.
    /// </summary>
    private void Awake()
    {
        if (config != null)
        {
            ID = config.GetID();
        }
        else if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning($"ShipID: ID не установлен и конфиг не задан для {gameObject.name}!");
        }
    }
}