using System;
using ATCG.Battle.Cards;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Players;
using ATCG.HexGrids;
using UnityEngine;

namespace ATCG.Battle.Commands.GameCommands
{
    [Serializable]
    public class DeployCardCommand : GameCommand
    {
        [field: SerializeField]
        public int PlayerId { get; private set; }

        [field: SerializeField]
        public int CardId { get; private set; }

        [field: SerializeField]
        public HexCoordinates Destination { get; private set; }

        public DeployCardCommand(int cardId, HexCoordinates destination, int playerId)
        {
            CardId = cardId;
            Destination = destination;
            PlayerId = playerId;
        }

        protected override void Process(in GameCommandContext context)
        {
            IBattlePlayer player = context.GetBattlePlayer(PlayerId);
            IBattleCard card = player.Hand.GetCard(CardId);

            if (card.InvocationCost > player.CurrentMana)
                return;

            Embed(in context, new ModifyPlayerManaCommand(PlayerId, -card.InvocationCost));
            switch (card)
            {
                case HeroBattleCard heroBattleCard:
                    Embed(in context, new SpawnHeroCommand(player, heroBattleCard, Destination));
                    break;
            }
        }

    }
}