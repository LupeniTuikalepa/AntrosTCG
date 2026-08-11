using Helteix.Tools.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG.Elements.UI
{
    public class ElementUI : UIItem<ElementData>
    {
        [SerializeField]
        private Image image;


        public void Setup(Element element)
        {
            if (element.TryGetData(out ElementData data))
                SyncUI(data);

        }

        protected override void SyncUI(ElementData current)
        {
            image.sprite = current.Icon;
            image.color = current.Color;
        }

        protected override void ClearUI()
        {
        }
    }
}