using Helteix.Cards.UI.Physical;
using UnityEngine;

namespace ATCG.Battle.Players.Local.UI.Cards
{
    [AddComponentMenu("ATCG/Gameplay/Player/UI/Player Hand CardHolder")]
    public class PlayerHandCardHolderUI : CardHolderUI
    {
        [SerializeField]
        private int selectedHeight;

        private float defaultHeight;

        protected override void OnSelect()
        {
            base.OnSelect();
            defaultHeight = defaultMover.positionOffset.y;
            defaultMover.positionOffset = new Vector2(defaultMover.positionOffset.x, selectedHeight);
        }

        protected override void OnDeselect()
        {
            base.OnDeselect();
            defaultMover.positionOffset = new Vector2(defaultMover.positionOffset.x, defaultHeight);
        }
    }
}