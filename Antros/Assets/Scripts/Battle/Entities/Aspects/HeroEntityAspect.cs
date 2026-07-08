using System.Linq;
using ATCG.Battle.Cards;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Tags;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using ATCG.HexGrids;

namespace ATCG.Battle.Entities.Aspects
{
    public readonly partial struct HeroEntityAspect : ICreateEntityAspect<HeroEntityAspect.Setup>,
        IEntityAspect<BattleCardComponent,
            BelongsToPlayerComponent,
            HealthComponent,
            MovementComponent,
            CapacityCasterComponent,
            BasicAttackerComponent,
            GridMemberComponent,
            DeathCostComponent,
            BattleIDOwner,
			DeployTargetComponent,
            PhysicalCellMemberTag,
			DefenseComponent>
    {
        public struct Setup
        {
            public HeroBattleCard card;
            public BattleGrid grid;
            public HexCoordinates coordinates;
            public BattleID battleID;
        }

        public string Name => HeroCard.Title;
        public IBattlePlayer Player => HeroCard.Player;
        public HeroBattleCard HeroCard => BattleCardComponent.battleCard as HeroBattleCard;
        public HexCoordinates Coordinates => GridMemberComponent.coordinates;
        public IBattleCard Card => BattleCardComponent.battleCard;

        private static partial void CreateComponents(ref ComponentsFactory componentsFactory, Setup setup)
        {
            IBattlePlayer battlePlayer = setup.card.Player;

            componentsFactory.BattleCardComponent = new BattleCardComponent(setup.card);
            componentsFactory.BelongsToPlayerComponent = new BelongsToPlayerComponent(battlePlayer.GetBattleID(), battlePlayer.GetPlayerNumber());

            componentsFactory.HealthComponent = new HealthComponent(setup.card.MaxHealth);
            componentsFactory.DefenseComponent = new DefenseComponent(setup.card.Defense);
            componentsFactory.MovementComponent = new MovementComponent(setup.card.Speed, setup.card.MovementPatterns, setup.card.MovementType);

            componentsFactory.CapacityCasterComponent = new CapacityCasterComponent(setup.card.CapacitiesData.ToArray());
            componentsFactory.BasicAttackerComponent = new BasicAttackerComponent(setup.card.Strength);
            componentsFactory.GridMemberComponent = new GridMemberComponent(setup.grid, setup.coordinates);
            componentsFactory.DeployTargetComponent = new DeployTargetComponent( setup.card.DeployPatterns);

            componentsFactory.DeathCostComponent = new DeathCostComponent(setup.card.DeathCost);
            //Heroes block pathfinding, Ray Casting and such
            componentsFactory.PhysicalCellMemberTag = new PhysicalCellMemberTag();
            componentsFactory.BattleIDOwner = new BattleIDOwner(setup.battleID);
        }
    }
}