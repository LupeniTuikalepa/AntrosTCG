using System.Collections;
using ATCG.Battle.Entities.Heroes.Runtime;
using PrimeTween;
using UnityEngine;

namespace ATCG.Battle.Entities.Heroes.UI
{
    public class HeroUIPanel : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private float animationDuration = .3f;

        [SerializeField]
        private float xOffset = 25;

        private Vector3 localPosition;

        private IHeroUIPanelElement[] panelElements;
        private RuntimeHero runtimeHero;

        private void Awake()
        {
            localPosition = transform.localPosition;
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        private IEnumerator Start()
        {
            runtimeHero = GetComponentInParent<RuntimeHero>();
            yield return new WaitForEndOfFrame();
            panelElements = GetComponentsInChildren<IHeroUIPanelElement>();
        }

        public void Open()
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            Tween.StopAll(canvasGroup);
            Tween.StopAll(transform);

            transform.localPosition = localPosition + Vector3.right * xOffset;

            Tween.Alpha(canvasGroup, 1, animationDuration);
            Tween.LocalPosition(transform, localPosition, animationDuration);
            for (int i = 0; i < panelElements.Length; i++)
                if (panelElements[i] != null)
                    panelElements[i].OnOpen(runtimeHero, this);
        }

        public void Close()
        {
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            Tween.StopAll(canvasGroup);
            Tween.StopAll(transform);


            Tween.Alpha(canvasGroup, 0, animationDuration);
            Tween.LocalPosition(transform, localPosition + Vector3.right * xOffset, animationDuration)
                .OnComplete(() => transform.localPosition = localPosition);

            for (int i = 0; i < panelElements.Length; i++)
                if (panelElements[i] != null)
                    panelElements[i]?.OnClose(runtimeHero, this);
        }
    }
}