using UnityEngine;

[CreateAssetMenu(fileName = "MovementSettings", menuName = "Settings/MovementSettings", order = 1)]
public class MovementSettings : ScriptableObject {
    [Header("Standard State Settings")]
    [SerializeField] private float standardMaxSpeed = 5f;
    [SerializeField] private float standardAcceleration = 10f;
    [SerializeField] private float standardDeceleration = 10f;

    [Header("Armed State Settings")]
    [SerializeField] private float armedMaxSpeed = 7f;
    [SerializeField] private float armedAcceleration = 12f;
    [SerializeField] private float armedDeceleration = 12f;

    public float StandardMaxSpeed => standardMaxSpeed;
    public float StandardAcceleration => standardAcceleration;
    public float StandardDeceleration => standardDeceleration;
    public float ArmedMaxSpeed => armedMaxSpeed;
    public float ArmedAcceleration => armedAcceleration;
    public float ArmedDeceleration => armedDeceleration;
}