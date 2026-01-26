using UnityEngine;
using UnityEngine.UI;

namespace ATCG.Battle.Heroes.Deployed
{
    public class HeroCapacityButtonUI : MonoBehaviour, ILayoutElement
    {
        [SerializeField]
        private RectTransform target;
        [SerializeField]
        private RectTransform source;

        void ILayoutElement.CalculateLayoutInputHorizontal()
        {
        }

        void ILayoutElement.CalculateLayoutInputVertical()
        {
        }

        float ILayoutElement.minWidth => source.sizeDelta.x;

        float ILayoutElement.preferredWidth => source.sizeDelta.x;

        float ILayoutElement.flexibleWidth => 0;

        float ILayoutElement.minHeight => source.sizeDelta.y;

        float ILayoutElement.preferredHeight => source.sizeDelta.y;

        float ILayoutElement.flexibleHeight => 0;

        int ILayoutElement.layoutPriority => 1;
    }
}