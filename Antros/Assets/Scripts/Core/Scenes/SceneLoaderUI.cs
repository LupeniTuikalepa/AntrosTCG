using PrimeTween;
using UnityEngine;

namespace ATCG.Scenes
{
    internal class SceneLoaderUI : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup canvasGroup;
        [SerializeField]
        private GameObject root;

        [SerializeField]
        private float fadeDuration;
        
        internal async Awaitable OnPhaseBegin(LoadingScenePhase phase)
        {
            root.SetActive(true);
            if (!Mathf.Approximately(canvasGroup.alpha, 1))
            {
                Tween.StopAll(canvasGroup);
                await Tween.Alpha(canvasGroup, 1f, fadeDuration, Ease.OutExpo);
            }
        }

        internal async Awaitable OnPhaseEnd(LoadingScenePhase phase)
        {
            if (!Mathf.Approximately(canvasGroup.alpha, 0f))
            {
                Tween.StopAll(canvasGroup);
                await Tween.Alpha(canvasGroup, 0f, fadeDuration, Ease.OutExpo)
                    .OnComplete(() => root.SetActive(false));
            }
            else
            {
                root.SetActive(false);
            }
        }
    }
}