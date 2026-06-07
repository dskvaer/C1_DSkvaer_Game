using UnityEngine;

#pragma warning disable 0414

namespace C1.Platformer.Characters {
    public sealed class PlatformerCharacterRigNotes : MonoBehaviour {
        [Header("Памятка по настройке персонажа")]
        [SerializeField, TextArea(10, 18)] private string notes =
            "1. Rigidbody2D: Dynamic, Freeze Rotation Z, Continuous Collision.\n" +
            "2. Body Collider: основной коллайдер тела. Он используется для земли, приседа и краёв платформ.\n" +
            "3. Movement Profile: хранить в Assets/C1/Scripts/Platformer/Characters/Settings.\n" +
            "4. Android: для экранных кнопок используйте PlatformerTouchInputSource и привяжите его методы к UI.\n" +
            "5. Spine 4.3: события ControlLock/ControlUnlock блокируют и возвращают управление; MotorStep даёт анимационный шаг; LedgeClimbComplete завершает залезание.\n" +
            "6. Оружие: каждый предмет/оружие лучше делать отдельным prefab и вставлять в PlatformerWeaponController.\n" +
            "7. RPG: PlatformerCharacterProgression хранит уровень, опыт, доступные очки и открытые навыки.";
    }
}
