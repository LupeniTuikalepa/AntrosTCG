using ATCG.Battle.Players;
using ATCG.Battle.Players.Runtime;
using Helteix.Cards;
using Helteix.Cards.UI.Physical;
using UnityEngine;

namespace ATCG.Battle.Cards.UI
{
    [AddComponentMenu("ATCG/Gameplay/Cards/Battle Hand")]
    public class BattleHandUI : PhysicalCardCollectionUI<IBattleCard>, IRuntimeBattlePlayerComponent<IBattlePlayer>
    {
        public IBattlePlayer Player { get; private set; }

        public void Connect(RuntimeBattlePlayer runtimeBattlePlayer, IBattlePlayer player)
        {
            Player = player;
            Connect(Player.Hand);
        }

        public void Disconnect(RuntimeBattlePlayer runtimeBattlePlayer, IBattlePlayer battlePlayer)
        {
            Disconnect();
            Player = null;
        }

        protected override bool CanCardBeDragged(ICard card)
        {
            return false;
        }

        protected override bool CanCardBeClicked(ICard card)
        {
            return false;
        }
    }
}