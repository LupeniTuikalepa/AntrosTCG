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
    public readonly partial struct ConstructionAspect : ICreateEntityAspect<ConstructionAspect.Setup>,
        IEntityAspect<
        BattleCardComponent,
        StatusReceiver,
        PassiveContainerComponent,
        BelongsToPlayerComponent,
        GridMemberComponent,
        ConstructionTag,
        HealthComponent,
        DeathCostComponent,
        PhysicalCellMemberTag,
        DefenseComponent,
        DeployTargetComponent,
        BattleIDOwner>
    {
        public struct Setup
        {
            public GameObject prefab;
            public ConstructionBattleCard card;
            public HexCoordinates coordinates;
            public BattleGrid grid;
            public BattleID battleID;
        }
        
        public string Name => ConstructionCard.Title;
        public IBattlePlayer Player => ConstructionCard.Player;
        public ConstructionBattleCard ConstructionCard => BattleCardComponent.battleCard as ConstructionBattleCard;
        public HexCoordinates Coordinates => GridMemberComponent.coordinates;
        public IBattleCard Card => BattleCardComponent.battleCard;

        private static partial void CreateComponents(ref ComponentsFactory componentsFactory, Setup setup, EntityAddress address)
        {
            IBattlePlayer battlePlayer = setup.card.Player;

            componentsFactory.BattleCardComponent = new BattleCardComponent(setup.card);
            componentsFactory.BelongsToPlayerComponent = new BelongsToPlayerComponent(battlePlayer.GetBattleID(), battlePlayer.GetPlayerNumber());

            componentsFactory.HealthComponent = new HealthComponent(setup.card.MaxHealth);
            componentsFactory.DefenseComponent = new DefenseComponent(setup.card.Defense);

            componentsFactory.GridMemberComponent = new GridMemberComponent(setup.grid, setup.coordinates);
            componentsFactory.DeployTargetComponent = new DeployTargetComponent( setup.card.DeployPatterns);

            componentsFactory.StatusReceiver = new StatusReceiver(64);
            componentsFactory.PassiveContainerComponent = new PassiveContainerComponent();
            componentsFactory.DeathCostComponent = new DeathCostComponent(setup.card.DeathCost);
            //Heroes block pathfinding, Ray Casting and such
            componentsFactory.PhysicalCellMemberTag = new PhysicalCellMemberTag();
            componentsFactory.BattleIDOwner = new BattleIDOwner(setup.battleID);
            
            componentsFactory.ConstructionTag = new ConstructionTag();
        }
    }
}