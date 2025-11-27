using UnityEngine;

namespace Ship {
    /// <summary>
    /// Интерфейс для объектов, которые могут получать урон и отбрасывание.
    /// </summary>
    /// <remarks>
    /// Реализация:
    /// - Должен быть реализован на объекте корабля (Player_Ship, Enemy_Ship, Trader_Ship).
    /// - Используется для обработки столкновений и таранов (например, в RamAttackTactic).
    /// Настройка сцены:
    /// - Убедитесь, что объект корабля имеет Rigidbody2D для обработки отбрасывания.
    /// - Используется в ShipHealth или аналогичном компоненте.
    /// Логика работы:
    /// - ApplyShipKnockback: Применяет силу отбрасывания при столкновении.
    /// - TakeShipDamage: Дублирует метод из IShipHealth для совместимости.
    /// </remarks>
    public interface IShipDamageable {
        /// <summary>
        /// Применяет отбрасывание к кораблю (например, при столкновении).
        /// </summary>
        /// <param name="force">Вектор силы отбрасывания.</param>
        void ApplyShipKnockback(Vector2 force);

        /// <summary>
        /// Наносит урон кораблю (дублирует TakeShipDamage из IShipHealth для совместимости).
        /// </summary>
        /// <param name="amount">Количество урона.</param>
        void TakeShipDamage(int amount);
    }
}