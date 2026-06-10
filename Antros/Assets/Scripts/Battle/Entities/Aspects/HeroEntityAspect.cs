using System.Linq;
using ATCG.Battle.Cards;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using ATCG.Cards.Implementations;
using ATCG.HexGrids;
using ATCG.HexGrids.Grids;

namespace ATCG.Battle.Entities.Aspects
{
    public partial struct HeroEntityAspect : ICreateEntityAspect<HeroEntityAspect.Setup>,
        IEntityAspect<BattleCardComponent,
            BelongsToPlayerComponent,
            BattleGridElementComponent,
            MovementComponent,
            CapacityCasterComponent,
            BasicAttackerComponent>
    {
        public struct Setup
        {
            public HeroBattleCard card;
            public BattleGrid grid;
            public HexCoordinates coordinates;
        }

        public string Name => HeroCard.Title;
        public IBattlePlayer Player => HeroCard.Player;
        public HeroBattleCard HeroCard => BattleCardComponent.battleCard as HeroBattleCard;
        public HexCoordinates Coordinates => BattleGridElementComponent.coordinates;
        public IBattleCard Card => BattleCardComponent.battleCard;

        private static partial void CreateComponents(ref ComponentsFactory componentsFactory, Setup setup)
        {
            componentsFactory.MovementComponent = new MovementComponent(setup.card.Speed);
            componentsFactory.BattleCardComponent = new BattleCardComponent(setup.card);
            componentsFactory.BelongsToPlayerComponent = new BelongsToPlayerComponent(setup.card.Player.GetPlayerID());
            componentsFactory.BattleGridElementComponent = new BattleGridElementComponent(setup.grid, setup.coordinates);
            componentsFactory.BasicAttackerComponent = new BasicAttackerComponent(setup.card.Strength);

            //TODO
            componentsFactory.CapacityCasterComponent = new CapacityCasterComponent(setup.card.CapacitiesData.ToArray());
        }
    }
}