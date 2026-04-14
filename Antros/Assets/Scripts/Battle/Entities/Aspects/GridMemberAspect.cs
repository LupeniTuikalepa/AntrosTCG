using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Tags;
using ATCG.HexGrids;

namespace ATCG.Battle.Entities.Aspects
{
    public struct GridMemberAspect : IEntityAspect<GridMemberComponent>
    {
        public EntityAddress EntityAddress { get; set; }

        public ref GridMemberComponent GridMemberComponent => ref EntityAddress.GetComponent<GridMemberComponent>();

        public bool IsPhysical => EntityAddress.HasComponent<PhysicalCellMemberTag>();

        public bool PreventsDeployment => EntityAddress.HasComponents<PreventDeploymentTag>();


        public HexCoordinates Coordinates
        {
            get
            {
                if (EntityAddress.TryGetComponent(out ComponentRef<GridMemberComponent> componentRef))
                {
                    ref GridMemberComponent component = ref componentRef.GetValue();
                    return component.Coordinates;
                }

                return HexCoordinates.None;
            }
            set
            {
                if (EntityAddress.TryGetComponent(out ComponentRef<GridMemberComponent> componentRef))
                {
                    ref GridMemberComponent component = ref componentRef.GetValue();
                    component.SetCoordinates(value);
                }
            }
        }
    }
}