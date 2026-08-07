using System.Linq;
using ATCG.Battle.Cards;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Tags;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using ATCG.Capacities;
using ATCG.HexGrids;

namespace ATCG.Battle.Entities.Aspects
{
    public partial struct DeployableAspect :ICreateEntityAspect<DeployableAspect.Setup>,
        IEntityAspect<
        GridMemberComponent,
        PhysicalCellMemberTag,
        DeployableEntityTag,
        InspectableTag,
        BelongsToPlayerComponent,
        BattleIDOwner>
    {
        public struct Setup
        {
            public EntityAddress caster;
            public DeployableData data;
            public HexCoordinates coordinates;
            public BattleGrid grid;
            public BattleID battleID;
            public IBattlePlayer battlePlayer;
        }
        
        public HexCoordinates Coordinates => GridMemberComponent.coordinates;
        
        private static partial void CreateComponents(ref ComponentsFactory componentsFactory, Setup setup, EntityAddress address)
        {
            componentsFactory.GridMemberComponent = new GridMemberComponent(setup.grid, setup.coordinates);
            componentsFactory.DeployableEntityTag = new DeployableEntityTag(setup.caster, setup.data);
            
            componentsFactory.PhysicalCellMemberTag = new PhysicalCellMemberTag();
            componentsFactory.InspectableTag = new InspectableTag();
            componentsFactory.BattleIDOwner = new BattleIDOwner(setup.battleID);

            var player = setup.battlePlayer;
            componentsFactory.BelongsToPlayerComponent = new BelongsToPlayerComponent(player.GetBattleID(), player.GetPlayerNumber());
        }
    }
}