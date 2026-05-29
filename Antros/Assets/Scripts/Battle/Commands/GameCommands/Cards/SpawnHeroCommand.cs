using ATCG.Battle.Cards;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players;
using ATCG.HexGrids;
using UnityEngine;

namespace ATCG.Battle.Commands.GameCommands
{
    public class SpawnHeroCommand : GameCommand
    {
        public IBattlePlayer Player { get; private set; }

        public HeroBattleCard Card { get; private set; }

        public HexCoordinates Destination { get; private set; }

        public EntityAddress DeployedEntity { get; private set; }

        public SpawnHeroCommand(IBattlePlayer player, HeroBattleCard heroBattleCard, HexCoordinates destination)
        {
            Player = player;
            Card = heroBattleCard;
            Destination = destination;
            DeployedEntity = EntityAddress.None;
        }


        protected override void Process(in GameCommandContext context)
        {
            HeroEntityAspect hero = HeroEntityAspect.CreateAspect(context.World, new HeroEntityAspect.Setup()
            {
                card = Card,
                coordinates = Destination,
                grid = context.Grid,
            });

            DeployedEntity = hero.EntityAddress;
            Embed(in context, new MoveCommand(DeployedEntity, Destination));
        }
    }
}