using Spine.Unity;
using System;
using UnityEngine;

[Serializable]
public class AnimationConfig {
    [SerializeField, SpineAnimation(dataField = nameof(asset))] private string animation;
    [SerializeField] private SkeletonDataAsset asset;
    [SerializeField] private bool loop;
    [SerializeField] private float speed = 1f;

    public string Animation => animation;
    public bool Loop => loop;
    public float Speed => speed;

    public bool IsValid() => !string.IsNullOrEmpty(animation) && speed > 0f;
}