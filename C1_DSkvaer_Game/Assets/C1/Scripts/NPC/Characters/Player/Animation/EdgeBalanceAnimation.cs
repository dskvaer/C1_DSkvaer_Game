using UnityEngine;
using NPC.Characters.Player;

public class EdgeBalanceAnimation : MonoBehaviour, IEdgeBalanceAnimation, IAnimation {
    [SerializeField] private EdgeBalanceConfigSO config;
    [SerializeField] private EdgeBalanceChecker edgeBalanceChecker;
    private IAnimationController spineController;
    private AnimationStateManager animationStateManager;

    private void Awake()
    {
        spineController = GetComponent<IAnimationController>();
        if (spineController == null)
        {
            Debug.LogError("EdgeBalanceAnimation: IAnimationController не найден на объекте!", this);
            enabled = false;
            return;
        }

        animationStateManager = GetComponent<AnimationStateManager>();
        if (animationStateManager == null)
        {
            Debug.LogError("EdgeBalanceAnimation: AnimationStateManager не найден на объекте!", this);
            enabled = false;
            return;
        }

        if (config == null)
        {
            Debug.LogError("EdgeBalanceAnimation: EdgeBalanceConfigSO не привязан!", this);
            enabled = false;
            return;
        }

        if (edgeBalanceChecker == null)
        {
            edgeBalanceChecker = GetComponent<EdgeBalanceChecker>();
            if (edgeBalanceChecker == null)
            {
                Debug.LogError("EdgeBalanceAnimation: EdgeBalanceChecker не привязан и не найден на объекте!", this);
                enabled = false;
                return;
            }
        }

        if (!config.IsValid())
        {
            Debug.LogError("EdgeBalanceAnimation: EdgeBalanceConfigSO имеет недействительную конфигурацию!", this);
            enabled = false;
            return;
        }

        var edgeAnim = animationStateManager.GetAnimation(CharacterAnimationType.EdgeBalance);
        if (!edgeAnim.IsValid())
        {
            Debug.LogError("EdgeBalanceAnimation: Анимация EdgeBalance не найдена в конфигурации!", this);
            enabled = false;
            return;
        }
        Debug.Log($"EdgeBalanceAnimation: Инициализирован успешно, EdgeBalance Animation: {edgeAnim.Animation}, Loop: {edgeAnim.Loop}, Speed: {edgeAnim.Speed}", this);
    }

    public void Play()
    {
        PlayIdleEdge();
    }

    public void PlayIdleEdge()
    {
        if (!enabled)
        {
            Debug.LogError("EdgeBalanceAnimation: Компонент отключен, воспроизведение невозможно!", this);
            return;
        }

        var edgeAnim = animationStateManager.GetAnimation(CharacterAnimationType.EdgeBalance);
        if (!edgeAnim.IsValid())
        {
            Debug.LogError($"EdgeBalanceAnimation: Анимация EdgeBalance невалидна. AnimationName: {(edgeAnim.Animation ?? "null")}", this);
            return;
        }

        string edgeAnimName = animationStateManager.GetAnimationName(CharacterAnimationType.EdgeBalance);
        if (spineController.CurrentAnimation != edgeAnimName)
        {
            spineController.PlayAnimation(edgeAnimName, edgeAnim.Loop, edgeAnim.Speed);
            Debug.Log($"EdgeBalanceAnimation: Воспроизведена анимация EdgeBalance: {edgeAnimName}, Loop: {edgeAnim.Loop}, Speed: {edgeAnim.Speed}, CurrentAnimation: {(spineController.CurrentAnimation ?? "null")}", this);
        }
        else
        {
            Debug.Log($"EdgeBalanceAnimation: Анимация EdgeBalance уже воспроизводится, пропуск. CurrentAnimation: {spineController.CurrentAnimation}", this);
        }
    }

    public bool IsOtherAnimationActive()
    {
        if (spineController == null || animationStateManager == null || string.IsNullOrEmpty(spineController.CurrentAnimation))
        {
            Debug.LogWarning($"EdgeBalanceAnimation: SpineAnimationController, AnimationStateManager или CurrentAnimation не инициализированы! SpineController: {(spineController == null ? "null" : "exists")}, AnimationStateManager: {(animationStateManager == null ? "null" : "exists")}, CurrentAnimation: {(spineController.CurrentAnimation ?? "null")}", this);
            return false;
        }

        var jumpAirAnim = animationStateManager.GetAnimation(CharacterAnimationType.Jump, 1);
        bool isActive = spineController.IsJumpStartOrEnd ||
                        (jumpAirAnim.IsValid() && spineController.CurrentAnimation == animationStateManager.GetAnimationName(CharacterAnimationType.Jump, 1));

        Debug.Log($"EdgeBalanceAnimation: IsOtherAnimationActive={isActive}, CurrentAnimation: {spineController.CurrentAnimation}, JumpAirAnimation: {(jumpAirAnim.IsValid() ? jumpAirAnim.Animation : "null")}", this);
        return isActive;
    }

    public bool IsAtEdge()
    {
        if (edgeBalanceChecker == null)
        {
            Debug.LogError("EdgeBalanceAnimation: EdgeBalanceChecker не инициализирован!", this);
            return false;
        }
        bool atEdge = edgeBalanceChecker.IsAtEdge();
        Debug.Log($"EdgeBalanceAnimation: IsAtEdge={atEdge}", this);
        return atEdge;
    }
}