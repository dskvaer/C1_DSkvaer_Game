using Ship;
using UnityEngine;

namespace Ship {
    /// <summary>
    /// Интерфейс для тактик ИИ врага. Определяет методы для проверки и выполнения тактики.
    /// </summary>
    /// <remarks>
    /// Реализация:
    /// - Используется в компонентах тактик (RamAttackTactic, PatrolTactic, FleeTactic).
    /// - Привязывается к NPCAI для управления поведением NPC.
    /// Настройка сцены:
    /// - Убедитесь, что тактики (RamAttackTactic, PatrolTactic, FleeTactic) находятся в namespace Ship.
    /// - Компоненты тактик должны быть прикреплены к объекту Enemy_Ship.
    /// Логика работы:
    /// - CanExecute: Проверяет возможность выполнения тактики на основе контекста.
    /// - Execute: Выполняет тактику, обновляя состояние NPC (движение, атака и т.д.).
    /// </remarks>
    public interface IEnemyTactic {
        /// <summary>
        /// Проверяет, может ли тактика быть выполнена в текущем состоянии.
        /// </summary>
        /// <param name="context">Контекст ИИ, содержащий данные о корабле, игроке и тайлмапе.</param>
        /// <returns>True, если тактика может быть выполнена, иначе false.</returns>
        bool CanExecute(EnemyAIContext context);

        /// <summary>
        /// Выполняет тактику, обновляя состояние врага (например, движение или атака).
        /// </summary>
        /// <param name="context">Контекст ИИ, содержащий данные о корабле, игроке и тайлмапе.</param>
        /// <param name="deltaTime">Время, прошедшее с последнего кадра.</param>
        void Execute(EnemyAIContext context, float deltaTime);
    }
}