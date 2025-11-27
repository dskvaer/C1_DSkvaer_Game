using UnityEngine;

namespace Ship {
    /// <summary>
    /// Интерфейс для управления здоровьем корабля.
    /// </summary>
    /// <remarks>
    /// Реализация:
    /// - Должен быть реализован на объекте корабля (Player_Ship, Enemy_Ship, Trader_Ship).
    /// - Предоставляет доступ к GameObject для логирования и эффектов.
    /// Настройка сцены:
    /// - Убедитесь, что компонент с IShipHealth прикреплён к объекту с ShipID.
    /// - Используется в ShipHealth, NPCAI, EnemySpawner, RamAttackTactic, Projectile.
    /// Логика работы:
    /// - TakeShipDamage: Наносит урон (с учётом типа атаки, например, таран).
    /// - SetShipHealth: Устанавливает текущее здоровье.
    /// - GetCurrentShipHealth/GetMaxShipHealth: Возвращают текущее и максимальное здоровье.
    /// - IsDead: Проверяет, уничтожен ли корабль (здоровье <= 0).
    /// - GameObject: Предоставляет доступ к объекту для эффектов и логирования.
    /// </remarks>
    public interface IShipHealth {
        /// <summary>
        /// Наносит урон кораблю.
        /// </summary>
        /// <param name="amount">Количество урона (положительное значение).</param>
        void TakeShipDamage(int amount);

        /// <summary>
        /// Наносит урон кораблю с учётом типа атаки (например, таран).
        /// </summary>
        /// <param name="amount">Количество урона.</param>
        /// <param name="isRam">Является ли урон тараном (для расчёта собственного урона).</param>
        void TakeShipDamage(int amount, bool isRam);

        /// <summary>
        /// Устанавливает текущее здоровье корабля.
        /// </summary>
        /// <param name="value">Новое значение здоровья (0 или больше, не превышает максимум).</param>
        void SetShipHealth(int value);

        /// <summary>
        /// Возвращает текущее здоровье корабля.
        /// </summary>
        /// <returns>Текущее здоровье (0 или больше).</returns>
        int GetCurrentShipHealth();

        /// <summary>
        /// Возвращает максимальное здоровье корабля.
        /// </summary>
        /// <returns>Максимальное здоровье.</returns>
        int GetMaxShipHealth();

        /// <summary>
        /// Проверяет, уничтожен ли корабль (здоровье <= 0).
        /// </summary>
        /// <returns>True, если здоровье <= 0, иначе false.</returns>
        bool IsDead { get; }

        /// <summary>
        /// Возвращает GameObject корабля для логирования и эффектов.
        /// </summary>
        GameObject GameObject { get; }
    }
}