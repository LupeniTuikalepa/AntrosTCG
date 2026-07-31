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
    public class SetEntityHealthCheat : ICheat
    {
        public string Name => "Set Health";
        public string Description => "Set the picked entity's health to an exact value.";

        [CheatParam("Value", Min = 0, Max = 100)]
        public int value = 10;

        [CheatTarget(nameof(Targets), Label = "Target")]
        public EntityAddress target;

        private readonly LocalBattlePlayer player;

        public SetEntityHealthCheat(LocalBattlePlayer player) => this.player = player;

        private IEnumerable<CheatTargetOption> Targets()
            => CheatUtilities.EnumerateTargets<HealthComponent>(player);

        public async Awaitable Execute(CheatContext context)
        {
            await Awaitable.MainThreadAsync();
            if (!target.IsValid)
                return;

            int current = target.TryGetComponentRO(out HealthComponent health) ? health.CurrentHealth : 0;
            new HealCommand(value - current, target).Run(player.BattlePhase);
        }
    }
}
