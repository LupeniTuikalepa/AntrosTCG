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
        public readonly GameCommandContext gameCommandContext;
        public readonly CapacityData data;
        public readonly HexCoordinates castPoint;

        public BattleGrid BattleGrid => gameCommandContext.Grid;

        public World World => gameCommandContext.battlePhase.world;

        public CapacityContext(CastCapacityCommand evt, CapacitySetup capacitySetup, GameCommandContext gameCommandContext)
        {
            this.evt = evt;
            this.gameCommandContext = gameCommandContext;

            this.data = capacitySetup.data;
            this.castPoint = capacitySetup.castPoint;
        }

        public void EmbedCommand<T>(T command) where T : IGameCommand => evt.Embed(gameCommandContext, command);
    }
}