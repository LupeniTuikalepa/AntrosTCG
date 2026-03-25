using System.Collections.Generic;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Patterns;
using ATCG.Capacities.Data;
using ATCG.Cards;
using ATCG.HexGrids;
using UnityEngine.Pool;

namespace ATCG.Battle.Cards.Capacities.Patterns
{
    public abstract class ComplexCapacityCastPattern<TData, TPattern> : ICapacityCastPattern<TData>
        where TPattern : ICellPattern
        where TData : ICapacityCastPatternData
    {
        protected abstract TPattern GetCellPattern(TData castData, Capacity context);

        public void FillTargetedCells(TData castData, in Capacity context, HashSet<BattleCell> targetedCells)
        {
            using (ListPool<HexCoordinates>.Get(out List<HexCoordinates> results))
            {
                BattleGrid battleGrid = context.battleGrid;

                TPattern pattern = GetCellPattern(castData, context);
                Capacity capacity = context;
                pattern.ValidateCell = ctx =>
                {
                    if(battleGrid.TryGetBattleCell(ctx, out BattleCell cell))
                        return ValidateCell(capacity, cell);

                    return false;
                };

                pattern.Evaluate(results);
                foreach (HexCoordinates hexCoordinates in results)
                {
                    if (!battleGrid.TryGetBattleCell(hexCoordinates, out  BattleCell battleCell))
                        continue;

                    targetedCells.Add(battleCell);
                }
            }
        }

        protected virtual bool ValidateCell(Capacity context, BattleCell battleCell)
        {
            return battleCell.CanBeAttacked(context.card);
        }
    }
}