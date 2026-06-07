using System;
using C1.Platformer.Characters.Settings;
using UnityEngine;

#pragma warning disable 0414

namespace C1.Platformer.Characters {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlatformerCharacterMotor : MonoBehaviour {
        [Header("Описание")]
        [SerializeField, TextArea(3, 7)] private string inspectorDescription =
            "Главное тело платформерного персонажа. Компонент читает команды из Input Source и управляет Rigidbody2D: ходьба, бег, прыжок, присед, толкание, края платформ, лестницы и вода. Для игрока используйте PlayerPlatformerInputSource или PlatformerTouchInputSource, для NPC - ScriptedPlatformerInputSource.";

        [Header("Основные ссылки")]
        [Tooltip("ScriptableObject с настройками движения. Для персонажей платформера храните его в Platformer/Characters/Settings.")]
        [SerializeField] private PlatformerMovementProfile profile;
        [Tooltip("Компонент, который реализует IPlatformerInputSource. Игрок, мобильный UI или AI могут подать сюда одинаковые команды.")]
        [SerializeField] private MonoBehaviour inputSourceComponent;
        [Tooltip("Основной Collider2D тела персонажа. Нужен для проверок земли, приседа и ledge grab.")]
        [SerializeField] private Collider2D bodyCollider;

        [Header("Опциональные точки проверки")]
        [Tooltip("Точка у ног. Если не указана, рассчитывается по основному коллайдеру.")]
        [SerializeField] private Transform feetProbe;
        [Tooltip("Точка на уровне груди. Используется для лестниц, воды, толкания и проверки края платформы.")]
        [SerializeField] private Transform chestProbe;
        [Tooltip("Точка у головы. Используется для воды и проверки, может ли персонаж встать после приседа.")]
        [SerializeField] private Transform headProbe;

        [Header("Отладка")]
        [Tooltip("Показывает gizmos-точки проверок в Scene View.")]
        [SerializeField] private bool drawDebugGizmos = true;

        private IPlatformerInputSource inputSource;
        private Rigidbody2D rb;
        private BoxCollider2D boxCollider;
        private CapsuleCollider2D capsuleCollider;
        private Vector2 originalColliderSize;
        private Vector2 originalColliderOffset;
        private float originalWorldColliderHeight;
        private PlatformerCharacterIntent intent;
        private PlatformerMovementMode movementMode = PlatformerMovementMode.Airborne;
        private Collider2D groundCollider;
        private Collider2D ladderCollider;
        private Collider2D waterCollider;
        private Rigidbody2D pushBody;
        private Vector2 ledgeHangPosition;
        private Vector2 ledgeStandPosition;
        private float coyoteTimer;
        private float jumpBufferTimer;
        private bool jumpCutQueued;
        private bool grounded;
        private bool inWater;
        private bool headInWater;
        private bool chestInWater;
        private bool crouchColliderApplied;
        private int facingDirection = 1;
        private int externalControlLockCount;
        private string externalControlLockReason;

        public event Action<PlatformerMovementMode, PlatformerMovementMode> OnMovementModeChanged;
        public event Action<PlatformerCharacterMotor> OnJumped;
        public event Action<PlatformerCharacterMotor> OnLanded;
        public event Action<PlatformerCharacterMotor> OnLedgeGrabbed;
        public event Action<PlatformerCharacterMotor> OnLedgeClimbed;

        public PlatformerMovementMode MovementMode => movementMode;
        public PlatformerCharacterIntent CurrentIntent => intent;
        public Rigidbody2D Rigidbody => rb;
        public Collider2D BodyCollider => bodyCollider;
        public Collider2D GroundCollider => groundCollider;
        public Collider2D LadderCollider => ladderCollider;
        public Collider2D WaterCollider => waterCollider;
        public Rigidbody2D PushBody => pushBody;
        public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;
        public int FacingDirection => facingDirection;
        public bool IsGrounded => grounded;
        public bool IsCrouching => movementMode == PlatformerMovementMode.Crouching;
        public bool IsPushing => movementMode == PlatformerMovementMode.Pushing;
        public bool IsHanging => movementMode == PlatformerMovementMode.LedgeHang;
        public bool IsOnLadder => movementMode == PlatformerMovementMode.Ladder;
        public bool IsSwimming => movementMode == PlatformerMovementMode.SwimmingSurface || movementMode == PlatformerMovementMode.SwimmingUnderwater;
        public bool IsUnderwater => movementMode == PlatformerMovementMode.SwimmingUnderwater;
        public bool IsLookingUp => intent.LookUpHeld && grounded && Mathf.Abs(intent.Move.x) < 0.1f;
        public bool IsControlLocked => externalControlLockCount > 0;
        public string ExternalControlLockReason => externalControlLockReason;

        private PlatformerMovementProfile Settings {
            get {
                if (profile == null)
                {
                    profile = ScriptableObject.CreateInstance<PlatformerMovementProfile>();
                    Debug.LogWarning("PlatformerCharacterMotor: No movement profile assigned. Runtime defaults are being used.", this);
                }

                return profile;
            }
        }

        private Bounds BodyBounds => bodyCollider != null ? bodyCollider.bounds : new Bounds(transform.position, Vector3.one);

        private Vector2 FeetPosition {
            get {
                if (feetProbe != null) return feetProbe.position;
                Bounds bounds = BodyBounds;
                return new Vector2(bounds.center.x, bounds.min.y + Settings.ProbeSkin);
            }
        }

        private Vector2 ChestPosition {
            get {
                if (chestProbe != null) return chestProbe.position;
                Bounds bounds = BodyBounds;
                return new Vector2(bounds.center.x, bounds.min.y + Mathf.Max(Settings.ChestProbeHeight, bounds.size.y * 0.45f));
            }
        }

        private Vector2 HeadPosition {
            get {
                if (headProbe != null) return headProbe.position;
                Bounds bounds = BodyBounds;
                return new Vector2(bounds.center.x, bounds.max.y - Settings.HeadProbeInset);
            }
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (bodyCollider == null)
            {
                bodyCollider = GetComponent<Collider2D>();
            }

            if (bodyCollider == null)
            {
                Debug.LogError("PlatformerCharacterMotor: Collider2D not found.", this);
                enabled = false;
                return;
            }

            inputSource = inputSourceComponent as IPlatformerInputSource;
            if (inputSource == null)
            {
                inputSource = GetComponent<IPlatformerInputSource>();
            }

            if (inputSource == null)
            {
                Debug.LogWarning("PlatformerCharacterMotor: No input source found. Character will stay idle until one is assigned.", this);
            }

            boxCollider = bodyCollider as BoxCollider2D;
            capsuleCollider = bodyCollider as CapsuleCollider2D;
            CacheColliderShape();
            ConfigureRigidbody();
        }

        private void OnValidate()
        {
            if (bodyCollider == null)
            {
                bodyCollider = GetComponent<Collider2D>();
            }

            if (inputSourceComponent != null && inputSourceComponent is not IPlatformerInputSource)
            {
                inputSourceComponent = null;
            }
        }

        private void Update()
        {
            intent = inputSource?.CurrentIntent ?? PlatformerCharacterIntent.Neutral;

            if (intent.JumpPressed)
            {
                jumpBufferTimer = Settings.JumpBufferTime;
            }
            else
            {
                jumpBufferTimer -= Time.deltaTime;
            }

            if (intent.JumpReleased)
            {
                jumpCutQueued = true;
            }

            UpdateFacing();
        }

        private void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;

            ProbeEnvironment();
            UpdateCoyoteTimer(deltaTime);

            if (IsControlLocked)
            {
                ApplyExternalControlLock();
                return;
            }

            if (HandleLockedModes(deltaTime))
            {
                return;
            }

            RefreshMovementMode();
            TryConsumeJumpBuffer();
            ApplyCrouchColliderIfNeeded();

            switch (movementMode)
            {
                case PlatformerMovementMode.Ladder:
                    ApplyLadderMovement(deltaTime);
                    break;
                case PlatformerMovementMode.SwimmingSurface:
                case PlatformerMovementMode.SwimmingUnderwater:
                    ApplySwimmingMovement(deltaTime);
                    break;
                default:
                    ApplyGravity();
                    ApplyGroundAirOrPushMovement(deltaTime);
                    ApplyJumpCut();
                    ClampFallSpeed();
                    break;
            }
        }

        public void SetInputSource(IPlatformerInputSource source)
        {
            inputSource = source;
            inputSourceComponent = source as MonoBehaviour;
        }

        public void ForceVelocity(Vector2 velocity)
        {
            rb.linearVelocity = velocity;
        }

        public void SetExternalControlLock(bool locked, string reason = null)
        {
            if (locked)
            {
                externalControlLockCount++;
                externalControlLockReason = string.IsNullOrEmpty(reason) ? "External" : reason;
                rb.linearVelocity = Vector2.zero;
                return;
            }

            externalControlLockCount = Mathf.Max(0, externalControlLockCount - 1);
            if (externalControlLockCount == 0)
            {
                externalControlLockReason = null;
            }
        }

        public void ClearExternalControlLocks()
        {
            externalControlLockCount = 0;
            externalControlLockReason = null;
        }

        public void ApplyAnimationDrivenStep(Vector2 velocity)
        {
            rb.linearVelocity = velocity;
        }

        public void Teleport(Vector2 position)
        {
            rb.position = position;
            rb.linearVelocity = Vector2.zero;
            Physics2D.SyncTransforms();
            ProbeEnvironment();
        }

        private void ConfigureRigidbody()
        {
            rb.gravityScale = Settings.GravityScale;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void ApplyExternalControlLock()
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
        }

        private void CacheColliderShape()
        {
            if (boxCollider != null)
            {
                originalColliderSize = boxCollider.size;
                originalColliderOffset = boxCollider.offset;
            }
            else if (capsuleCollider != null)
            {
                originalColliderSize = capsuleCollider.size;
                originalColliderOffset = capsuleCollider.offset;
            }

            originalWorldColliderHeight = Mathf.Max(BodyBounds.size.y, 0.01f);
        }

        private void ProbeEnvironment()
        {
            bool wasGrounded = grounded;
            int solidMask = SolidMask();
            int ladderMask = Settings.LadderLayers.value;
            int waterMask = Settings.WaterLayers.value;

            groundCollider = FindOverlap(FeetPosition + Vector2.down * Settings.GroundProbeDistance, Settings.GroundProbeRadius, solidMask, false);
            grounded = groundCollider != null && rb.linearVelocity.y <= 0.2f && movementMode != PlatformerMovementMode.LedgeHang && !IsSwimming;

            ladderCollider = ladderMask != 0 ? FindOverlap(ChestPosition, Settings.GroundProbeRadius * 1.2f, ladderMask, true) : null;

            Collider2D waterAtFeet = waterMask != 0 ? FindOverlap(FeetPosition, Settings.GroundProbeRadius, waterMask, true) : null;
            Collider2D waterAtChest = waterMask != 0 ? FindOverlap(ChestPosition, Settings.GroundProbeRadius * 1.3f, waterMask, true) : null;
            Collider2D waterAtHead = waterMask != 0 ? FindOverlap(HeadPosition, Settings.GroundProbeRadius, waterMask, true) : null;

            waterCollider = waterAtChest != null ? waterAtChest : waterAtFeet != null ? waterAtFeet : waterAtHead;
            chestInWater = waterAtChest != null;
            headInWater = waterAtHead != null;
            inWater = waterCollider != null && (chestInWater || headInWater || !grounded);

            if (!wasGrounded && grounded)
            {
                OnLanded?.Invoke(this);
            }
        }

        private void UpdateCoyoteTimer(float deltaTime)
        {
            if (grounded)
            {
                coyoteTimer = Settings.CoyoteTime;
            }
            else
            {
                coyoteTimer -= deltaTime;
            }
        }

        private bool HandleLockedModes(float deltaTime)
        {
            if (movementMode == PlatformerMovementMode.LedgeHang)
            {
                HandleLedgeHang();
                return true;
            }

            if (movementMode == PlatformerMovementMode.Ladder)
            {
                if (ladderCollider == null)
                {
                    SetMovementMode(PlatformerMovementMode.Airborne);
                    return false;
                }

                if (jumpBufferTimer > 0f)
                {
                    SetMovementMode(PlatformerMovementMode.Airborne);
                    PerformJump(Settings.JumpVelocity);
                    return false;
                }

                ApplyLadderMovement(deltaTime);
                return true;
            }

            return false;
        }

        private void RefreshMovementMode()
        {
            pushBody = null;

            if (CanStartLadder())
            {
                StartLadder();
                return;
            }

            if (ShouldSwim())
            {
                SetMovementMode(headInWater || intent.DiveHeld || intent.Move.y < -0.25f
                    ? PlatformerMovementMode.SwimmingUnderwater
                    : PlatformerMovementMode.SwimmingSurface);
                return;
            }

            if (TryStartLedgeHang())
            {
                return;
            }

            if (CanPush(out pushBody))
            {
                SetMovementMode(PlatformerMovementMode.Pushing);
                return;
            }

            if (grounded && ShouldCrouch())
            {
                SetMovementMode(PlatformerMovementMode.Crouching);
                return;
            }

            SetMovementMode(grounded ? PlatformerMovementMode.Grounded : PlatformerMovementMode.Airborne);
        }

        private bool CanStartLadder()
        {
            if (ladderCollider == null || IsSwimming || movementMode == PlatformerMovementMode.LedgeHang)
            {
                return false;
            }

            return intent.InteractPressed || Mathf.Abs(intent.Move.y) > 0.25f;
        }

        private void StartLadder()
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;

            if (ladderCollider != null)
            {
                float snappedX = Mathf.MoveTowards(rb.position.x, ladderCollider.bounds.center.x, Settings.LadderSnapSpeed * Time.fixedDeltaTime);
                rb.position = new Vector2(snappedX, rb.position.y);
            }

            SetMovementMode(PlatformerMovementMode.Ladder);
        }

        private bool ShouldSwim()
        {
            if (!inWater || movementMode == PlatformerMovementMode.Ladder)
            {
                return false;
            }

            if (grounded && !chestInWater && !headInWater)
            {
                return false;
            }

            return true;
        }

        private bool ShouldCrouch()
        {
            return intent.CrouchHeld || (crouchColliderApplied && !CanStandUp());
        }

        private bool CanPush(out Rigidbody2D targetBody)
        {
            targetBody = null;

            if (!grounded || !intent.InteractHeld || Mathf.Abs(intent.Move.x) < 0.15f)
            {
                return false;
            }

            if (Mathf.Sign(intent.Move.x) != facingDirection)
            {
                return false;
            }

            RaycastHit2D hit = Raycast(ChestPosition + Vector2.up * Settings.PushCheckHeight, Vector2.right * facingDirection, Settings.PushCheckDistance, PushableMask(), false);
            if (hit.collider == null)
            {
                return false;
            }

            targetBody = hit.rigidbody != null ? hit.rigidbody : hit.collider.attachedRigidbody;
            return targetBody != null && targetBody != rb;
        }

        private void TryConsumeJumpBuffer()
        {
            if (jumpBufferTimer <= 0f)
            {
                return;
            }

            if (IsSwimming && !headInWater)
            {
                SetMovementMode(PlatformerMovementMode.Airborne);
                PerformJump(Settings.WaterJumpVelocity);
                return;
            }

            if (coyoteTimer > 0f && !IsSwimming && movementMode != PlatformerMovementMode.Ladder)
            {
                PerformJump(Settings.JumpVelocity);
            }
        }

        private void PerformJump(float velocity)
        {
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            grounded = false;
            rb.gravityScale = Settings.GravityScale;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, velocity);
            SetMovementMode(PlatformerMovementMode.Airborne);
            OnJumped?.Invoke(this);
        }

        private void ApplyGroundAirOrPushMovement(float deltaTime)
        {
            float targetSpeed = GetTargetHorizontalSpeed();
            float acceleration = GetHorizontalAcceleration(targetSpeed);
            float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, acceleration * deltaTime);
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);

            if (movementMode == PlatformerMovementMode.Pushing && pushBody != null)
            {
                float pushTarget = facingDirection * Settings.PushSpeed;
                float pushX = Mathf.MoveTowards(pushBody.linearVelocity.x, pushTarget, Settings.PushAcceleration * deltaTime);
                pushBody.linearVelocity = new Vector2(pushX, pushBody.linearVelocity.y);
            }
        }

        private float GetTargetHorizontalSpeed()
        {
            float inputX = Mathf.Abs(intent.Move.x) < 0.05f ? 0f : intent.Move.x;

            return movementMode switch
            {
                PlatformerMovementMode.Crouching => inputX * Settings.CrouchSpeed,
                PlatformerMovementMode.Pushing => inputX * Settings.PushSpeed,
                PlatformerMovementMode.Grounded => inputX * (intent.RunHeld ? Settings.RunSpeed : Settings.WalkSpeed),
                PlatformerMovementMode.Airborne => inputX * (intent.RunHeld ? Settings.RunSpeed : Settings.WalkSpeed),
                _ => 0f
            };
        }

        private float GetHorizontalAcceleration(float targetSpeed)
        {
            bool hasInput = Mathf.Abs(targetSpeed) > 0.01f;
            if (grounded || movementMode == PlatformerMovementMode.Pushing || movementMode == PlatformerMovementMode.Crouching)
            {
                return hasInput ? Settings.Acceleration : Settings.Deceleration;
            }

            return hasInput ? Settings.AirAcceleration : Settings.AirDeceleration;
        }

        private void ApplyGravity()
        {
            if (movementMode == PlatformerMovementMode.LedgeHang || movementMode == PlatformerMovementMode.Ladder)
            {
                rb.gravityScale = 0f;
                return;
            }

            float multiplier = 1f;
            if (rb.linearVelocity.y < -0.01f)
            {
                multiplier = Settings.FallGravityMultiplier;
            }
            else if (rb.linearVelocity.y > 0.01f && !intent.JumpHeld)
            {
                multiplier = Settings.LowJumpGravityMultiplier;
            }

            rb.gravityScale = Settings.GravityScale * multiplier;
        }

        private void ApplyJumpCut()
        {
            if (!jumpCutQueued)
            {
                return;
            }

            jumpCutQueued = false;
            if (rb.linearVelocity.y > 0.01f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * Settings.JumpCutMultiplier);
            }
        }

        private void ClampFallSpeed()
        {
            if (rb.linearVelocity.y < -Settings.MaxFallSpeed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -Settings.MaxFallSpeed);
            }
        }

        private void ApplyLadderMovement(float deltaTime)
        {
            rb.gravityScale = 0f;
            Vector2 targetVelocity = new Vector2(intent.Move.x * Settings.LadderHorizontalSpeed, intent.Move.y * Settings.ClimbSpeed);

            if (ladderCollider != null)
            {
                float snappedX = Mathf.MoveTowards(rb.position.x, ladderCollider.bounds.center.x, Settings.LadderSnapSpeed * deltaTime);
                rb.position = new Vector2(snappedX, rb.position.y);
            }

            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, Settings.SwimAcceleration * deltaTime);

            if (grounded && intent.Move.y < -0.2f)
            {
                SetMovementMode(PlatformerMovementMode.Grounded);
            }
        }

        private void ApplySwimmingMovement(float deltaTime)
        {
            rb.gravityScale = Settings.WaterGravityScale;

            float speed = movementMode == PlatformerMovementMode.SwimmingUnderwater ? Settings.UnderwaterSwimSpeed : Settings.SurfaceSwimSpeed;
            Vector2 targetVelocity = new Vector2(intent.Move.x * speed, intent.Move.y * speed);

            if (movementMode == PlatformerMovementMode.SwimmingSurface && !intent.DiveHeld)
            {
                targetVelocity.y = Mathf.Min(targetVelocity.y, 0.25f);
            }

            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, Settings.SwimAcceleration * deltaTime);

            if (!inWater)
            {
                SetMovementMode(grounded ? PlatformerMovementMode.Grounded : PlatformerMovementMode.Airborne);
            }
        }

        private bool TryStartLedgeHang()
        {
            if (!Settings.AllowLedgeGrab || grounded || IsSwimming || movementMode == PlatformerMovementMode.Ladder || rb.linearVelocity.y > 0.1f)
            {
                return false;
            }

            if (Mathf.Abs(intent.Move.x) < 0.2f)
            {
                return false;
            }

            int dir = Math.Sign(intent.Move.x);
            Vector2 lowerOrigin = new Vector2(BodyBounds.center.x, BodyBounds.min.y + Settings.LedgeLowerHeight);
            Vector2 upperOrigin = new Vector2(BodyBounds.center.x, BodyBounds.min.y + Settings.LedgeUpperHeight);
            Vector2 direction = Vector2.right * dir;

            RaycastHit2D lowerHit = Raycast(lowerOrigin, direction, Settings.LedgeForwardDistance, SolidMask(), false);
            RaycastHit2D upperHit = Raycast(upperOrigin, direction, Settings.LedgeForwardDistance, SolidMask(), false);

            if (lowerHit.collider == null || upperHit.collider != null)
            {
                return false;
            }

            Vector2 topProbe = upperOrigin + direction * Settings.LedgeForwardDistance;
            RaycastHit2D topHit = Raycast(topProbe, Vector2.down, Settings.LedgeDownProbeDistance, SolidMask(), false);
            if (topHit.collider == null)
            {
                return false;
            }

            Bounds bounds = BodyBounds;
            Vector2 standPosition = topHit.point + Vector2.up * (bounds.extents.y + Settings.ProbeSkin) + direction * Settings.LedgeClimbForwardOffset;
            if (!CanOccupy(standPosition, bounds.size * 0.92f))
            {
                return false;
            }

            facingDirection = dir;
            ledgeStandPosition = standPosition;
            ledgeHangPosition = topHit.point + new Vector2(-dir * Settings.LedgeHangHorizontalOffset, -Settings.LedgeHangVerticalOffset);
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            rb.position = ledgeHangPosition;
            SetMovementMode(PlatformerMovementMode.LedgeHang);
            OnLedgeGrabbed?.Invoke(this);
            return true;
        }

        private void HandleLedgeHang()
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            rb.position = ledgeHangPosition;

            if (intent.JumpPressed || intent.InteractPressed || intent.Move.y > 0.45f)
            {
                ClimbLedge();
                return;
            }

            if (intent.CrouchHeld || intent.Move.y < -0.45f || intent.Move.x < -0.35f * facingDirection)
            {
                DropFromLedge();
            }
        }

        private void ClimbLedge()
        {
            rb.position = ledgeStandPosition;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = Settings.GravityScale;
            SetMovementMode(PlatformerMovementMode.Grounded);
            OnLedgeClimbed?.Invoke(this);
        }

        private void DropFromLedge()
        {
            rb.position += Vector2.left * facingDirection * 0.05f;
            rb.gravityScale = Settings.GravityScale;
            SetMovementMode(PlatformerMovementMode.Airborne);
        }

        private void ApplyCrouchColliderIfNeeded()
        {
            bool wantsCrouchCollider = movementMode == PlatformerMovementMode.Crouching;
            if (!wantsCrouchCollider && crouchColliderApplied && !CanStandUp())
            {
                wantsCrouchCollider = true;
                SetMovementMode(PlatformerMovementMode.Crouching);
            }

            if (wantsCrouchCollider == crouchColliderApplied)
            {
                return;
            }

            if (boxCollider != null)
            {
                ApplyBoxCrouch(wantsCrouchCollider);
            }
            else if (capsuleCollider != null)
            {
                ApplyCapsuleCrouch(wantsCrouchCollider);
            }

            crouchColliderApplied = wantsCrouchCollider;
        }

        private void ApplyBoxCrouch(bool crouch)
        {
            if (!crouch)
            {
                boxCollider.size = originalColliderSize;
                boxCollider.offset = originalColliderOffset;
                return;
            }

            float newHeight = originalColliderSize.y * Settings.CrouchColliderHeightMultiplier;
            boxCollider.size = new Vector2(originalColliderSize.x, newHeight);
            boxCollider.offset = originalColliderOffset + Vector2.down * ((originalColliderSize.y - newHeight) * 0.5f);
        }

        private void ApplyCapsuleCrouch(bool crouch)
        {
            if (!crouch)
            {
                capsuleCollider.size = originalColliderSize;
                capsuleCollider.offset = originalColliderOffset;
                return;
            }

            float newHeight = originalColliderSize.y * Settings.CrouchColliderHeightMultiplier;
            capsuleCollider.size = new Vector2(originalColliderSize.x, newHeight);
            capsuleCollider.offset = originalColliderOffset + Vector2.down * ((originalColliderSize.y - newHeight) * 0.5f);
        }

        private bool CanStandUp()
        {
            if (!crouchColliderApplied)
            {
                return true;
            }

            Bounds bounds = BodyBounds;
            float extraHeight = Mathf.Max(0f, originalWorldColliderHeight - bounds.size.y);
            Vector2 center = bounds.center + Vector3.up * (extraHeight * 0.5f);
            Vector2 size = new Vector2(bounds.size.x * 0.9f, originalWorldColliderHeight * 0.95f);
            return CanOccupy(center, size);
        }

        private bool CanOccupy(Vector2 center, Vector2 size)
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, SolidMask());
            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hit = hits[i];
                if (IsBlockingExternalCollider(hit, false))
                {
                    return false;
                }
            }

            return true;
        }

        private void UpdateFacing()
        {
            if (movementMode == PlatformerMovementMode.LedgeHang)
            {
                return;
            }

            if (Mathf.Abs(intent.Move.x) > 0.1f && movementMode != PlatformerMovementMode.Pushing)
            {
                facingDirection = intent.Move.x > 0f ? 1 : -1;
            }
        }

        private void SetMovementMode(PlatformerMovementMode nextMode)
        {
            if (movementMode == nextMode)
            {
                return;
            }

            PlatformerMovementMode previousMode = movementMode;
            movementMode = nextMode;
            OnMovementModeChanged?.Invoke(previousMode, nextMode);
        }

        private Collider2D FindOverlap(Vector2 point, float radius, int mask, bool includeTriggers)
        {
            if (mask == 0)
            {
                return null;
            }

            Collider2D[] hits = Physics2D.OverlapCircleAll(point, radius, mask);
            for (int i = 0; i < hits.Length; i++)
            {
                if (IsBlockingExternalCollider(hits[i], includeTriggers))
                {
                    return hits[i];
                }
            }

            return null;
        }

        private RaycastHit2D Raycast(Vector2 origin, Vector2 direction, float distance, int mask, bool includeTriggers)
        {
            if (mask == 0)
            {
                return default;
            }

            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, distance, mask);
            for (int i = 0; i < hits.Length; i++)
            {
                if (IsBlockingExternalCollider(hits[i].collider, includeTriggers))
                {
                    return hits[i];
                }
            }

            return default;
        }

        private bool IsBlockingExternalCollider(Collider2D candidate, bool includeTriggers)
        {
            if (candidate == null)
            {
                return false;
            }

            if (!includeTriggers && candidate.isTrigger)
            {
                return false;
            }

            if (candidate == bodyCollider || candidate.transform == transform || candidate.transform.IsChildOf(transform))
            {
                return false;
            }

            return true;
        }

        private int SolidMask()
        {
            int mask = Settings.GroundLayers.value | Settings.OneWayPlatformLayers.value | Settings.PushableLayers.value;
            return mask != 0 ? mask : Physics2D.DefaultRaycastLayers;
        }

        private int PushableMask()
        {
            return Settings.PushableLayers.value != 0 ? Settings.PushableLayers.value : SolidMask();
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawDebugGizmos)
            {
                return;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(FeetPosition + Vector2.down * Settings.GroundProbeDistance, Settings.GroundProbeRadius);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(ChestPosition, Settings.GroundProbeRadius * 1.2f);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(HeadPosition, Settings.GroundProbeRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(ChestPosition + Vector2.up * Settings.PushCheckHeight, ChestPosition + Vector2.up * Settings.PushCheckHeight + Vector2.right * facingDirection * Settings.PushCheckDistance);
        }
    }
}
