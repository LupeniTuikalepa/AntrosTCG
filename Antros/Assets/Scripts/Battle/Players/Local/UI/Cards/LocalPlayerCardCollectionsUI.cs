using ATCG.Battle.Cards.UI;
using UnityEngine;

namespace ATCG.Battle.Players.Local.UI.Cards
{
    [AddComponentMenu("ATCG/Gameplay/Player/UI/Player HeroCard Collection ")]
    public class LocalPlayerCardCollectionsUI : PlayerHUDElement
    {
        [SerializeField]
        private BattleDeckUI deckUI;

        [SerializeField]
        private BattleDiscardUI discardUI;

        protected override void OnConnect()
        {
            deckUI.Connect(Player.Deck);
            discardUI.Connect(Player.DeadCards);
        }

        protected override void OnDisconnect()
        {
            deckUI.Disconnect();
            discardUI.Disconnect();
        }
    }
}