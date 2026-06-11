using ATCG.Battle.Cards.Capacities;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids.Patterns;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Capacities;
using ATCG.Capacities.Data;
using ATCG.HexGrids;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle
{
    public class CastCapacityAction : IEntityAction
    {
        public int ManaCost => capacityData.Cost;

        public readonly CapacityData capacityData;

        public CastCapacityAction(CapacityData capacityData)
        {
            this.capacityData = capacityData;
        }


        //TODO pour les spells, le cast des capacites ne se fera pas depuis une action donc il faudra sortir la logique et la rendre commune dans le capacity manager.


        public async Awaitable Execute(EntityAddress address, BattlePhase battlePhase)
        {
            ICapacityPatternData[] patterns = capacityData.CastPatterns;

            //If no pattern, use the entity position
            if (patterns.Length == 0)
            {
                if (address.TryGetComponentRO(out BattleGridElementComponent component))
                    await ExecuteCommand(battlePhase, component.coordinates);
            }
            else
            {
                //Collects every selectableCell By The player and wait for him to choose.
                using (HashSetPool<HexCoordinates>.Get(out var possibilities))
                {
                    for (int i = 0; i < patterns.Length; i++)
                    {
                        ICapacityPatternData patternData = patterns[i];
                        if (!CapacityManager.TryGetFor(patternData, out ICellPattern pattern))
                            continue;

                        foreach (var coordinate in pattern.GetAllCoordinates())
                            possibilities.Add(coordinate);
                    }

                    SpecificBattleCellFilter filter = new SpecificBattleCellFilter(possibilities);

                    SelectEntityPhase<SpecificBattleCellFilter> phase =
                        new SelectEntityPhase<SpecificBattleCellFilter>(filter);

                    EntityAddress[] result = await phase;

                    for (int i = 0; i < result.Length; i++)
                    {
                        EntityAddress target = result[i];
                        if (target.TryGetComponentRO(out BattleGridElementComponent component))
                            await ExecuteCommand(battlePhase, component.coordinates);

                    }
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