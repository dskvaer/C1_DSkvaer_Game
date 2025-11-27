using Spine;
using Spine.Unity;
using System;

namespace Digimight.Extensions
{
	public static class SkeletonGraphicExtension
	{
		private static void PlayAnimation(this SkeletonGraphic skeleton, string animationName, int trackIndex = 0, bool loop = false, Action onComplete = null)
		{
			if (string.IsNullOrEmpty(animationName)) return;

			TrackEntry trackEntry = skeleton.AnimationState.SetAnimation(trackIndex, animationName, loop);
			trackEntry.Complete += (te) => onComplete?.Invoke();
		}

		public static void PlayAnimationOnShot(this SkeletonGraphic skeleton, string animationName, int trackIndex = 0, Action onComplete = null)
		{
			skeleton.PlayAnimation(animationName, trackIndex, false, onComplete);
		}

		public static void PlayAnimationLoop(this SkeletonGraphic skeleton, string animationName, int trackIndex = 0)
		{
			skeleton.PlayAnimation(animationName, trackIndex, true);
		}
	}
}