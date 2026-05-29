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

        void IRuntimeBattlePlayerComponent<IBattlePlayer>.Connect(RuntimeBattlePlayer runtimeBattlePlayer, IBattlePlayer player)
        {
            Player = player;
            Connect(Player.Hand);
        }

        void IRuntimeBattlePlayerComponent<IBattlePlayer>.Disconnect(RuntimeBattlePlayer runtimeBattlePlayer, IBattlePlayer player)
        {
            Disconnect();
            Player = null;
        }

        public override bool CanCardBeDragged(ICard card)
        {
            return false;
        }

        public override bool CanCardBeClicked(ICard card)
        {
            return false;
        }
    }
}