public interface IEdgeBalanceAnimation {
    void Play();
    void PlayIdleEdge();
    bool IsAtEdge();
    bool IsOtherAnimationActive();
}