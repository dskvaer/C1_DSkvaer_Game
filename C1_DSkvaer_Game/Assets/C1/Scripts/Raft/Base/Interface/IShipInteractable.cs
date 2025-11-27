using UnityEngine;

namespace Ship {
    /// <summary>
    /// Интерфейс для интерактивных действий с кораблём (сбор груза, порты, респавн).
    /// </summary>
    /// <remarks>
    /// Реализация:
    /// - Должен быть реализован на объекте корабля (обычно Player_Ship).
    /// - Используется для управления взаимодействиями (например, через ShipPlayerInputHandler).
    /// Настройка сцены:
    /// - Убедитесь, что объект корабля имеет компонент с IShipInteractable.
    /// - Порты и грузы должны иметь уникальные идентификаторы (itemId, portId).
    /// Логика работы:
    /// - PickupShipCargo: Добавляет груз на корабль (например, ресурсы).
    /// - EnterShipPort/LeaveShipPort: Управляет входом и выходом из порта.
    /// - RespawnAtNearestShipPort: Возвращает корабль в ближайший порт после уничтожения.
    /// </remarks>
    public interface IShipInteractable {
        /// <summary>
        /// Подбирает груз на корабль.
        /// </summary>
        /// <param name="itemId">Идентификатор предмета.</param>
        /// <param name="amount">Количество предметов.</param>
        void PickupShipCargo(string itemId, int amount);

        /// <summary>
        /// Вход в порт.
        /// </summary>
        /// <param name="portId">Идентификатор порта.</param>
        void EnterShipPort(string portId);

        /// <summary>
        /// Выход из порта.
        /// </summary>
        void LeaveShipPort();

        /// <summary>
        /// Респавн корабля в ближайшем порту после смерти.
        /// </summary>
        /// <param name="deathPosition">Позиция, где корабль был уничтожен.</param>
        void RespawnAtNearestShipPort(Vector2 deathPosition);
    }
}