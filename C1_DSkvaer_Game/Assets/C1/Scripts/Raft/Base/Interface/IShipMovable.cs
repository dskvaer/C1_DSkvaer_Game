using UnityEngine;

namespace Ship {
    /// <summary>
    /// Интерфейс для управления движением корабля (перемещение и поворот).
    /// </summary>
    /// <remarks>
    /// Реализация:
    /// - Должен быть реализован на объекте корабля (Player_Ship, Enemy_Ship, Trader_Ship).
    /// - Используется для управления движением через ShipMovement или аналогичный компонент.
    /// Настройка сцены:
    /// - Убедитесь, что объект корабля имеет Rigidbody2D (Kinematic или Dynamic).
    /// - Привязать к компонентам, использующим движение (например, NPCAI, ShipPlayerInputHandler).
    /// Логика работы:
    /// - ShipMove: Перемещает корабль в заданном направлении с учётом силы ввода.
    /// - ShipRotate: Поворачивает корабль в заданном направлении с учётом множителя скорости.
    /// - GetShipVelocity: Возвращает текущую скорость корабля для расчётов (например, для тарана).
    /// </remarks>
    public interface IShipMovable {
        /// <summary>
        /// Перемещает корабль в заданном направлении с указанной силой.
        /// </summary>
        /// <param name="inputDirection">Направление ввода (X для поворота, Y для движения вперёд/назад).</param>
        /// <param name="inputStrength">Сила ввода (0-1, влияет на скорость).</param>
        void ShipMove(Vector2 inputDirection, float inputStrength);

        /// <summary>
        /// Поворачивает корабль в заданном направлении.
        /// </summary>
        /// <param name="inputDirection">Направление поворота (X для угла, Y не используется).</param>
        /// <param name="speedFactor">Множитель скорости поворота (0-1).</param>
        void ShipRotate(Vector2 inputDirection, float speedFactor);

        /// <summary>
        /// Возвращает текущую скорость корабля в мировых координатах.
        /// </summary>
        /// <returns>Вектор скорости (Vector2).</returns>
        Vector2 GetShipVelocity();
    }
}