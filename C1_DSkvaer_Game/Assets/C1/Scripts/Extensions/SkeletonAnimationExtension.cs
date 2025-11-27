using Spine;
using Spine.Unity;
using System;
using UnityEngine;

namespace Digimight.Extensions
{
	public static class SkeletonAnimationExtension
	{
		public static void SetSkin(this SkeletonAnimation skeletonAnimation, string skinName)
		{
			Skin newSkin = new Skin("~Skin");
			Skin requireSkin = skeletonAnimation.Skeleton.Data.FindSkin(skinName);

			Debug.Assert(requireSkin != null, $"Null skin for \"{skinName}\"");
			if (requireSkin == null) return;
			newSkin?.AddSkin(requireSkin);

			skeletonAnimation.ApplySkin(newSkin);
		}

		public static void AddSkin(this SkeletonAnimation skeletonAnimation, string skinName)
		{
			Skin mySkin = skeletonAnimation.Skeleton.Skin;
			Skin newSkin = skeletonAnimation.Skeleton.Data.FindSkin(skinName);

			Debug.Assert(newSkin != null, $"Null skin for \"{skinName}\"");
			if (newSkin == null) return;
			mySkin?.AddSkin(newSkin);

			skeletonAnimation.ApplySkin(mySkin);
		}

		public static void ApplySkin(this SkeletonAnimation skeletonAnimation, Skin skin)
		{
			skeletonAnimation.Skeleton.SetSkin(skin);
			skeletonAnimation.Skeleton.SetSlotsToSetupPose();
			skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);
		}

		private static void PlayAnimation(this SkeletonAnimation skeletonAnimation, string animationName, int trackIndex = 0, bool loop = false, float normalizedFramePoint = 0, Action onComplete = null)
		{
			if (string.IsNullOrEmpty(animationName)) return;

			TrackEntry trackEntry = skeletonAnimation.AnimationState.SetAnimation(trackIndex, animationName, loop);
			trackEntry.AnimationStart = trackEntry.AnimationEnd * normalizedFramePoint;
			trackEntry.Complete += (te) => onComplete?.Invoke();
		}

		public static void PlayAnimationOnShot(this SkeletonAnimation skeletonAnimation, string animationName, Action onComplete = null, int trackIndex = 0, float normalizedFramePoint = 0)
		{
			skeletonAnimation.PlayAnimation(animationName, trackIndex, false, normalizedFramePoint, onComplete);
		}

		public static void PlayAnimationOnShot(this SkeletonAnimation skeletonAnimation, string animationName, Action onComplete = null, int trackIndex = 0, float normalizedFramePoint = 0, float speed = 1f)
        {
			skeletonAnimation.AnimationState.TimeScale = speed;
            skeletonAnimation.PlayAnimation(animationName, trackIndex, false, normalizedFramePoint, onComplete);
        }

        public static void PlayAnimationLoop(this SkeletonAnimation skeletonAnimation, string animationName, int trackIndex = 0)
		{
			skeletonAnimation.PlayAnimation(animationName, trackIndex, true);
		}
	}
}