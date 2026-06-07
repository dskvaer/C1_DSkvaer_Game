using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace NPC.Characters.Player.Energy {
    [DisallowMultipleComponent]
    public class EnergyManager : MonoBehaviour, IEnergy, Player.IEnergy {
        [Header("Настройки энергии")]
        [InspectorLabel("Конфиг энергии")]
        [Tooltip("ScriptableObject с максимальной энергией, регенерацией и порогом низкой энергии.")]
        [SerializeField] private EnergyConfigSO config;

        [InspectorLabel("Стоимость действий")]
        [Tooltip("ScriptableObject с расходом энергии на блок, движение, толкание, атаку и прыжок.")]
        [SerializeField] private ActionCostSO actionCost;

        [Header("Отладка")]
        [InspectorLabel("Включить логи")]
        [Tooltip("Если включено, менеджер энергии пишет отладочные сообщения в консоль.")]
        [SerializeField] private bool enableDebugLogs = false;

        private EnergyCore energyCore;
        private EnergyRegenerator regenerator;
        private Dictionary<string, IEnergyConsumer> consumers = new Dictionary<string, IEnergyConsumer>();

        private bool isBlocking;
        private bool isMoving;
        private bool isPushing;

        public int CurrentEnergy => energyCore?.CurrentEnergy ?? 0;
        public int MaxEnergy => energyCore?.MaxEnergy ?? 100;
        public bool IsEnergyEmpty => energyCore?.IsEnergyEmpty ?? false;
        public bool IsLowEnergy => energyCore?.IsLowEnergy ?? false;

        public UnityEvent OnEnergyChanged => energyCore?.OnEnergyChanged;
        public UnityEvent OnEnergyEmpty => energyCore?.OnEnergyEmpty;
        public UnityEvent OnLowEnergy => energyCore?.OnLowEnergy;

        private void Awake()
        {
            if (!ValidateConfiguration())
            {
                enabled = false;
                return;
            }

            InitializeSystems();
            RegisterDefaultConsumers();
            SetupRegenerationConditions();
        }

        private bool ValidateConfiguration()
        {
            if (config == null)
            {
                Debug.LogError("EnergyManager: EnergyConfigSO не назначен!", this);
                return false;
            }

            if (actionCost == null)
            {
                Debug.LogError("EnergyManager: ActionCostSO не назначен!", this);
                return false;
            }

            return true;
        }

        private void InitializeSystems()
        {
            energyCore = new EnergyCore(config);
            regenerator = new EnergyRegenerator(config);

            if (enableDebugLogs)
                Debug.Log($"EnergyManager: системы инициализированы, MaxEnergy={MaxEnergy}", this);
        }

        private void RegisterDefaultConsumers()
        {
            RegisterConsumer(new SingleActionConsumer("Attack", actionCost.AttackSingleCost));
            RegisterConsumer(new SingleActionConsumer("Jump", actionCost.JumpSingleCost));
            RegisterConsumer(new SingleActionConsumer("BlockHit", actionCost.BlockHitCost));
            RegisterConsumer(new HeldActionConsumer("Block", actionCost.BlockCostPerSecond));
            RegisterConsumer(new HeldActionConsumer("Move", actionCost.MoveCostPerSecond));
            RegisterConsumer(new HeldActionConsumer("Push", actionCost.PushCostPerSecond));

            if (enableDebugLogs)
                Debug.Log($"EnergyManager: Registered {consumers.Count} default consumers", this);
        }

        private void SetupRegenerationConditions()
        {
            regenerator.AddCondition(new NoRegenWhileMovingCondition(() => isMoving));
            regenerator.AddCondition(new NoRegenWhileBlockingCondition(() => isBlocking));
        }

        private void Start()
        {
            energyCore.Initialize();
        }

        private void Update()
        {
            regenerator.Update(energyCore, energyCore, Time.deltaTime);
            ProcessHeldAction("Block", isBlocking);
            ProcessHeldAction("Move", isMoving);
            ProcessHeldAction("Push", isPushing);
        }

        private void ProcessHeldAction(string actionName, bool isActive)
        {
            if (!isActive) return;

            if (consumers.TryGetValue(actionName, out var consumer))
            {
                float cost = consumer.CalculateCost(Time.deltaTime);
                if (cost > 0f)
                {
                    energyCore.ConsumeEnergy(cost);
                    consumer.OnConsumed();

                    if (enableDebugLogs)
                        Debug.Log($"EnergyManager: {actionName} consumed {cost} energy", this);
                }
            }
        }

        public void ConsumeEnergy(float amount) => energyCore?.ConsumeEnergy(amount);
        public void RestoreEnergy(float amount) => energyCore?.RestoreEnergy(amount);
        public void ConsumeForSingleAction(float cost) => energyCore?.ConsumeForSingleAction(cost);
        public void ConsumeForHeldAction(float costPerSecond, float deltaTime) => energyCore?.ConsumeForHeldAction(costPerSecond, deltaTime);

        public void RegisterConsumer(IEnergyConsumer consumer)
        {
            if (consumer == null) return;
            consumers[consumer.ActionName] = consumer;
        }

        public void ConsumeEnergyForAction(string actionName)
        {
            if (!consumers.TryGetValue(actionName, out var consumer)) return;
            if (!consumer.CanConsume(energyCore)) return;

            float cost = consumer.CalculateCost(0f);
            if (cost > 0f)
            {
                energyCore.ConsumeEnergy(cost);
                consumer.OnConsumed();
            }
        }

        public void OnBlockStarted() => isBlocking = true;
        public void OnBlockCanceled() { isBlocking = false; ResetConsumer("Block"); }
        public void OnAttack() => ConsumeEnergyForAction("Attack");
        public void OnMoveStarted() => isMoving = true;
        public void OnMoveCanceled() { isMoving = false; ResetConsumer("Move"); }
        public void OnJump() => ConsumeEnergyForAction("Jump");
        public void OnPushStarted() => isPushing = true;
        public void OnPushCanceled() { isPushing = false; ResetConsumer("Push"); }

        public void OnHealthChanged()
        {
            if (isBlocking)
                ConsumeEnergyForAction("BlockHit");
        }

        private void ResetConsumer(string actionName)
        {
            if (consumers.TryGetValue(actionName, out var consumer))
                consumer.Reset();
        }
    }
}
