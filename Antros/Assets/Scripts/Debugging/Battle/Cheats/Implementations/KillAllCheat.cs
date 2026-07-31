using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players.Local;
using ATCG.Debugging.Cheats;
using UnityEngine;

namespace ATCG.Debugging.Debugging.Battle
{
    [CheatGroup("Combat")]
    public class KillAllCheat : ICheat
    {
        public string Name => "Kill All";
        public string Description => "Kill every entity that has health.";

        private readonly LocalBattlePlayer player;

        public KillAllCheat(LocalBattlePlayer player) => this.player = player;

        public async Awaitable Execute(CheatContext context)
        {
            await Awaitable.MainThreadAsync();

            // EnumerateTargets returns a materialised snapshot, so destroying while iterating is fine.
            foreach (CheatTargetOption option in CheatUtilities.EnumerateTargets<HealthComponent>(player))
                new DeathCommand(option.Address).Run(player.BattlePhase);
        }
    }
}
