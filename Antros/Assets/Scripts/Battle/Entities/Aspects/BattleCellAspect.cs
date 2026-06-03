using System;
using ATCG.Battle.Cards;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Lookups;
using ATCG.Battle.Grids;
using ATCG.HexGrids;
using UnityEngine;

namespace ATCG.Battle.Entities.Aspects
{
    public readonly partial struct BattleCellAspect : IEntityAspect<BattleGridElementComponent>,
        ICreateEntityAspect<BattleCellAspect.Setup>
    {
        public readonly struct IsCellMemberFilter : IFilter<BattleGridElementComponent>
        {
            private readonly HexCoordinates coordinates;
            private readonly int cellEntityID;

            public IsCellMemberFilter(HexCoordinates coordinates, int cellEntityID)
            {
                this.coordinates = coordinates;
                this.cellEntityID = cellEntityID;
            }

            public bool IsValid(in ComponentRef<BattleGridElementComponent> componentRef)
            {
                return componentRef.GetValue().coordinates == coordinates && componentRef.entityID != cellEntityID;
            }
        }

        public struct Setup
        {
            public HexCoordinates coordinates;
            public BattleGrid battleGrid;
        }

        public HexCoordinates Coordinate => BattleGridElementComponent.coordinates;


        public ComponentQuery<BattleGridElementComponent, IsCellMemberFilter> GetMembers()
        {
            IsCellMemberFilter filter = new(BattleGridElementComponent.coordinates, EntityAddress.entity);
            return EntityAddress.world.Query<IsCellMemberFilter, BattleGridElementComponent>(filter);
        }


        public bool HasPhysicalMember()
        {
            foreach (ComponentRef<BattleGridElementComponent> member in GetMembers())
            {
                if (!member.Address.IsGridMemberAspect(out GridMemberAspect aspect))
                    continue;

                if (aspect.IsPhysical)
                    return true;
            }

            return false;
        }

        public bool CanBeDeployedOn(IBattleCard card)
        {
            foreach (ComponentRef<BattleGridElementComponent> member in GetMembers())
            {
                if (!member.Address.IsGridMemberAspect(out GridMemberAspect aspect))
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
                BattleGridElementComponent elementComponent = new BattleGridElementComponent(setup.battleGrid, setup.coordinates);
                componentsFactory.BattleGridElementComponent = elementComponent;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}