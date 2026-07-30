using ATCG.Battle.Commands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Debugging.Cheats;
using UnityEngine;

namespace ATCG.Debugging.Debugging.Battle
{
    [CheatGroup("Mana")]
    public class AddManaCheat : ICheat
    {
        public string Name => "Add Mana";
        public string Description => "Give the player mana.";

        [CheatParam("Amount", Min = 0, Max = 20)]
        public int amount = 1;

        private readonly RuntimeLocalBattlePlayer player;

        public AddManaCheat(RuntimeLocalBattlePlayer player) => this.player = player;

        public async Awaitable Execute(CheatContext context)
        {
            await Awaitable.MainThreadAsync();
            player.BattlePlayer.AddOrRemoveMana(amount);
            new ModifyPlayerManaCommand(player.BattlePlayer, amount).Run(player.BattlePlayer.BattlePhase);
        }
    }
}
