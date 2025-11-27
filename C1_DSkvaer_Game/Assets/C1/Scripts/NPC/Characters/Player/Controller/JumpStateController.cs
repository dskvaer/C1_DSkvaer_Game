using UnityEngine;
using System;
using NPC.Characters.Player;

[RequireComponent(typeof(Rigidbody2D))]
public class JumpStateController : MonoBehaviour, IJumpState {
    [SerializeField] private JumpConfig jumpConfig;
    private bool isGrounded = true;
    private bool isJumping;
    private bool canJump = true;
    private bool wasInAir;
    private bool failedJump;
    private bool isLocked;
    private bool canPlayJumpAir = true;
    private JumpPhase currentPhase = JumpPhase.None;
    private Rigidbody2D rb;
    private float jumpTimer;
    private float preJumpVelocityX;

    public Action OnJumpStart { get; set; }
    public Action OnJumpLandComplete { get; set; }
    public Action OnJumpAir { get; set; }
    public Action OnResetJumpPhase { get; set; }

    public bool IsLocked => isLocked;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("JumpStateController: Rigidbody2D not found!", this);
            enabled = false;
            return;
        }
        if (jumpConfig == null)
        {
            Debug.LogError("JumpStateController: JumpConfig not assigned!", this);
            enabled = false;
            return;
        }
        Debug.Log("JumpStateController: Initialized", this);
    }

    private void FixedUpdate()
    {
        bool wasGrounded = isGrounded;
        isGrounded = CheckGround(rb.position, jumpConfig.GroundCheckDistance, jumpConfig.GroundLayer, jumpConfig.PushableLayer);
        Debug.Log($"JumpStateController: FixedUpdate, isGrounded={isGrounded}, currentPhase={currentPhase}, isJumping={isJumping}, velocity={rb.linearVelocity}, isLocked={isLocked}, canPlayJumpAir={canPlayJumpAir}", this);

        if (wasGrounded && !isGrounded && !isJumping && currentPhase != JumpPhase.JumpAir && canPlayJumpAir)
        {
            currentPhase = JumpPhase.JumpAir;
            wasInAir = true;
            preJumpVelocityX = rb.linearVelocity.x;
            OnJumpAir?.Invoke();
            Debug.Log($"JumpStateController: Falling (no jump): phase={currentPhase}, grounded={isGrounded}, velocity={rb.linearVelocity}", this);
        }

        if (isJumping)
        {
            jumpTimer -= Time.fixedDeltaTime;
            if (currentPhase == JumpPhase.JumpStart && jumpTimer <= 0)
            {
                rb.AddForce(new Vector2(preJumpVelocityX, jumpConfig.JumpForce), ForceMode2D.Impulse);
                currentPhase = JumpPhase.JumpAir;
                wasInAir = true;
                isLocked = false;
                if (canPlayJumpAir)
                {
                    OnJumpAir?.Invoke();
                    Debug.Log($"JumpStateController: Applied jump force, phase={currentPhase}, JumpForce={jumpConfig.JumpForce}, velocity={rb.linearVelocity}", this);
                }
                else
                {
                    Debug.Log($"JumpStateController: JumpAir animation blocked, canPlayJumpAir={canPlayJumpAir}", this);
                }
            }
            else if (currentPhase == JumpPhase.JumpLand && jumpTimer <= 0)
            {
                ResetJumpPhase();
                Debug.Log($"JumpStateController: JumpLand completed, resetting phase, velocity={rb.linearVelocity}", this);
            }
        }
    }

    public void Initialize(float jumpForce, float jumpDelay, float groundCheckDistance)
    {
        jumpConfig.JumpForce = jumpForce;
        jumpConfig.JumpDelay = jumpDelay;
        jumpConfig.GroundCheckDistance = groundCheckDistance;
        Debug.Log($"JumpStateController: Initialized with JumpForce={jumpForce}, JumpDelay={jumpDelay}, GroundCheckDistance={groundCheckDistance}", this);
    }

    public void Jump()
    {
        if (!CanJump())
        {
            Debug.LogWarning($"JumpStateController: Jump not possible, CanJump={CanJump()}, isGrounded={isGrounded}, isJumping={isJumping}", this);
            failedJump = true;
            return;
        }

        isJumping = true;
        canJump = false;
        currentPhase = JumpPhase.JumpStart;
        jumpTimer = jumpConfig.JumpStartDuration;
        isLocked = true;
        OnJumpStart?.Invoke();
        Debug.Log($"JumpStateController: Jump started, phase={currentPhase}, jumpTimer={jumpTimer}, velocity={rb.linearVelocity}", this);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        int groundLayerMask = 1 << LayerMask.NameToLayer("Ground");
        int pushableLayerMask = 1 << LayerMask.NameToLayer("Pushable");
        if (((1 << collision.gameObject.layer) & (groundLayerMask | pushableLayerMask)) != 0)
        {
            if (wasInAir && (currentPhase == JumpPhase.JumpAir || currentPhase == JumpPhase.JumpStart))
            {
                isGrounded = true;
                isJumping = true;
                currentPhase = JumpPhase.JumpLand;
                jumpTimer = jumpConfig.JumpEndDuration;
                isLocked = true;
                canPlayJumpAir = false;
                Debug.Log($"JumpStateController: Landed on {(collision.gameObject.layer == LayerMask.NameToLayer("Ground") ? "Ground" : "Pushable")}: {collision.gameObject.name}, phase={currentPhase}, jumpTimer={jumpTimer}, velocity={rb.linearVelocity}, canPlayJumpAir={canPlayJumpAir}", this);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        int groundLayerMask = 1 << LayerMask.NameToLayer("Ground");
        int pushableLayerMask = 1 << LayerMask.NameToLayer("Pushable");
        if (((1 << collision.gameObject.layer) & (groundLayerMask | pushableLayerMask)) != 0)
        {
            if (!CheckGround(rb.position, jumpConfig.GroundCheckDistance, jumpConfig.GroundLayer, jumpConfig.PushableLayer))
            {
                isGrounded = false;
                wasInAir = true;
                if (!isJumping && canPlayJumpAir && currentPhase != JumpPhase.JumpAir)
                {
                    currentPhase = JumpPhase.JumpAir;
                    preJumpVelocityX = rb.linearVelocity.x;
                    OnJumpAir?.Invoke();
                    Debug.Log($"JumpStateController: Left {(collision.gameObject.layer == LayerMask.NameToLayer("Ground") ? "Ground" : "Pushable")}: {collision.gameObject.name}, phase={currentPhase}, grounded={isGrounded}, velocity={rb.linearVelocity}, canPlayJumpAir={canPlayJumpAir}", this);
                }
            }
        }
    }

    public void ResetJumpPhase()
    {
        if (currentPhase == JumpPhase.JumpLand)
        {
            currentPhase = JumpPhase.None;
            isJumping = false;
            canJump = true;
            isLocked = false;
            canPlayJumpAir = true;
            OnJumpLandComplete?.Invoke();
            OnResetJumpPhase?.Invoke();
            Debug.Log($"JumpStateController: ResetJumpPhase: phase set to None, isJumping={isJumping}, canJump={canJump}, velocity={rb.linearVelocity}, isLocked={isLocked}, canPlayJumpAir={canPlayJumpAir}", this);
        }
    }

    public void SetPreJumpVelocity(float velocityX)
    {
        preJumpVelocityX = velocityX;
        Debug.Log($"JumpStateController: Pre-jump velocity set to {velocityX}", this);
    }

    public bool IsGrounded() => isGrounded;
    public bool IsJumping() => isJumping;
    public bool CanJump() => canJump && isGrounded && !isJumping;
    public bool WasInAir() => wasInAir;
    public JumpPhase CurrentPhase() => currentPhase;
    public void SetGrounded(bool grounded) => isGrounded = grounded;
    public void SetJumping(bool jumping) => isJumping = jumping;
    public void SetCanJump(bool canJump) => this.canJump = canJump;
    public void SetWasInAir(bool wasInAir) => this.wasInAir = wasInAir;
    public void SetFailedJump(bool failed) => failedJump = failed;
    public void SetCurrentPhase(JumpPhase phase) => currentPhase = phase;

    public bool CheckGround(Vector2 position, float distance, LayerMask groundLayer, LayerMask pushableLayer)
    {
        Vector2 rayOrigin = position + new Vector2(0f, -0.2f);
        Debug.DrawRay(rayOrigin, Vector2.down * distance, Color.red, 1f);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, distance, groundLayer | pushableLayer);
        bool grounded = hit.collider != null;
        Debug.Log($"JumpStateController: CheckGround, position={position}, distance={distance}, grounded={grounded}, hit.collider={(hit.collider != null ? hit.collider.name : "null")}", this);
        return grounded;
    }
}