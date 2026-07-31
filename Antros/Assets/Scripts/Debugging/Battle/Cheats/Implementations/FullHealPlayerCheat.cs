using ATCG.Battle.Commands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Debugging.Cheats;
using UnityEngine;

namespace ATCG.Debugging.Debugging.Battle
{
    [CheatGroup("Health")]
    public class FullHealPlayerCheat : ICheat
    {
        public string Name => "Full Heal";
        public string Description => "Restore the player to full health.";

        private readonly RuntimeLocalBattlePlayer player;

        public FullHealPlayerCheat(RuntimeLocalBattlePlayer player) => this.player = player;

        public async Awaitable Execute(CheatContext context)
        {
            await Awaitable.MainThreadAsync();
            LocalBattlePlayer p = player.BattlePlayer;
            new ModifyPlayerHealthCommand(p, p.MaxHealth - p.CurrentHealth).Run(p.BattlePhase);
        }
    }
}
