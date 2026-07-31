using ATCG.Battle.Commands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Debugging.Cheats;
using UnityEngine;

namespace ATCG.Debugging.Debugging.Battle
{
    [CheatGroup("Health")]
    public class SetPlayerHealthCheat : ICheat
    {
        public string Name => "Set Health";
        public string Description => "Set the player's health to an exact value.";

        [CheatParam("Value", Min = 0, Max = 200)]
        public int value = 100;

        private readonly RuntimeLocalBattlePlayer player;

        public SetPlayerHealthCheat(RuntimeLocalBattlePlayer player) => this.player = player;

        public async Awaitable Execute(CheatContext context)
        {
            await Awaitable.MainThreadAsync();
            LocalBattlePlayer p = player.BattlePlayer;
            new ModifyPlayerHealthCommand(p, value - p.CurrentHealth).Run(p.BattlePhase);
        }
    }
}
