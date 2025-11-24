using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG.UI
{
    public class RowUI : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup group;

        private float animationDuration = .25f;


        private VerticalLayoutGroup layoutGroup;

        private void Awake()
        {
            layoutGroup = GetComponentInParent<VerticalLayoutGroup>();
        }

        public void SetVisibility(bool visible)
        {
            if(visible)
                Show();
            else
                Hide();
        }

        public void Show()
        {
            Tween.StopAll(group);
            Tween.StopAll(transform);

            Tween.Alpha(group, 1, animationDuration * .5f);
            Tween.ScaleY(transform, 1, animationDuration).OnUpdate(transform, (ctx, tween) =>
            {
                if (layoutGroup != null)
                {
                    layoutGroup.SetLayoutVertical();
                }
            });
            group.blocksRaycasts = true;
            group.interactable = true;
        }

        public void Hide()
        {
            Tween.StopAll(group);
            Tween.StopAll(transform);

            Tween.Alpha(group, 0, animationDuration * .5f, startDelay: animationDuration * .25f);
            Tween.ScaleY(transform, 0, animationDuration).OnUpdate(transform, (ctx, tween) =>
            {
                if (layoutGroup != null)
                {
                    layoutGroup.SetLayoutVertical();
                }
            });
            group.blocksRaycasts = false;
            group.interactable = false;
        }
    }
}