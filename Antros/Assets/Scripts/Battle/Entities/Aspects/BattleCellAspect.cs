using System;
using ATCG.Battle.Cards;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Tags;
using ATCG.Battle.Entities.Lookups;
using ATCG.Battle.Grids;
using ATCG.HexGrids;
using UnityEngine;

namespace ATCG.Battle.Entities.Aspects
{
    public readonly partial struct BattleCellAspect : ICreateEntityAspect<BattleCellAspect.Setup>,
        IEntityAspect<
            StatusReceiver,
            GridMemberComponent,
            BattleCellComponent,
            BattleIDOwner>
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

        public readonly struct IsCellPhysicalMemberFilter : IFilter<GridMemberComponent>
        {
            private readonly HexCoordinates coordinates;
            private readonly int cellEntityID;

            public IsCellPhysicalMemberFilter(HexCoordinates coordinates, int cellEntityID)
            {
                this.coordinates = coordinates;
                this.cellEntityID = cellEntityID;
            }

            public bool IsValid(in ComponentRef<GridMemberComponent> componentRef)
            {
                return componentRef.GetValue().coordinates == coordinates
                       && componentRef.entityID != cellEntityID
                       && componentRef.EntityAddress.HasComponent<PhysicalCellMemberTag>();
            }
        }
        public struct Setup
        {
            public HexCoordinates coordinates;
            public BattleGrid battleGrid;
            public BattleID battleID;
        }

        public HexCoordinates Coordinate => GridMemberComponent.coordinates;

        public bool HasMembers => GetMembers().Any();

        public ComponentQuery<GridMemberComponent, IsCellMemberFilter> GetMembers()
        {
            IsCellMemberFilter filter = new(GridMemberComponent.coordinates, EntityAddress.entity);
            return EntityAddress.world.Query<IsCellMemberFilter, GridMemberComponent>(filter);
        }
        public ComponentQuery<GridMemberComponent, IsCellPhysicalMemberFilter> GetPhysicalMembers()
        {
            IsCellPhysicalMemberFilter filter = new(GridMemberComponent.coordinates, EntityAddress.entity);
            return EntityAddress.world.Query<IsCellPhysicalMemberFilter, GridMemberComponent>(filter);
        }
        public bool CanBeMovedOn()
        {
            return !HasPhysicalMember();
        }

        public bool HasPhysicalMember()
        {
            foreach (ComponentRef<GridMemberComponent> member in GetMembers())
            {
                if (!member.EntityAddress.Is(out GridMemberAspect aspect))
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
                if (!member.EntityAddress.Is(out GridMemberAspect aspect))
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
                componentsFactory.GridMemberComponent = new GridMemberComponent(setup.battleGrid, setup.coordinates);
                componentsFactory.BattleCellComponent = new BattleCellComponent();
                componentsFactory.BattleIDOwner = new BattleIDOwner(setup.battleID);
                componentsFactory.StatusReceiver = new StatusReceiver(64);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

    }
}