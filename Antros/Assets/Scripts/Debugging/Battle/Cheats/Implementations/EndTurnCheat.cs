using ATCG.Battle.Commands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Players.Local;
using ATCG.Debugging.Cheats;
using UnityEngine;

namespace ATCG.Debugging.Debugging.Battle
{
    [CheatGroup("Turn")]
    public class EndTurnCheat : ICheat
    {
        public string Name => "End Turn";
        public string Description => "End this player's turn.";

        private readonly LocalBattlePlayer player;

        public EndTurnCheat(LocalBattlePlayer player) => this.player = player;

        public async Awaitable Execute(CheatContext context)
        {
            await Awaitable.MainThreadAsync();
            new EndTurnCommand(player).Run(player.BattlePhase);
        }
    }
}
