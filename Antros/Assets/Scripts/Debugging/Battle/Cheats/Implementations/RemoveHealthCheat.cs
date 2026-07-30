using ATCG.Battle.Commands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Debugging.Cheats;
using UnityEngine;

namespace ATCG.Debugging.Debugging.Battle
{
    [CheatGroup("Health")]
    public class RemoveHealthCheat : ICheat
    {
        public string Name => "Remove Health";
        public string Description => "Remove health from the player.";

        [CheatParam("Amount", Min = 0, Max = 200)]
        public int amount = 5;

        private readonly RuntimeLocalBattlePlayer player;

        public RemoveHealthCheat(RuntimeLocalBattlePlayer player) => this.player = player;

        public async Awaitable Execute(CheatContext context)
        {
            await Awaitable.MainThreadAsync();
            player.BattlePlayer.AddOrRemoveHealth(-amount);
            new ModifyPlayerHealthCommand(player.BattlePlayer, -amount).Run(player.BattlePlayer.BattlePhase);
        }
    }
}
