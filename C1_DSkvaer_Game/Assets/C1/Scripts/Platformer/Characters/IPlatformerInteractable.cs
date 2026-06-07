using UnityEngine;

namespace C1.Platformer.Characters {
    public interface IPlatformerInteractable {
        bool CanInteract(GameObject interactor);
        void Interact(GameObject interactor);
    }
}
