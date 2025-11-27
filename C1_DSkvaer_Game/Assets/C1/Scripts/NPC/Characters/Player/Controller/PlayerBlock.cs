using UnityEngine;
using NPC.Characters.Player;
using EnergyNS = NPC.Characters.Player.Energy; // Алиас для нового namespace

public class PlayerBlock : MonoBehaviour, IActionHandler, IBlock {
    [Header("Configuration")]
    [SerializeField] private EnergyNS.ActionCostSO actionCost;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    private IInputProvider inputProvider;
    private IArmStateManager stateManager;
    private IAnimationController animationController;
    private IEnergy energyManager; // Старый интерфейс из NPC.Characters.Player

    private bool isBlocking;

    public bool IsBlocking => isBlocking;

    private void Awake()
    {
        if (actionCost == null)
        {
            Debug.LogError("PlayerBlock: ActionCostSO not assigned!", this);
            enabled = false;
        }
    }

    public void Initialize()
    {
        if (enableDebugLogs)
            Debug.Log("PlayerBlock: IBlock.Initialize called", this);
    }

    public void Initialize(IInputProvider input, IArmStateManager state, IAnimationController animation, IEnergy energy)
    {
        inputProvider = input;
        stateManager = state;
        animationController = animation;
        energyManager = energy;

        if (enableDebugLogs)
            Debug.Log("PlayerBlock: Initialized with IActionHandler", this);
    }

    public void Tick()
    {
        if (!enabled || energyManager == null || inputProvider == null || stateManager == null)
            return;

        if (inputProvider.IsBlockPressed() && stateManager.IsArmed && !stateManager.IsTransitioning)
        {
            Block();
        }
        else if (inputProvider.IsBlockHeld() && isBlocking)
        {
            energyManager.ConsumeForHeldAction(actionCost.BlockCostPerSecond, Time.deltaTime);
        }
        else if (!inputProvider.IsBlockHeld() && isBlocking)
        {
            CancelBlock();
        }
    }

    public void Block()
    {
        if (isBlocking || !stateManager.IsArmed || stateManager.IsTransitioning)
            return;

        if (energyManager.IsEnergyEmpty)
        {
            if (enableDebugLogs)
                Debug.Log("PlayerBlock: Cannot block - no energy!", this);
            return;
        }

        isBlocking = true;

        if (TryGetComponent(out IBlockAnimation blockAnimation))
        {
            blockAnimation.PlayBlock(true);
        }

        // Уведомляем EnergyManager
        if (TryGetComponent(out EnergyNS.EnergyManager energyMgr))
        {
            energyMgr.OnBlockStarted();
        }

        if (enableDebugLogs)
            Debug.Log("PlayerBlock: Block started", this);
    }

    public void CancelBlock()
    {
        if (!isBlocking)
            return;

        isBlocking = false;

        if (TryGetComponent(out IBlockAnimation blockAnimation))
        {
            blockAnimation.StopBlock();
        }

        if (TryGetComponent(out EnergyNS.EnergyManager energyMgr))
        {
            energyMgr.OnBlockCanceled();
        }

        if (enableDebugLogs)
            Debug.Log("PlayerBlock: Block canceled", this);
    }

    private void OnDisable()
    {
        if (isBlocking)
        {
            CancelBlock();
        }
    }
}
