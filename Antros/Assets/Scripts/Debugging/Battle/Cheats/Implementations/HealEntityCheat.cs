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
    public class HealEntityCheat : ICheat
    {
        public string Name => "Heal";
        public string Description => "Heal the picked entity.";

        [CheatParam("Amount", Min = 0, Max = 100)]
        public int amount = 20;

        [CheatTarget(nameof(Targets), Label = "Target")]
        public EntityAddress target;

        private readonly LocalBattlePlayer player;

        public HealEntityCheat(LocalBattlePlayer player) => this.player = player;

        private IEnumerable<CheatTargetOption> Targets()
            => CheatUtilities.EnumerateTargets<HealthComponent>(player);

        public async Awaitable Execute(CheatContext context)
        {
            await Awaitable.MainThreadAsync();
            if (!target.IsValid)
                return;

            new HealCommand(amount, target).Run(player.BattlePhase);
        }
    }
}
