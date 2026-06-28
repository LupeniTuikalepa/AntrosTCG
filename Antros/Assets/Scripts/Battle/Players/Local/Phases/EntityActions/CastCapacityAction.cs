using ATCG.Battle.Capacities;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Capacities;
using ATCG.Capacities.Data;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle
{
    public class CastCapacityAction : EntityAction
    {
        public override int ManaCost => capacityData.Cost;

        public readonly CapacityData capacityData;
        private readonly HexCoordinates from;

        public CastCapacityAction(LocalBattlePlayer fromPlayer, CapacityData capacityData, HexCoordinates from) : base(fromPlayer)
        {
            this.capacityData = capacityData;

            this.from = from;
        }

        //TODO pour les spells, le cast des capacites ne se fera pas depuis une action donc il faudra sortir la logique et la rendre commune dans le capacity manager.

        public override async Awaitable Execute(EntityAddress address, BattlePhase battlePhase)
        {
            var patterns = capacityData.CastPatterns;

            //If no pattern, use the entity position
            if (patterns.IsEmpty)
            {
                if (address.TryGetComponentRO(out GridMemberComponent component))
                    await ExecuteCommand(battlePhase, component.coordinates);
            }
            else
            {

                BattlePatternController patternController = new BattlePatternController(BattleGrid);
                using var patternBuilder = new HexPatternBuilder(from, patternController)
                    .With(capacityData.CastPatterns);

                AspectFilter<BattleCellAspect> filter = new AspectFilter<BattleCellAspect>();
                SelectEntityPhase<AspectFilter<BattleCellAspect> > phase = new SelectEntityPhase<AspectFilter<BattleCellAspect>>(fromPlayer, filter, patternBuilder);

                EntityAddress[] result = await phase;

                for (int i = 0; i < result.Length; i++)
                {
                    EntityAddress target = result[i];
                    if (target.TryGetComponentRO(out GridMemberComponent component))
                        await ExecuteCommand(battlePhase, component.coordinates);
                }
            }
        }

        private async Awaitable ExecuteCommand(BattlePhase battlePhase, HexCoordinates source)
        {
            CapacitySetup setup = new CapacitySetup(capacityData, source, battlePhase);
            await CapacityManager.CastCapacityAsync(capacityData, setup);
        }
    }
}