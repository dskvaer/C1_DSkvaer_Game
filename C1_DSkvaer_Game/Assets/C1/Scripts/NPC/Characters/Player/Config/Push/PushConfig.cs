using UnityEngine;

[CreateAssetMenu(fileName = "PushConfig", menuName = "Configs/PushConfig", order = 1)]
public class PushConfig : ScriptableObject {
    [SerializeField, Range(0, 31)] private int playerLayer = 3; // По умолчанию слой Player
    [SerializeField, Range(0, 31)] private int pushableLayer = 7; // По умолчанию слой Pushable
    [SerializeField, Range(0, 31)] private int obstacleLayer = 8; // Слой препятствий/стен
    [SerializeField, Range(0.1f, 5f)] private float pushCheckDistance = 1.5f;
    [SerializeField, Range(0f, 10f)] private float pushForceLerpSpeed = 5f;
    [SerializeField, Range(0f, 500f)] private float maxPushForce = 100f;
    [SerializeField] private bool shouldLog = true;

    // Cycle impulse settings (перенёс сюда)
    [Header("Cycle impulse settings")]
    [SerializeField] private bool useCycleImpulse = true;       // true => импульсы по завершению цикла анимации
    [SerializeField, Range(0.0f, 1f)] private float cycleDecay = 0.85f; // множитель затухания силы после каждого импульса
    [SerializeField, Range(0f, 50f)] private float minForceThreshold = 0.05f; // порог ниже которого импульс не применяется

    public int PlayerLayer => playerLayer;
    public int PushableLayer => pushableLayer;
    public int ObstacleLayer => obstacleLayer;
    public float PushCheckDistance => pushCheckDistance;
    public float PushForceLerpSpeed => pushForceLerpSpeed;
    public float MaxPushForce => maxPushForce;
    public bool ShouldLog => shouldLog;

    public bool UseCycleImpulse => useCycleImpulse;
    public float CycleDecay => cycleDecay;
    public float MinForceThreshold => minForceThreshold;

    private void OnValidate()
    {
        if (playerLayer < 0 || playerLayer > 31)
        {
            Debug.LogError($"PushConfig: PlayerLayer ({playerLayer}) is invalid! Must be between 0 and 31.", this);
            playerLayer = 3;
        }
        if (pushableLayer < 0 || pushableLayer > 31)
        {
            Debug.LogError($"PushConfig: PushableLayer ({pushableLayer}) is invalid! Must be between 0 and 31.", this);
            pushableLayer = 7;
        }
        if (obstacleLayer < 0 || obstacleLayer > 31)
        {
            Debug.LogWarning($"PushConfig: ObstacleLayer ({obstacleLayer}) is invalid! Resetting to 8.", this);
            obstacleLayer = 8;
        }
        if (pushCheckDistance < 0.1f)
        {
            Debug.LogWarning($"PushConfig: PushCheckDistance ({pushCheckDistance}) is too small! Setting to 0.1.", this);
            pushCheckDistance = 0.1f;
        }
        if (cycleDecay < 0f || cycleDecay > 1f)
        {
            cycleDecay = Mathf.Clamp01(cycleDecay);
        }
        if (minForceThreshold < 0f)
        {
            minForceThreshold = 0f;
        }
    }
}
