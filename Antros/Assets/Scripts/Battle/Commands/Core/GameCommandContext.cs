using ATCG.Battle.Entities;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;

namespace ATCG.Battle.Commands.Core
{
    public readonly struct GameCommandContext
    {
        public readonly BattlePhase battlePhase;

        public BattleGrid Grid => battlePhase.BattleGrid;

        public World World => battlePhase.World;


        public GameCommandContext(BattlePhase battlePhase)
        {
            this.battlePhase = battlePhase;

        }

        public IBattlePlayer GetPlayer(int playerID) => battlePhase.GetPlayer(playerID);


        public static implicit operator World(GameCommandContext context) => context.World;

        public static implicit operator BattleGrid(GameCommandContext context) => context.Grid;

        public static implicit operator BattlePhase(GameCommandContext context) => context.battlePhase;
    }
}