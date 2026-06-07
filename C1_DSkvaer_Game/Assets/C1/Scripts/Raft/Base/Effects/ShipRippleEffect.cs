using Ship;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ShipRippleEffect : MonoBehaviour {
    [Header("Круги на воде")]
    [InspectorLabel("Конфиг кругов")]
    [Tooltip("ScriptableObject с настройками кругов воды вокруг корабля.")]
    [SerializeField] private ShipRippleConfig config;

    [InspectorLabel("Движение корабля")]
    [Tooltip("Компонент движения корабля, по скорости которого включаются круги на воде.")]
    [SerializeField] private ShipMovement shipMovement;

    [InspectorLabel("Материал кругов")]
    [Tooltip("Материал частиц кругов воды.")]
    [SerializeField] private Material rippleParticleMaterial;

    private ParticleSystem rippleParticles;
    private ParticleSystem.EmissionModule emissionModule;
    private float stopTimer;
    private bool isMoveInputActive;
    private const float StopDelay = 2f;

    private void Awake()
    {
        rippleParticles = GetComponent<ParticleSystem>();
        emissionModule = rippleParticles.emission;

        if (config == null)
        {
            Debug.LogError("ShipRippleConfig не назначен в инспекторе!", this);
            enabled = false;
            return;
        }
        if (shipMovement == null)
        {
            Debug.LogError("ShipMovement не назначен в инспекторе!", this);
            enabled = false;
            return;
        }
        if (rippleParticleMaterial == null)
        {
            Debug.LogError("Материал кругов воды не назначен в инспекторе!", this);
            enabled = false;
            return;
        }

        var main = rippleParticles.main;
        main.startSpeed = config.RippleParticleSpeed;
        main.startSize = config.RippleParticleSize;
        main.startColor = config.RippleParticleColor;
        main.startLifetime = config.RippleParticleLifetime * config.RippleFadeSpeed;
        emissionModule.rateOverTime = 0f;

        var shape = rippleParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = config.RippleEmissionRadius;

        var renderer = rippleParticles.GetComponent<ParticleSystemRenderer>();
        renderer.material = rippleParticleMaterial;
        renderer.sortingLayerName = "Default";
        renderer.sortingOrder = 4;
    }

    private void OnEnable()
    {
        if (shipMovement != null)
        {
            shipMovement.OnMoveInputChanged += HandleMoveInputChanged;
            HandleMoveInputChanged(false);
        }
    }

    private void OnDisable()
    {
        if (shipMovement != null)
        {
            shipMovement.OnMoveInputChanged -= HandleMoveInputChanged;
        }
    }

    private void Update()
    {
        if (!isMoveInputActive && stopTimer > 0f)
        {
            stopTimer -= Time.deltaTime;
            if (stopTimer <= 0f)
            {
                emissionModule.rateOverTime = config.RippleEmissionRate;
            }
        }
    }

    private void HandleMoveInputChanged(bool isInputActive)
    {
        isMoveInputActive = isInputActive;
        stopTimer = StopDelay;
        emissionModule.rateOverTime = 0f;
    }
}
