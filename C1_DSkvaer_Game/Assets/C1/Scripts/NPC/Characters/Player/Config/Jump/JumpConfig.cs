using UnityEngine;

[CreateAssetMenu(fileName = "JumpConfig", menuName = "Character/JumpConfig")]
public class JumpConfig : ScriptableObject {
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float jumpDelay = 0.5f;
    [SerializeField] private float jumpStartDuration = 0.5f;
    [SerializeField] private float jumpEndDuration = 0.3f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask pushableLayer;

    public float JumpForce { get => jumpForce; set => jumpForce = value; }
    public float JumpDelay { get => jumpDelay; set => jumpDelay = value; }
    public float JumpStartDuration { get => jumpStartDuration; set => jumpStartDuration = value; }
    public float JumpEndDuration { get => jumpEndDuration; set => jumpEndDuration = value; }
    public float GroundCheckDistance { get => groundCheckDistance; set => groundCheckDistance = value; }
    public LayerMask GroundLayer { get => groundLayer; set => groundLayer = value; }
    public LayerMask PushableLayer { get => pushableLayer; set => pushableLayer = value; }
}