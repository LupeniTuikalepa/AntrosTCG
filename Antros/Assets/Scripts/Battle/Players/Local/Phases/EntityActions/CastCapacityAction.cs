using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Patterns.Building;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Capacities;
using ATCG.Capacities.Data;
using ATCG.HexGrids;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle
{
    public class CastCapacityAction : IEntityAction
    {
        public int ManaCost => capacityData.Cost;

        public readonly CapacityData capacityData;
        private readonly HexCoordinates from;

        public CastCapacityAction(CapacityData capacityData, HexCoordinates from)
        {
            this.capacityData = capacityData;
            this.from = from;
        }

        //TODO pour les spells, le cast des capacites ne se fera pas depuis une action donc il faudra sortir la logique et la rendre commune dans le capacity manager.

        public async Awaitable Execute(EntityAddress address, BattlePhase battlePhase)
        {
            CapacityPatternData[] patterns = capacityData.CastPatterns;

            //If no pattern, use the entity position
            if (patterns.Length == 0)
            {
                if (address.TryGetComponentRO(out GridMemberComponent component))
                    await ExecuteCommand(battlePhase, component.coordinates);
            }
            else
            {
                using HexPatternBuilder patternBuilder = capacityData.CastPatterns.ToPatternBuilder(from);

                HexPatternFilters filter = new HexPatternFilters(patternBuilder);

                SelectEntityPhase<HexPatternFilters> phase = new SelectEntityPhase<HexPatternFilters>(filter);

                EntityAddress[] result = await phase;

                for (int i = 0; i < result.Length; i++)
                {
                    EntityAddress target = result[i];
                    if (target.TryGetComponentRO(out GridMemberComponent component))
                        await ExecuteCommand(battlePhase, component.coordinates);

                }
            }
        }

        private async Awaitable ExecuteCommand(BattlePhase battlePhase, HexCoordinates from)
        {
            CapacityContext context = new CapacityContext(capacityData, from);
            CastCapacityCommand command = new CastCapacityCommand(in context);

            await command.Run(battlePhase);
        }
    }
}