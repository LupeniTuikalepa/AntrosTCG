using System;
using ATCG.Battle.Cards;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Lookups;
using ATCG.Battle.Grids;
using ATCG.HexGrids;
using UnityEngine;

namespace ATCG.Battle.Entities.Aspects
{
    public readonly partial struct BattleCellAspect : IEntityAspect<GridMemberComponent>,
        ICreateEntityAspect<BattleCellAspect.Setup>
    {
        public readonly struct IsCellMemberFilter : IFilter<GridMemberComponent>
        {
            private readonly HexCoordinates coordinates;
            private readonly int cellEntityID;

            public IsCellMemberFilter(HexCoordinates coordinates, int cellEntityID)
            {
                this.coordinates = coordinates;
                this.cellEntityID = cellEntityID;
            }

            public bool IsValid(in ComponentRef<GridMemberComponent> componentRef)
            {
                return componentRef.GetValue().coordinates == coordinates && componentRef.entityID != cellEntityID;
            }
        }

        public struct Setup
        {
            public HexCoordinates coordinates;
            public BattleGrid battleGrid;
        }

        public HexCoordinates Coordinate => GridMemberComponent.coordinates;


        public ComponentQuery<GridMemberComponent, IsCellMemberFilter> GetMembers()
        {
            IsCellMemberFilter filter = new(GridMemberComponent.coordinates, EntityAddress.entity);
            return EntityAddress.world.Query<IsCellMemberFilter, GridMemberComponent>(filter);
        }


        public bool HasPhysicalMember()
        {
            foreach (ComponentRef<GridMemberComponent> member in GetMembers())
            {
                if (!member.EntityAddress.IsGridMemberAspect(out GridMemberAspect aspect))
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
                if (!member.EntityAddress.IsGridMemberAspect(out GridMemberAspect aspect))
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

        private static partial void CreateComponents(ref ComponentsFactory componentsFactory, Setup setup)
        {
            try
            {
                GridMemberComponent elementComponent = new GridMemberComponent(setup.battleGrid, setup.coordinates);
                componentsFactory.GridMemberComponent = elementComponent;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}