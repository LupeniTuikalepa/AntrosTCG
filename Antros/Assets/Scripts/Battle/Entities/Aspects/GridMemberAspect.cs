using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Tags;
using ATCG.HexGrids;

namespace ATCG.Battle.Entities.Aspects
{
    public readonly partial struct GridMemberAspect : IEntityAspect<GridMemberComponent>
    {

        public bool IsPhysical => EntityAddress.HasComponent<PhysicalCellMemberTag>();

        public bool PreventsDeployment => EntityAddress.HasComponents<PreventDeploymentTag>();

        public HexCoordinates Coordinates
        {
            get
            {
                if (EntityAddress.TryGetComponent(out ComponentRef<GridMemberComponent> componentRef))
                {
                    ref GridMemberComponent component = ref componentRef.GetValue();
                    return component.coordinates;
                }

                return HexCoordinates.None;
            }
            set
            {
                if (EntityAddress.TryGetComponent(out ComponentRef<GridMemberComponent> componentRef))
                {
                    ref GridMemberComponent component = ref componentRef.GetValue();
                    component.coordinates = value;
                }
            }
        }
    }
}