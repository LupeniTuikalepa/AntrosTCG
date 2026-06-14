using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Patterns.Building;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Capacities;
using ATCG.Capacities.Data;
using ATCG.HexGrids;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle
{
    public class CastCapacityAction : EntityAction
    {
        public override int ManaCost => capacityData.Cost;

        public readonly CapacityData capacityData;
        private readonly HexCoordinates from;

        public CastCapacityAction(LocalBattlePlayer playerOrigin, CapacityData capacityData, HexCoordinates from) : base(playerOrigin)
        {
            this.capacityData = capacityData;
            this.from = from;
        }

        //TODO pour les spells, le cast des capacites ne se fera pas depuis une action donc il faudra sortir la logique et la rendre commune dans le capacity manager.

        public override async Awaitable Execute(EntityAddress address, BattlePhase battlePhase)
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

                SelectEntityPhase<HexPatternFilters> phase = new SelectEntityPhase<HexPatternFilters>(playerOrigin, filter);

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
            CapacitySetup context = new CapacitySetup(capacityData, source);
            CastCapacityCommand command = new CastCapacityCommand(in context);

            await command.RunAsync(battlePhase);
        }
    }
}