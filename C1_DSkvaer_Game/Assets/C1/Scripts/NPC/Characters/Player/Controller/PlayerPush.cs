using UnityEngine;
using UnityEngine.InputSystem;
using Spine.Unity;

namespace NPC.Characters.Player {
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerPush : MonoBehaviour, IPush {
        [SerializeField] private PushConfig pushConfig;

        private SkeletonAnimation _skeletonAnimation;
        private InputSystem_Actions _inputSystem;
        private Rigidbody2D _rigidbody;
        private Vector2 _moveInput;
        private bool _isInteractHeld;
        private bool _isInPushContact;
        private float _currentPushForce;
        private bool _isInitialized;
        private Collider2D _lastHitCollider;
        private Rigidbody2D _lastPushableRb;

        public bool IsPushing => _isInitialized && _isInPushContact && _isInteractHeld && _moveInput.x != 0 && IsSkeletonValid() && Mathf.Sign(_moveInput.x) == _skeletonAnimation.Skeleton.ScaleX;
        public bool IsInPushContact => _isInPushContact;
        public bool IsInteractHeld => _isInteractHeld;
        public PushConfig Config => pushConfig;
        public bool IsInitialized => _isInitialized;

        private void Awake()
        {
            _inputSystem = new();
            _rigidbody = GetComponent<Rigidbody2D>();
            if (_rigidbody == null)
            {
                LogError("Rigidbody2D not found!");
                enabled = false;
                return;
            }

            if (pushConfig == null)
            {
                LogError("PushConfig not assigned in Inspector!");
                enabled = false;
                return;
            }

            Log("Initialized (Awake)");
        }

        public void Initialize(SkeletonAnimation skeletonAnimation)
        {
            _skeletonAnimation = skeletonAnimation;
            if (!IsSkeletonValid())
            {
                LogError($"SkeletonAnimation initialization failed: SkeletonAnimation={_skeletonAnimation}, SkeletonDataAsset={(_skeletonAnimation != null ? _skeletonAnimation.SkeletonDataAsset : "null")}, Skeleton={(_skeletonAnimation != null ? _skeletonAnimation.Skeleton : "null")}");
                enabled = false;
                return;
            }

            SetupInput();
            _isInitialized = true;
            Log("Initialized with SkeletonAnimation");
        }

        private void SetupInput()
        {
            _inputSystem.Player.Move.performed += context => _moveInput = context.ReadValue<Vector2>();
            _inputSystem.Player.Move.canceled += context => _moveInput = Vector2.zero;

            _inputSystem.Player.Interact.performed += _ => {
                _isInteractHeld = true;
                _currentPushForce = pushConfig != null ? pushConfig.MaxPushForce : 0f;
                Log("Interact performed");
            };
            _inputSystem.Player.Interact.canceled += _ => {
                _isInteractHeld = false;
                _currentPushForce = 0f;
                _lastHitCollider = null;
                _lastPushableRb = null;
                Log("Interact canceled");
            };
        }

        public void EnableInput()
        {
            if (_inputSystem != null)
            {
                _inputSystem.Enable();
                Log("Input enabled");
            }
        }

        public void DisableInput()
        {
            if (_inputSystem != null)
            {
                _inputSystem.Disable();
                Log("Input disabled");
            }
        }

        public void UpdatePush()
        {
            if (!enabled || !_isInitialized)
            {
                LogError("UpdatePush: Not initialized!");
                return;
            }

            bool wasInPushContact = _isInPushContact;
            bool isPushable;
            _isInPushContact = CheckPushContact(out isPushable);

            if (_isInPushContact && !wasInPushContact) Log("Push contact detected");
            if (!_isInPushContact && wasInPushContact) Log("Push contact lost");

            if (!pushConfig.UseCycleImpulse)
            {
                UpdatePushForce(isPushable);
            }
        }

        private bool CheckPushContact(out bool isPushable)
        {
            isPushable = false;
            if (!IsSkeletonValid())
            {
                LogError($"CheckPushContact: SkeletonAnimation or Skeleton is null! SkeletonAnimation={_skeletonAnimation}, Skeleton={(_skeletonAnimation != null ? _skeletonAnimation.Skeleton : "null")}");
                _isInPushContact = false;
                return false;
            }

            Vector2 rayOrigin = _rigidbody.position + new Vector2(0f, 0.5f);
            Vector2 rayDirection = new Vector2(_skeletonAnimation.Skeleton.ScaleX, 0f);

            int mask = (1 << pushConfig.PushableLayer) | (1 << pushConfig.ObstacleLayer);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, pushConfig.PushCheckDistance, mask);
            Debug.DrawRay(rayOrigin, rayDirection * pushConfig.PushCheckDistance, Color.red, 0.1f);

            _lastHitCollider = null;
            _lastPushableRb = null;

            if (hit.collider != null)
            {
                Log($"Raycast hit: {hit.collider.name}, tag={hit.collider.tag}, distance={hit.distance}");
                if (hit.collider.CompareTag("Pushable"))
                {
                    if (hit.collider.TryGetComponent<Rigidbody2D>(out var pushableRb))
                    {
                        _lastHitCollider = hit.collider;
                        _lastPushableRb = pushableRb;

                        if (!pushConfig.UseCycleImpulse && _isInteractHeld && _moveInput.x != 0 && Mathf.Sign(_moveInput.x) == _skeletonAnimation.Skeleton.ScaleX)
                        {
                            float pushForce = _moveInput.x * _currentPushForce;
                            if (!float.IsNaN(pushForce) && !float.IsInfinity(pushForce))
                            {
                                pushableRb.AddForce(Vector2.right * pushForce, ForceMode2D.Force);
                                isPushable = true;
                                Log($"Continuous pushing {hit.collider.gameObject.name}, force={pushForce}");
                            }
                        }
                        else
                        {
                            isPushable = true;
                        }
                    }
                    return true;
                }
                if (hit.collider.gameObject.layer == pushConfig.ObstacleLayer)
                {
                    _lastHitCollider = hit.collider;
                    _lastPushableRb = null;
                    Log($"Hit obstacle/wall: {_lastHitCollider.name}");
                    return true;
                }
            }
            Log($"Raycast missed: direction={rayDirection}, distance={pushConfig.PushCheckDistance}, layer={mask}");
            return false;
        }

        private void UpdatePushForce(bool isPushable)
        {
            if (pushConfig == null) return;
            float targetPushForce = isPushable ? pushConfig.MaxPushForce : 0f;
            _currentPushForce = Mathf.Lerp(_currentPushForce, targetPushForce, Mathf.Clamp01(pushConfig.PushForceLerpSpeed * Time.fixedDeltaTime));
        }

        private void UpdateCharacterOrientation()
        {
            if (_moveInput.x != 0 && !_isInPushContact && IsSkeletonValid())
            {
                float newScaleX = _moveInput.x > 0 ? 1f : -1f;
                if (_skeletonAnimation.Skeleton.ScaleX != newScaleX)
                {
                    _skeletonAnimation.Skeleton.ScaleX = newScaleX;
                    Log($"Updated orientation, ScaleX={newScaleX}");
                }
            }
        }

        private bool IsSkeletonValid() => _skeletonAnimation != null && _skeletonAnimation.Skeleton != null && _skeletonAnimation.SkeletonDataAsset != null;

        private void Update()
        {
            UpdatePush();
            UpdateCharacterOrientation();
        }

        private void OnEnable()
        {
            if (_isInitialized)
            {
                EnableInput();
            }
        }

        private void OnDisable()
        {
            DisableInput();
        }

        public void OnPushCycle()
        {
            if (!_isInitialized) return;
            if (!pushConfig.UseCycleImpulse) return;

            if (!_isInPushContact || !_isInteractHeld || _lastPushableRb == null) return;
            if (Mathf.Abs(_moveInput.x) < 0.1f) return;
            if (Mathf.Sign(_moveInput.x) != _skeletonAnimation.Skeleton.ScaleX) return;

            float forceMagnitude = _currentPushForce;
            if (forceMagnitude <= 0f) forceMagnitude = pushConfig.MaxPushForce;

            Vector2 dir = Vector2.right * Mathf.Sign(_moveInput.x);
            _lastPushableRb.AddForce(dir * forceMagnitude, ForceMode2D.Impulse);
            Log($"OnPushCycle: applied impulse={forceMagnitude} to {_lastPushableRb.name}");

            _currentPushForce *= pushConfig.CycleDecay;
            if (_currentPushForce < pushConfig.MinForceThreshold)
            {
                _currentPushForce = 0f;
            }
        }

        private void Log(string message)
        {
            if (pushConfig != null && pushConfig.ShouldLog) Debug.Log($"PlayerPush: {message}", this);
        }

        private void LogError(string message) => Debug.LogError($"PlayerPush: {message}", this);
    }
}