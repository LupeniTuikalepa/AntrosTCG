using ATCG.Battle.Commands.Core;
using ATCG.Battle.Entities;
using ATCG.Battle.Grids;
using ATCG.Capacities;
using ATCG.HexGrids;

namespace ATCG.Battle.Commands.GameCommands
{
    public readonly struct CapacityContext
    {
        public readonly CastCapacityCommand evt;
        public readonly CommandContext commandContext;
        public readonly CapacityData data;
        public readonly HexCoordinates castPoint;

        public BattleGrid BattleGrid => commandContext.Grid;

        public World World => commandContext.battlePhase.world;

        public CapacityContext(CastCapacityCommand evt, CapacitySetup capacitySetup, CommandContext commandContext)
        {
            this.evt = evt;
            this.commandContext = commandContext;

            this.data = capacitySetup.data;
            this.castPoint = capacitySetup.castPoint;
        }

        public void EmbedCommand<T>(T command) where T : IGameCommand => evt.Embed(commandContext, command);
    }
}