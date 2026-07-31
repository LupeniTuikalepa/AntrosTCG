using ATCG.Battle.Commands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Debugging.Cheats;
using UnityEngine;

namespace ATCG.Debugging.Debugging.Battle
{
    [CheatGroup("Mana")]
    public class FullManaCheat : ICheat
    {
        public string Name => "Full Mana";
        public string Description => "Refill the player's mana.";

        private readonly RuntimeLocalBattlePlayer player;

        public FullManaCheat(RuntimeLocalBattlePlayer player) => this.player = player;

        public async Awaitable Execute(CheatContext context)
        {
            await Awaitable.MainThreadAsync();
            LocalBattlePlayer p = player.BattlePlayer;
            new ModifyPlayerManaCommand(p, p.MaxMana - p.CurrentMana).Run(p.BattlePhase);
        }
    }
}
