using Spine.Unity;
using UnityEngine;

#pragma warning disable 0414

namespace C1.Platformer.Characters {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SkeletonAnimation))]
    public sealed class PlatformerSpineAnimator : MonoBehaviour {
        [Header("Описание")]
        [SerializeField, TextArea(3, 8)] private string inspectorDescription =
            "Мост между PlatformerCharacterMotor и Spine. По состоянию движения выбирает анимацию, разворачивает Skeleton и слушает Spine Events. Для Spine 4.3 оставьте те же имена событий: ControlLock, ControlUnlock, MotorStep, LedgeClimbComplete.";

        [Header("Основные ссылки")]
        [Tooltip("Компонент движения персонажа.")]
        [SerializeField] private PlatformerCharacterMotor motor;
        [Tooltip("Контроллер оружия. Нужен, чтобы выбирать armed idle/walk.")]
        [SerializeField] private PlatformerWeaponController weaponController;
        [Tooltip("Spine SkeletonAnimation персонажа.")]
        [SerializeField] private SkeletonAnimation skeletonAnimation;
        [Tooltip("Разворачивать Skeleton.ScaleX по направлению движения.")]
        [SerializeField] private bool flipWithMotor = true;

        [Header("Имена анимаций Spine")]
        [Tooltip("Обычное ожидание.")]
        [SerializeField] private string idle = "Idle";
        [Tooltip("Ходьба.")]
        [SerializeField] private string walk = "Walk";
        [Tooltip("Бег.")]
        [SerializeField] private string run = "Run";
        [Tooltip("Присед или движение в приседе.")]
        [SerializeField] private string crouch = "Crouch";
        [Tooltip("Персонаж смотрит вверх.")]
        [SerializeField] private string lookUp = "LookUp";
        [Tooltip("Восходящая часть прыжка.")]
        [SerializeField] private string jumpRise = "JumpRise";
        [Tooltip("Падение.")]
        [SerializeField] private string jumpFall = "JumpFall";
        [Tooltip("Толкание предмета.")]
        [SerializeField] private string push = "Push";
        [Tooltip("Висит на краю платформы.")]
        [SerializeField] private string ledgeHang = "LedgeHang";
        [Tooltip("Лестница.")]
        [SerializeField] private string ladder = "Ladder";
        [Tooltip("Плавание на поверхности.")]
        [SerializeField] private string swimSurface = "SwimSurface";
        [Tooltip("Плавание под водой.")]
        [SerializeField] private string swimUnderwater = "SwimUnderwater";
        [Tooltip("Ожидание с оружием.")]
        [SerializeField] private string armedIdle = "ArmedIdle";
        [Tooltip("Ходьба с оружием.")]
        [SerializeField] private string armedWalk = "ArmedWalk";

        [Header("Spine Events")]
        [Tooltip("Событие Spine, которое блокирует управление персонажем до ControlUnlock или конца анимации.")]
        [SerializeField] private string controlLockEvent = "ControlLock";
        [Tooltip("Событие Spine, которое снимает блокировку управления.")]
        [SerializeField] private string controlUnlockEvent = "ControlUnlock";
        [Tooltip("Событие Spine для анимационного шага. Float события = скорость X, Int события = скорость Y * 100.")]
        [SerializeField] private string motorStepEvent = "MotorStep";
        [Tooltip("Событие Spine, которое сообщает, что анимация залезания на платформу завершена.")]
        [SerializeField] private string ledgeClimbCompleteEvent = "LedgeClimbComplete";
        [Tooltip("Индекс трека Spine, который считается управляющим. Пока на нём играет нецикличная анимация, можно держать управление заблокированным событиями.")]
        [SerializeField] private int controlTrackIndex;

        private string currentAnimation;
        private bool lockedBySpineEvent;

        private void Awake()
        {
            if (skeletonAnimation == null && !TryGetComponent(out skeletonAnimation))
            {
                Debug.LogError("PlatformerSpineAnimator: SkeletonAnimation not found.", this);
                enabled = false;
                return;
            }

            if (motor == null)
            {
                motor = GetComponent<PlatformerCharacterMotor>();
            }

            if (weaponController == null)
            {
                weaponController = GetComponent<PlatformerWeaponController>();
            }

            skeletonAnimation.AnimationState.Event += OnSpineEvent;
            skeletonAnimation.AnimationState.Complete += OnSpineComplete;
        }

        private void OnDestroy()
        {
            if (skeletonAnimation != null)
            {
                skeletonAnimation.AnimationState.Event -= OnSpineEvent;
                skeletonAnimation.AnimationState.Complete -= OnSpineComplete;
            }
        }

        private void LateUpdate()
        {
            if (motor == null || skeletonAnimation == null || skeletonAnimation.Skeleton == null)
            {
                return;
            }

            if (flipWithMotor)
            {
                skeletonAnimation.Skeleton.ScaleX = motor.FacingDirection >= 0 ? Mathf.Abs(skeletonAnimation.Skeleton.ScaleX) : -Mathf.Abs(skeletonAnimation.Skeleton.ScaleX);
            }

            string nextAnimation = PickAnimation();
            bool loop = ShouldLoop(motor.MovementMode);
            Play(nextAnimation, loop);
        }

        private string PickAnimation()
        {
            bool armed = weaponController != null && weaponController.IsArmed;
            float speedX = Mathf.Abs(motor.Velocity.x);

            switch (motor.MovementMode)
            {
                case PlatformerMovementMode.Crouching:
                    return crouch;
                case PlatformerMovementMode.Pushing:
                    return push;
                case PlatformerMovementMode.LedgeHang:
                    return ledgeHang;
                case PlatformerMovementMode.Ladder:
                    return ladder;
                case PlatformerMovementMode.SwimmingSurface:
                    return swimSurface;
                case PlatformerMovementMode.SwimmingUnderwater:
                    return swimUnderwater;
                case PlatformerMovementMode.Airborne:
                    return motor.Velocity.y >= 0f ? jumpRise : jumpFall;
                default:
                    if (motor.IsLookingUp)
                    {
                        return lookUp;
                    }

                    if (speedX > 0.2f)
                    {
                        if (armed && !string.IsNullOrEmpty(armedWalk)) return armedWalk;
                        return motor.CurrentIntent.RunHeld ? run : walk;
                    }

                    if (armed && !string.IsNullOrEmpty(armedIdle)) return armedIdle;
                    return idle;
            }
        }

        private bool ShouldLoop(PlatformerMovementMode mode)
        {
            return mode != PlatformerMovementMode.Airborne;
        }

        private void Play(string animationName, bool loop)
        {
            if (string.IsNullOrEmpty(animationName) || currentAnimation == animationName)
            {
                return;
            }

            if (skeletonAnimation.SkeletonDataAsset == null)
            {
                return;
            }

            currentAnimation = animationName;
            skeletonAnimation.AnimationState.SetAnimation(controlTrackIndex, animationName, loop);
        }

        private void OnSpineEvent(Spine.TrackEntry trackEntry, Spine.Event spineEvent)
        {
            if (motor == null || spineEvent == null || trackEntry == null || trackEntry.TrackIndex != controlTrackIndex)
            {
                return;
            }

            string eventName = spineEvent.Data?.Name;
            if (eventName == controlLockEvent)
            {
                lockedBySpineEvent = true;
                motor.SetExternalControlLock(true, "SpineEvent");
            }
            else if (eventName == controlUnlockEvent)
            {
                UnlockFromSpine();
            }
            else if (eventName == motorStepEvent)
            {
                float x = spineEvent.Float * motor.FacingDirection;
                float y = spineEvent.Int * 0.01f;
                motor.ApplyAnimationDrivenStep(new Vector2(x, y));
            }
            else if (eventName == ledgeClimbCompleteEvent)
            {
                UnlockFromSpine();
            }
        }

        private void OnSpineComplete(Spine.TrackEntry trackEntry)
        {
            if (trackEntry != null && trackEntry.TrackIndex == controlTrackIndex)
            {
                UnlockFromSpine();
            }
        }

        private void UnlockFromSpine()
        {
            if (!lockedBySpineEvent || motor == null)
            {
                return;
            }

            lockedBySpineEvent = false;
            motor.SetExternalControlLock(false, "SpineEvent");
        }
    }
}
