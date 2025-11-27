namespace NPC.Characters.Player {
    public interface IJumpAnimation {
        void Play();
        void PlayJumpStart();
        void PlayJumpAir();
        void PlayJumpLand();
        bool IsJumpAnimationActive { get; }
    }
}