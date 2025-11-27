using System;
using UnityEngine;

public interface ICollisionHandler {
    void HandleCollision(Collision2D collision);
    event Action OnCollisionHit;
}