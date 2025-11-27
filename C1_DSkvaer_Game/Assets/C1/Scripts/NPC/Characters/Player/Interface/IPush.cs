using Spine.Unity;

namespace NPC.Characters.Player {
    public interface IPush {
        void Initialize(SkeletonAnimation skeletonAnimation);
        bool IsPushing { get; }
        bool IsInteractHeld { get; }
        bool IsInPushContact { get; }
        void UpdatePush();
        void EnableInput();
        void DisableInput();
    }
}