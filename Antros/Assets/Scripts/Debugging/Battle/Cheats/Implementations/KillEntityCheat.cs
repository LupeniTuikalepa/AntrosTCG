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
    [CheatGroup("Combat")]
    public class KillEntityCheat : ICheat
    {
        public string Name => "Kill Entity";
        public string Description => "Kill the picked entity.";

        [CheatTarget(nameof(Targets), Label = "Target")]
        public EntityAddress target;

        private readonly LocalBattlePlayer player;

        public KillEntityCheat(LocalBattlePlayer player) => this.player = player;

        private IEnumerable<CheatTargetOption> Targets()
            => CheatUtilities.EnumerateTargets<HealthComponent>(player);

        public async Awaitable Execute(CheatContext context)
        {
            await Awaitable.MainThreadAsync();
            if (!target.IsValid)
                return;

            new DeathCommand(target).Run(player.BattlePhase);
        }
    }
}
