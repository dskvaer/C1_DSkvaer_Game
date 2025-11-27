using System;

public enum JumpPhase {
    None,      // Нет прыжка
    JumpStart, // Начало прыжка (анимация JumpStart)
    JumpAir,   // В воздухе (анимация JumpMid)
    JumpLand   // Приземление (анимация JumpEnd)
}