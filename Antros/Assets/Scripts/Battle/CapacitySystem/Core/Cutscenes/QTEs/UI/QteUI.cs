using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.Entities.Runtime.Animations;
using ATCG.Metrics;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.QTEs.UI
{
    public class QteUI : MonoBehaviour
    {
        [SerializeField]
        private Image fill;
        [SerializeField]
        private Image criticalBar;

        [SerializeField]
        private CanvasGroup canvasGroup;


        private Qte current;

        private void Update()
        {
            if (current is { IsDone: false })
            {
                fill.fillAmount = (float)current.NormalizedTime;
                criticalBar.fillAmount = GameMetrics.Current.QTESuccessRange;
            }
        }

        public void Connect(Qte qte, IRuntimeEntity runtimeEntity, Camera uiCamera)
        {
            current = qte;
            if (runtimeEntity != null)
            {
                Vector3 worldPos = runtimeEntity.transform.position;
                if(qte.data.OverrideAnchor && runtimeEntity is IRuntimeEntityWithAnimator withAnimator)
                    worldPos = withAnimator.Animator.GetBoneTransform(qte.data.BoneAnchor).position;

                Vector3 screenPos = uiCamera.WorldToScreenPoint(worldPos) + (Vector3)qte.data.ScreenOffset;
                transform.position = screenPos;
            }

            transform.localScale = Vector3.one * 1.3f;
            canvasGroup.alpha = 0;

            Tween.Scale(transform, Vector3.one, .2f, Ease.InElastic);
            Tween.Alpha(canvasGroup, 1, .05f);
        }

        public void Disconnect()
        {
            current = null;
            Tween.Alpha(canvasGroup, 0, .25f)
                .OnComplete(() => Destroy(gameObject));
        }

        public void Resolve()
        {
            Sequence.Create()
                .Chain(Tween.PunchScale(transform, Vector3.one * .2f, 0.2f))
                .Chain(Tween.Alpha(canvasGroup, 0, 0.1f));
        }
    }
}