using System;
using ATCG.Battle.Cards;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
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

        public override void Process(in GameCommandContext context)
        {
            IBattlePlayer player = context.GetBattlePlayer(PlayerId);
            IBattleCard card = player.Hand.GetCard(CardId);

            if (card.InvocationCost > player.CurrentMana)
                return;

            Embed(in context, new ModifyPlayerManaCommand(PlayerId, -card.InvocationCost));
            switch (card)
            {
                case HeroBattleCard heroBattleCard:

                    using (EntityAspectBuilder<HeroEntityAspect> builder = new())
                    {
                        builder.Add(new ComponentFactory<BattleCardComponent>(() => new BattleCardComponent(heroBattleCard)));
                        builder.Add(new ComponentFactory<HeroComponent>(() => new HeroComponent(heroBattleCard)));

                        HeroEntityAspect hero = builder.CreateAndDispose(context.World);

                        Embed(in context, new MoveCommand(hero.ToEntity(), Destination));
                        break;
                    }
            }
        }

    }
}