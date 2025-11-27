using UnityEngine;

namespace NPC.Characters.Player {
    [RequireComponent(typeof(ArmStateManager))]
    public class PlayerStateManager : MonoBehaviour, IArmStateManager {
        [SerializeField] private ArmStateManager armStateManager;
        private bool isArmed;
        private bool isTransitioning;

        public bool IsArmed => isArmed;
        public bool IsTransitioning => isTransitioning;

        private void Awake()
        {
            if (armStateManager == null)
            {
                armStateManager = GetComponent<ArmStateManager>();
                if (armStateManager == null)
                {
                    Debug.LogError("PlayerStateManager: ArmStateManager не привязан и не найден на объекте!", this);
                    enabled = false;
                    return;
                }
            }

            Debug.Log("PlayerStateManager: Инициализирован", this);
        }

        public void TriggerArmTransition(bool toArmed)
        {
            if (isTransitioning)
            {
                Debug.LogWarning($"PlayerStateManager: Переход уже выполняется, игнорируем новое действие Arm. Current isArmed={isArmed}, isTransitioning={isTransitioning}", this);
                return;
            }

            isTransitioning = true;
            isArmed = toArmed;
            Debug.Log($"PlayerStateManager: Начало перехода, toArmed={toArmed}, isTransitioning={isTransitioning}", this);

            if (armStateManager != null)
            {
                armStateManager.TriggerArmTransition(toArmed);
            }
            else
            {
                Debug.LogError("PlayerStateManager: ArmStateManager не найден, завершаем переход!", this);
                OnTransitionComplete(toArmed);
            }
        }

        public void OnTransitionComplete(bool isArmed)
        {
            this.isArmed = isArmed;
            isTransitioning = false;
            Debug.Log($"PlayerStateManager: Переход завершён, isArmed={isArmed}, isTransitioning={isTransitioning}", this);
        }
    }
}