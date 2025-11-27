using System;

public interface IHit {
    void Hit();
    bool IsHitAnimationActive { get; }
    event Action OnHitComplete;
}