using ATCG.Battle.Commands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Debugging.Cheats;
using UnityEngine;

namespace ATCG.Debugging.Debugging.Battle
{
    [CheatGroup("Mana")]
    public class RemoveManaCheat : ICheat
    {
        public string Name => "Remove Mana";
        public string Description => "Remove mana from the player.";

        [CheatParam("Amount", Min = 0, Max = 20)]
        public int amount = 1;

        private readonly RuntimeLocalBattlePlayer player;

        public RemoveManaCheat(RuntimeLocalBattlePlayer player) => this.player = player;

        public async Awaitable Execute(CheatContext context)
        {
            await Awaitable.MainThreadAsync();
            player.BattlePlayer.AddOrRemoveMana(-amount);
            new ModifyPlayerManaCommand(player.BattlePlayer, -amount).Run(player.BattlePlayer.BattlePhase);
        }
    }
}
