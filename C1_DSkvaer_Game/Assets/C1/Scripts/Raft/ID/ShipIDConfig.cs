using UnityEngine;

/// <summary>
/// Настройки генерации ID корабля.
/// </summary>
[CreateAssetMenu(fileName = "ShipIDConfig", menuName = "ShipConfigs/Ship ID Config", order = 1)]
public class ShipIDConfig : ScriptableObject {
    [Header("ID корабля")]
    [InspectorLabel("Префикс ID")]
    [Tooltip("Короткий код типа корабля. Например RP для игрока, RE для врага, RT для торговца.")]
    [SerializeField] private string idPrefix = "RP";
    public string IdPrefix => idPrefix;

    [InspectorLabel("Фиксированный номер")]
    [Tooltip("Номер ID, если случайная генерация выключена.")]
    [SerializeField] private int idNumber = 0;
    public int IdNumber => idNumber;

    [InspectorLabel("Случайный номер")]
    [Tooltip("Если включено, номер ID будет случайным в диапазоне 1000-9999.")]
    [SerializeField] private bool randomizeNumber = false;
    public bool RandomizeNumber => randomizeNumber;

    /// <summary>
    /// Возвращает ID в формате {IdPrefix}_{IdNumber} или {IdPrefix}_{RandomNumber}.
    /// </summary>
    public string GetID()
    {
        if (randomizeNumber)
        {
            int randomNum = Random.Range(1000, 9999);
            return $"{idPrefix}_{randomNum}";
        }

        return $"{idPrefix}_{idNumber}";
    }
}
