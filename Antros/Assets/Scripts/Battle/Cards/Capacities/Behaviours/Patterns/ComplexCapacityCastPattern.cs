using System.Collections.Generic;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Patterns;
using ATCG.Capacities.Data;
using ATCG.HexGrids;
using UnityEngine.Pool;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Patterns
{
    public abstract class ComplexCapacityCastPattern<TData, TPattern> : ICapacityCastPattern<TData>
        where TPattern : ICellPattern
        where TData : ICapacityCastPatternData
    {
        public void FillTargetedCells(TData castData, in Capacity context, HashSet<BattleCellAspect> targetedCells)
        {
            using (ListPool<HexCoordinates>.Get(out List<HexCoordinates> results))
            {
                BattleGrid battleGrid = context.grid;

                TPattern pattern = GetCellPattern(castData, context);
                Capacity capacity = context;
                pattern.ValidateCell = ctx =>
                {
                    if (battleGrid.TryGetBattleCell(ctx, out BattleCellAspect cell))
                        return ValidateCell(capacity, cell);

                    return false;
                };

                pattern.Evaluate(results);
                foreach (HexCoordinates hexCoordinates in results)
                {
                    if (!battleGrid.TryGetBattleCell(hexCoordinates, out BattleCellAspect battleCell))
                        continue;

                    targetedCells.Add(battleCell);
                }
            }
        }

        protected abstract TPattern GetCellPattern(TData castData, Capacity context);

        protected virtual bool ValidateCell(Capacity context, BattleCellAspect battleCellAspect)
        {
            return battleCellAspect.CanBeAttacked(context.card);
        }
    }
}