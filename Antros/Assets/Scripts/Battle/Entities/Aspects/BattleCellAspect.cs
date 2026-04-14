using ATCG.Battle.Cards;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Lookups;
using ATCG.HexGrids;

namespace ATCG.Battle.Entities.Aspects
{
    public struct BattleCellAspect : IEntityAspect<BattleCellComponent>
    {
        public readonly struct IsCellMemberFilter : IComponentLookupFilter<GridMemberComponent>
        {
            private readonly HexCoordinates coordinates;

            public IsCellMemberFilter(HexCoordinates coordinates)
            {
                this.coordinates = coordinates;
            }

            public bool IsValid(in ComponentRef<GridMemberComponent> componentRef)
            {
                return componentRef.GetValue().Coordinates == coordinates;
            }
        }

        public EntityAddress EntityAddress { get; set; }

        public HexCoordinates Coordinate => BattleCellComponent.coordinates;

        public ref BattleCellComponent BattleCellComponent => ref EntityAddress.GetComponent<BattleCellComponent>();


        public ComponentsLookupResult<GridMemberComponent, IsCellMemberFilter> GetMembers()
        {
            IsCellMemberFilter filter = new(BattleCellComponent.coordinates);
            return EntityAddress.world.LookupComponents<IsCellMemberFilter, GridMemberComponent>(filter);
        }


        public bool HasPhysicalMember()
        {
            foreach (ComponentRef<GridMemberComponent> member in GetMembers())
            {
                if (!member.Entity.TryConvertToAspect(member.world, out GridMemberAspect aspect))
                    continue;

                if (aspect.IsPhysical)
                    return true;
            }

            return false;
        }

        public bool CanBeDeployedOn(IBattleCard card)
        {
            foreach (ComponentRef<GridMemberComponent> member in GetMembers())
            {
                if (!member.Entity.TryConvertToAspect(member.world, out GridMemberAspect aspect))
                    continue;

                if (aspect.IsPhysical || aspect.PreventsDeployment)
                    return false;
            }

            return true;
        }

        public bool CanBeAttacked(IBattleCard contextCard)
        {
            return true;
        }
    }
}