using ATCG.Battle.GameModes;

namespace ATCG.Battle.Commands.Core
{
    public class GameCommandManager
    {
        private readonly BattlePhase battlePhase;

        public GameCommandManager(BattlePhase battlePhase)
        {
            this.battlePhase = battlePhase;
        }

        public void ExecuteGameCommand(GameCommand gameCommand)
        {
            //GameCommandContext context = new(battlePhase, false);
            //gameCommand.Execute(in context);
        }
    }
}