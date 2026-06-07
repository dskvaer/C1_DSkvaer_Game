using Ship;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem), typeof(TrailRenderer))]
public class ShipEffects : MonoBehaviour {
    [Header("Эффекты движения")]
    [InspectorLabel("Конфиг эффектов")]
    [Tooltip("ScriptableObject с настройками волн и следа на воде.")]
    [SerializeField] private ShipEffectsConfig config;

    [InspectorLabel("Движение корабля")]
    [Tooltip("Компонент движения корабля, по скорости которого включаются волны и след.")]
    [SerializeField] private ShipMovement shipMovement;

    [InspectorLabel("Материал волн")]
    [Tooltip("Материал частиц волн за кораблем.")]
    [SerializeField] private Material waveParticleMaterial;

    private ParticleSystem waveParticles;
    private TrailRenderer trailRenderer;
    private ParticleSystem.EmissionModule emissionModule;

    private void Awake()
    {
        waveParticles = GetComponent<ParticleSystem>();
        trailRenderer = GetComponent<TrailRenderer>();
        emissionModule = waveParticles.emission;

        if (config == null)
        {
            Debug.LogError("ShipEffectsConfig не назначен в инспекторе!", this);
            return;
        }
        if (shipMovement == null)
        {
            Debug.LogError("ShipMovement не назначен в инспекторе!", this);
            return;
        }
        if (waveParticleMaterial == null)
        {
            Debug.LogError("Материал волн не назначен в инспекторе!", this);
            return;
        }

        var main = waveParticles.main;
        main.startSpeed = config.WaveParticleSpeed;
        main.startSize = config.WaveParticleSize;
        main.startColor = config.WaveParticleColor;
        main.startLifetime = config.WaveParticleLifetime;
        emissionModule.rateOverTime = 0f;

        var particleRenderer = waveParticles.GetComponent<ParticleSystemRenderer>();
        particleRenderer.material = waveParticleMaterial;
        particleRenderer.sortingLayerName = "Default";
        particleRenderer.sortingOrder = 5;

        trailRenderer.time = config.TrailTime;
        trailRenderer.startWidth = config.TrailStartWidth;
        trailRenderer.endWidth = config.TrailEndWidth;
        trailRenderer.startColor = config.TrailStartColor;
        trailRenderer.endColor = config.TrailEndColor;
        trailRenderer.emitting = false;
        trailRenderer.sortingLayerName = "Default";
        trailRenderer.sortingOrder = 5;
    }

    private void Update()
    {
        if (shipMovement == null || config == null) return;

        float speed = shipMovement.GetShipVelocity().magnitude;
        emissionModule.rateOverTime = speed >= config.WaveMinSpeed ? config.WaveEmissionRate : 0f;
        trailRenderer.emitting = speed >= config.TrailMinSpeed;
    }
}
