using Helteix.Cards.UI.Physical;
using Helteix.Cards.UI.Physical.Movers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ATCG.Battle.Cards.UI.Customs
{
    public class PlayerHandCardHolderUI : CardHolderUI
    {
        [SerializeField]
        private int selectedHeight;

        private float defaultHeight;

        protected override void OnSelect(BaseEventData baseEventData)
        {
            base.OnSelect(baseEventData);
            defaultHeight = defaultMover.positionOffset.y;
            defaultMover.positionOffset = new Vector2(defaultMover.positionOffset.x, selectedHeight);
        }

        protected override void OnDeselect(BaseEventData baseEventData)
        {
            base.OnDeselect(baseEventData);
            defaultMover.positionOffset = new Vector2(defaultMover.positionOffset.x, defaultHeight);
        }
    }
}