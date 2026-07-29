using System.Linq;
using ATCG.Battle.Cards;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Tags;
using ATCG.Battle.Grids;
using ATCG.Battle.PassiveSystem.Core;
using ATCG.Battle.Players;
using ATCG.HexGrids;
using UnityEngine;

namespace ATCG.Battle.Entities.Aspects
{
    public readonly partial struct HeroEntityAspect : ICreateEntityAspect<HeroEntityAspect.Setup>,
        IEntityAspect<BattleCardComponent,
            StatusReceiver,
            PassiveContainerComponent,
            BelongsToPlayerComponent,
            HealthComponent,
            MovementComponent,
            PathfindingAgentComponent,
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

        private static partial void CreateComponents(ref ComponentsFactory componentsFactory, Setup setup, EntityAddress address)
        {
            IBattlePlayer battlePlayer = setup.card.Player;

            componentsFactory.BattleCardComponent = new BattleCardComponent(setup.card);
            componentsFactory.BelongsToPlayerComponent = new BelongsToPlayerComponent(battlePlayer.GetBattleID(), battlePlayer.GetPlayerNumber());

            componentsFactory.HealthComponent = new HealthComponent(setup.card.MaxHealth);
            componentsFactory.DefenseComponent = new DefenseComponent(setup.card.Defense);
            componentsFactory.MovementComponent = new MovementComponent(setup.card.Speed, setup.card.MovementType);
            // Traversability is fully defined by the rules passed here — CellOccupancyRule
            // blocks tiles held by other units (and lets the unit pass back through its own).
            componentsFactory.PathfindingAgentComponent = new PathfindingAgentComponent(
                setup.card.MovementType.ToAgentMovementType(),
                new CellOccupancyRule());

            componentsFactory.CapacityCasterComponent = new CapacityCasterComponent(setup.card.Capacities.ToArray());
            componentsFactory.BasicAttackerComponent = new BasicAttackerComponent(setup.card.Strength);
            componentsFactory.GridMemberComponent = new GridMemberComponent(setup.grid, setup.coordinates);
            componentsFactory.DeployTargetComponent = new DeployTargetComponent( setup.card.DeployPatterns);
            componentsFactory.StatusReceiver = new StatusReceiver(64);

            var passiveContainerComponent = new PassiveContainerComponent(8);
            foreach (var passiveData in setup.card.Data.Passives)
            {
                var passiveContext = new PassiveContext(address, setup.grid.battlePhase, passiveData);
                passiveContainerComponent.AddPassive(passiveData, passiveContext);
            }
            componentsFactory.PassiveContainerComponent = passiveContainerComponent;


            componentsFactory.DeathCostComponent = new DeathCostComponent(setup.card.DeathCost);
            //Heroes block pathfinding, Ray Casting and such
            componentsFactory.PhysicalCellMemberTag = new PhysicalCellMemberTag();
            componentsFactory.BattleIDOwner = new BattleIDOwner(setup.battleID);
        }
    }
}