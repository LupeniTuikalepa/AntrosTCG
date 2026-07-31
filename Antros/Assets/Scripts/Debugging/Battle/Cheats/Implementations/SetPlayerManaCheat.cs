using ATCG.Battle.Commands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Debugging.Cheats;
using UnityEngine;

namespace ATCG.Debugging.Debugging.Battle
{
    [CheatGroup("Mana")]
    public class SetPlayerManaCheat : ICheat
    {
        public string Name => "Set Mana";
        public string Description => "Set the player's mana to an exact value.";

        [CheatParam("Value", Min = 0, Max = 20)]
        public int value = 10;

        private readonly RuntimeLocalBattlePlayer player;

        public SetPlayerManaCheat(RuntimeLocalBattlePlayer player) => this.player = player;

        public async Awaitable Execute(CheatContext context)
        {
            await Awaitable.MainThreadAsync();
            LocalBattlePlayer p = player.BattlePlayer;
            new ModifyPlayerManaCommand(p, value - p.CurrentMana).Run(p.BattlePhase);
        }
    }
}
