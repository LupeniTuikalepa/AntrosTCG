using Helteix.Cards.UI.Physical;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ATCG.Battle.Players.Local.UI.Cards
{
    [AddComponentMenu("ATCG/Gameplay/Player/UI/Player Hand CardHolder")]
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