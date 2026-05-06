using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Tags;
using ATCG.HexGrids;

namespace ATCG.Battle.Entities.Aspects
{
    public partial struct GridMemberAspect : IEntityAspect<BattleGridElementComponent>
    {

        public bool IsPhysical => EntityAddress.HasComponent<PhysicalCellMemberTag>();

        public bool PreventsDeployment => EntityAddress.HasComponents<PreventDeploymentTag>();

        public HexCoordinates Coordinates
        {
            get
            {
                if (EntityAddress.TryGetComponent(out ComponentRef<BattleGridElementComponent> componentRef))
                {
                    ref BattleGridElementComponent component = ref componentRef.GetValue();
                    return component.coordinates;
                }

                return HexCoordinates.None;
            }
            set
            {
                if (EntityAddress.TryGetComponent(out ComponentRef<BattleGridElementComponent> componentRef))
                {
                    ref BattleGridElementComponent component = ref componentRef.GetValue();
                    component.coordinates = value;
                }
            }
        }
    }
}