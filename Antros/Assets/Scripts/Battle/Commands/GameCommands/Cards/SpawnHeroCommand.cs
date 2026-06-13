using ATCG.Battle.Cards;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Players;
using ATCG.HexGrids;

namespace ATCG.Battle.Commands.GameCommands
{
    public class SpawnHeroCommand : GameCommand<SpawnHeroCommand.Infos>
    {
        public struct Infos
        {
            public Entity spawnedEntity;
            public int cardID;
        }

        public IBattlePlayer Player { get; private set; }

        public HeroBattleCard Card { get; private set; }

        public HexCoordinates Destination { get; private set; }

        public SpawnHeroCommand(IBattlePlayer player, HeroBattleCard heroBattleCard, HexCoordinates destination)
        {
            Player = player;
            Card = heroBattleCard;
            Destination = destination;

        }


        protected override void Process(in GameCommandContext context)
        {
            HeroEntityAspect hero = HeroEntityAspect.CreateAspect(context.World, new HeroEntityAspect.Setup()
            {
                card = Card,
                coordinates = Destination,
                grid = context.Grid,
            });

            infos.cardID = Player.Hand.GetCardIndex(Card);
            infos.spawnedEntity = hero.EntityAddress.entity;
            
            Embed(in context, new MoveCommand(hero.EntityAddress, Destination));
        }
    }
}