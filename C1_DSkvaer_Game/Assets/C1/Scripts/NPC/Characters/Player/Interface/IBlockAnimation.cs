using Spine.Unity;

namespace NPC.Characters.Player {
    public interface IBlockAnimation {
        bool IsBlockAnimationActive { get; }
        void Initialize(SkeletonAnimation skeletonAnimation);
        void PlayBlock(bool loop);
        void StopBlock();
    }
}