using UnityEngine;

public class EdgeBalanceController : MonoBehaviour {
    [SerializeField] private EdgeBalanceConfigSO config;
    [SerializeField] private EdgeBalanceChecker edgeBalanceChecker;
    [SerializeField] private EdgeBalanceAnimation edgeBalanceAnimation;
    [SerializeField] private AnimationStateManager animationStateManager;

    private IEdgeBalanceChecker _checker;
    private IEdgeBalanceAnimation _animation;
    private string _lastPlayedAnimation;

    private void Awake()
    {
        if (edgeBalanceChecker == null)
        {
            if (!TryGetComponent<EdgeBalanceChecker>(out edgeBalanceChecker))
            {
                Debug.LogError("EdgeBalanceController: EdgeBalanceChecker не привязан и не найден на объекте!", this);
                enabled = false;
                return;
            }
        }

        if (edgeBalanceAnimation == null)
        {
            if (!TryGetComponent<EdgeBalanceAnimation>(out edgeBalanceAnimation))
            {
                Debug.LogError("EdgeBalanceController: EdgeBalanceAnimation не привязан и не найден на объекте!", this);
                enabled = false;
                return;
            }
        }

        if (animationStateManager == null)
        {
            if (!TryGetComponent<AnimationStateManager>(out animationStateManager))
            {
                Debug.LogError("EdgeBalanceController: AnimationStateManager не привязан и не найден на объекте!", this);
                enabled = false;
                return;
            }
        }

        _checker = edgeBalanceChecker;
        _animation = edgeBalanceAnimation;

        if (config == null)
        {
            Debug.LogError("EdgeBalanceController: EdgeBalanceConfigSO не привязан!", this);
            enabled = false;
            return;
        }

        if (_checker == null)
        {
            Debug.LogError($"EdgeBalanceController: IEdgeBalanceChecker не привязан! Проверяемый компонент: {(edgeBalanceChecker != null ? edgeBalanceChecker.GetType().Name : "None")}", this);
            enabled = false;
            return;
        }

        if (_animation == null)
        {
            Debug.LogError($"EdgeBalanceController: IEdgeBalanceAnimation не привязан! Проверяемый компонент: {(edgeBalanceAnimation != null ? edgeBalanceAnimation.GetType().Name : "None")}", this);
            enabled = false;
            return;
        }

        if (!config.IsValid())
        {
            Debug.LogError("EdgeBalanceController: EdgeBalanceConfigSO имеет недействительную конфигурацию!", this);
            enabled = false;
            return;
        }

        Debug.Log($"EdgeBalanceController: Инициализирован, checker={(_checker != null ? edgeBalanceChecker.name : "None")}, animation={(_animation != null ? edgeBalanceAnimation.name : "None")}", this);
    }

    private void Update()
    {
        if (_checker.IsGrounded() && _checker.IsAtEdge() && !_animation.IsOtherAnimationActive())
        {
            var edgeBalanceAnim = animationStateManager.GetAnimation(CharacterAnimationType.EdgeBalance);
            if (edgeBalanceAnim.IsValid() && _lastPlayedAnimation != edgeBalanceAnim.Animation)
            {
                _animation.PlayIdleEdge();
                _lastPlayedAnimation = edgeBalanceAnim.Animation;
                Debug.Log($"EdgeBalanceController: Playing EdgeBalance animation, animation={edgeBalanceAnim.Animation}", this);
            }
        }
        else
        {
            _lastPlayedAnimation = null;
            Debug.Log($"EdgeBalanceController: Not playing EdgeBalance - grounded={_checker.IsGrounded()}, atEdge={_checker.IsAtEdge()}, otherAnimationActive={_animation.IsOtherAnimationActive()}", this);
        }
    }
}