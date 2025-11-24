using PrimeTween;
using UnityEngine;

namespace ATCG.Utilities
{
    public static class TweenUtilities
    {
        public static Tween Show(this CanvasGroup canvasGroup, float duration)
        {
            Tween.StopAll(canvasGroup);
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            return Tween.Alpha(canvasGroup, 1, duration);
        }

        public static Tween Hide(this CanvasGroup canvasGroup, float duration)
        {
            Tween.StopAll(canvasGroup);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            return Tween.Alpha(canvasGroup, 0, duration);
        }
    }
}