using UnityEngine;

/// <summary>
/// Настройки для генерации ID корабля. Определяет префикс и номер ID, а также режим рандомизации.
/// </summary>
[CreateAssetMenu(fileName = "ShipIDConfig", menuName = "ShipConfigs/Ship ID Config", order = 1)]
public class ShipIDConfig : ScriptableObject {
    [Header("Базовая информация о корабле")]
    [SerializeField] private string idPrefix = "RP"; // Префикс: RP, RE, RR, RT, и т.д.
    /// <summary>
    /// Префикс ID корабля (например, RP для игрока, RE для врага, RT для торговца).
    /// </summary>
    public string IdPrefix => idPrefix;

    [SerializeField] private int idNumber = 0; // Число или версия
    /// <summary>
    /// Номер ID, если не используется рандомизация.
    /// </summary>
    public int IdNumber => idNumber;

    [SerializeField] private bool randomizeNumber = false; // Случайный номер при старте
    /// <summary>
    /// Если true, генерирует случайный номер ID (1000–9999). Иначе использует IdNumber.
    /// </summary>
    public bool RandomizeNumber => randomizeNumber;

    /// <summary>
    /// Генерирует ID корабля в формате {IdPrefix}_{IdNumber} или {IdPrefix}_{RandomNumber}.
    /// </summary>
    /// <returns>Сгенерированный ID.</returns>
    public string GetID()
    {
        if (randomizeNumber)
        {
            int randomNum = Random.Range(1000, 9999);
            return $"{idPrefix}_{randomNum}";
        }
        else
        {
            return $"{idPrefix}_{idNumber}";
        }
    }
}