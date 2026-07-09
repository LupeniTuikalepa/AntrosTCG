using System.Linq;
using ATCG.Battle.Cards;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Tags;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using ATCG.HexGrids;

namespace ATCG.Battle.Entities.Aspects
{
    public readonly partial struct DeployableAspect :ICreateEntityAspect<DeployableAspect.Setup>,
        IEntityAspect<
        GridMemberComponent,
        PhysicalCellMemberTag,
        DeployableEntityTag,
        BattleIDOwner>
    {
        public struct Setup
        {
            public HexCoordinates coordinates;
            public BattleGrid grid;
            public BattleID battleID;
        }
        
        public HexCoordinates Coordinates => GridMemberComponent.coordinates;

        private static partial void CreateComponents(ref ComponentsFactory componentsFactory, Setup setup)
        {
            
            componentsFactory.GridMemberComponent = new GridMemberComponent(setup.grid, setup.coordinates);
            componentsFactory.DeployableEntityTag = new DeployableEntityTag();
            
            componentsFactory.PhysicalCellMemberTag = new PhysicalCellMemberTag();
            componentsFactory.BattleIDOwner = new BattleIDOwner(setup.battleID);
        }
    }
}