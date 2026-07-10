using System;
using ATCG.Battle.Cards;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players;
using ATCG.HexGrids;
using UnityEngine;

namespace ATCG.Battle.Commands.GameCommands
{
    [Serializable]
    public class DeployCardCommand : PlayerCommand<NoInfos>
    {
        [field: SerializeField]
        public BattleID CardId { get; private set; }

        [field: SerializeField]
        public HexCoordinates Destination { get; private set; }

        public DeployCardCommand(IBattleCard card, HexCoordinates destination, IBattlePlayer player)  : base(player)
        {
            //TODO use something better, card could be shuffled
            CardId = card.ID;
            Destination = destination;
        }

        protected override void Process(in CommandContext context)
        {
            IBattlePlayer player = GetPlayer(in context);

            if(!player.TryGetCard(CardId, out IBattleCard card))
                return;

            if (card.InvocationCost > player.CurrentMana)
                return;

            Inject(in context, new ModifyPlayerManaCommand(player, -card.InvocationCost));
            switch (card)
            {
                case HeroBattleCard heroBattleCard:
                    Inject(in context, new SpawnHeroCommand(player, heroBattleCard, Destination));
                    player.Hand.TryRemoveCard(card);
                    break;
            }
        }

    }
}