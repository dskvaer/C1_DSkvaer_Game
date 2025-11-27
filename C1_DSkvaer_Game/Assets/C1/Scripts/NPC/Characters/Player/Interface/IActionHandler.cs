using NPC.Characters.Player;

public interface IActionHandler {
    void Initialize(IInputProvider input, IArmStateManager state, IAnimationController animation, IEnergy energy);
    void Tick();
}