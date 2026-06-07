using UnityEngine;

namespace C1.Player {
    public static class PlayerSystemRuntimeBootstrap {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreatePlayerSystem()
        {
            PlayerSystem.GetOrCreate();
        }
    }
}
