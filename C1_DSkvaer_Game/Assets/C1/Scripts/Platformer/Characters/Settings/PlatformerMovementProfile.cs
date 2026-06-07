using UnityEngine;

#pragma warning disable 0414

namespace C1.Platformer.Characters.Settings {
    [CreateAssetMenu(fileName = "PlatformerMovementProfile", menuName = "C1/Platformer/Characters/Movement Profile")]
    public sealed class PlatformerMovementProfile : ScriptableObject {
        [Header("Описание")]
        [SerializeField, TextArea(3, 7)] private string inspectorDescription =
            "Основные настройки платформерного персонажа. Один профиль можно использовать для героя, NPC или тестового персонажа. Слои Ladder и Interactable нужно добавить в Project Settings/Tags and Layers, если вы хотите лестницы и интерактивные объекты.";

        [Header("Слои")]
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private LayerMask oneWayPlatformLayers;
        [SerializeField] private LayerMask pushableLayers;
        [SerializeField] private LayerMask ladderLayers;
        [SerializeField] private LayerMask waterLayers;
        [SerializeField] private LayerMask interactableLayers;

        [Header("Движение по земле")]
        [SerializeField] private float walkSpeed = 4.5f;
        [SerializeField] private float runSpeed = 7.25f;
        [SerializeField] private float crouchSpeed = 2.1f;
        [SerializeField] private float acceleration = 70f;
        [SerializeField] private float deceleration = 90f;
        [SerializeField] private float airAcceleration = 35f;
        [SerializeField] private float airDeceleration = 30f;

        [Header("Прыжок")]
        [SerializeField] private float jumpVelocity = 12.5f;
        [SerializeField] private float coyoteTime = 0.12f;
        [SerializeField] private float jumpBufferTime = 0.12f;
        [SerializeField, Range(0.1f, 1f)] private float jumpCutMultiplier = 0.45f;
        [SerializeField] private float gravityScale = 3f;
        [SerializeField] private float fallGravityMultiplier = 1.65f;
        [SerializeField] private float lowJumpGravityMultiplier = 1.25f;
        [SerializeField] private float maxFallSpeed = 18f;

        [Header("Проверки окружения")]
        [SerializeField] private float probeSkin = 0.03f;
        [SerializeField] private float groundProbeRadius = 0.14f;
        [SerializeField] private float groundProbeDistance = 0.08f;
        [SerializeField] private float chestProbeHeight = 0.6f;
        [SerializeField] private float headProbeInset = 0.08f;

        [Header("Присед")]
        [SerializeField, Range(0.35f, 1f)] private float crouchColliderHeightMultiplier = 0.55f;

        [Header("Толкание")]
        [SerializeField] private float pushSpeed = 2.1f;
        [SerializeField] private float pushAcceleration = 18f;
        [SerializeField] private float pushCheckDistance = 0.45f;
        [SerializeField] private float pushCheckHeight = 0.35f;

        [Header("Края платформ")]
        [SerializeField] private bool allowLedgeGrab = true;
        [SerializeField] private float ledgeForwardDistance = 0.42f;
        [SerializeField] private float ledgeLowerHeight = 0.25f;
        [SerializeField] private float ledgeUpperHeight = 1.05f;
        [SerializeField] private float ledgeDownProbeDistance = 1.1f;
        [SerializeField] private float ledgeHangHorizontalOffset = 0.24f;
        [SerializeField] private float ledgeHangVerticalOffset = 0.52f;
        [SerializeField] private float ledgeClimbForwardOffset = 0.38f;

        [Header("Лестницы")]
        [SerializeField] private float climbSpeed = 3.25f;
        [SerializeField] private float ladderHorizontalSpeed = 1.3f;
        [SerializeField] private float ladderSnapSpeed = 12f;

        [Header("Вода")]
        [SerializeField] private float surfaceSwimSpeed = 3.25f;
        [SerializeField] private float underwaterSwimSpeed = 3.6f;
        [SerializeField] private float swimAcceleration = 22f;
        [SerializeField] private float waterGravityScale = 0.35f;
        [SerializeField] private float waterJumpVelocity = 8f;

        public LayerMask GroundLayers => groundLayers;
        public LayerMask OneWayPlatformLayers => oneWayPlatformLayers;
        public LayerMask PushableLayers => pushableLayers;
        public LayerMask LadderLayers => ladderLayers;
        public LayerMask WaterLayers => waterLayers;
        public LayerMask InteractableLayers => interactableLayers;
        public float WalkSpeed => walkSpeed;
        public float RunSpeed => runSpeed;
        public float CrouchSpeed => crouchSpeed;
        public float Acceleration => acceleration;
        public float Deceleration => deceleration;
        public float AirAcceleration => airAcceleration;
        public float AirDeceleration => airDeceleration;
        public float JumpVelocity => jumpVelocity;
        public float CoyoteTime => coyoteTime;
        public float JumpBufferTime => jumpBufferTime;
        public float JumpCutMultiplier => jumpCutMultiplier;
        public float GravityScale => gravityScale;
        public float FallGravityMultiplier => fallGravityMultiplier;
        public float LowJumpGravityMultiplier => lowJumpGravityMultiplier;
        public float MaxFallSpeed => maxFallSpeed;
        public float ProbeSkin => probeSkin;
        public float GroundProbeRadius => groundProbeRadius;
        public float GroundProbeDistance => groundProbeDistance;
        public float ChestProbeHeight => chestProbeHeight;
        public float HeadProbeInset => headProbeInset;
        public float CrouchColliderHeightMultiplier => crouchColliderHeightMultiplier;
        public float PushSpeed => pushSpeed;
        public float PushAcceleration => pushAcceleration;
        public float PushCheckDistance => pushCheckDistance;
        public float PushCheckHeight => pushCheckHeight;
        public bool AllowLedgeGrab => allowLedgeGrab;
        public float LedgeForwardDistance => ledgeForwardDistance;
        public float LedgeLowerHeight => ledgeLowerHeight;
        public float LedgeUpperHeight => ledgeUpperHeight;
        public float LedgeDownProbeDistance => ledgeDownProbeDistance;
        public float LedgeHangHorizontalOffset => ledgeHangHorizontalOffset;
        public float LedgeHangVerticalOffset => ledgeHangVerticalOffset;
        public float LedgeClimbForwardOffset => ledgeClimbForwardOffset;
        public float ClimbSpeed => climbSpeed;
        public float LadderHorizontalSpeed => ladderHorizontalSpeed;
        public float LadderSnapSpeed => ladderSnapSpeed;
        public float SurfaceSwimSpeed => surfaceSwimSpeed;
        public float UnderwaterSwimSpeed => underwaterSwimSpeed;
        public float SwimAcceleration => swimAcceleration;
        public float WaterGravityScale => waterGravityScale;
        public float WaterJumpVelocity => waterJumpVelocity;
    }
}
