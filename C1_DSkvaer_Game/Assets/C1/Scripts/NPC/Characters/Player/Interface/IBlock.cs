namespace NPC.Characters.Player {
    public interface IBlock {
        bool IsBlocking { get; }
        void Initialize();
        void Block();
        void CancelBlock();
    }
}