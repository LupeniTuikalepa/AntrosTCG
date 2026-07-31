using System.Collections.Generic;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players.Local;
using ATCG.Debugging.Cheats;
using UnityEngine;

namespace ATCG.Debugging.Debugging.Battle
{
    [CheatGroup("Health")]
    public class FullHealEntityCheat : ICheat
    {
        public string Name => "Full Heal";
        public string Description => "Restore the picked entity to full health.";

        [CheatTarget(nameof(Targets), Label = "Target")]
        public EntityAddress target;

        private readonly LocalBattlePlayer player;

        public FullHealEntityCheat(LocalBattlePlayer player) => this.player = player;

        private IEnumerable<CheatTargetOption> Targets()
            => CheatUtilities.EnumerateTargets<HealthComponent>(player);

        public async Awaitable Execute(CheatContext context)
        {
            await Awaitable.MainThreadAsync();
            if (!target.IsValid)
                return;

            // AddOrRemoveHealth clamps to max, so a large heal fills the bar.
            new HealCommand(999999, target).Run(player.BattlePhase);
        }
    }
}
