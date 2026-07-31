using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players.Local;
using ATCG.Capacities.Data.Status;
using ATCG.Debugging.Cheats;
using ATCG.Debugging.Debugging.Battle;
using UnityEngine;

namespace ATCG.Debugging
{
    [CheatGroup("Status")]
    public class StatusApplyCheat : ICheat
    {
        public string Name => "Apply Status";
        public string Description => "Apply a status to the picked entity (all statuses if none is set).";

        [CheatTarget(nameof(Targets), Label = "Target")]
        public EntityAddress target;

        [CheatParam("Status (optional)")]
        public StatusData status;

        private readonly LocalBattlePlayer player;

        public StatusApplyCheat(LocalBattlePlayer player) => this.player = player;

        private IEnumerable<CheatTargetOption> Targets()
            => CheatUtilities.EnumerateTargets<HealthComponent>(player);

        public async Awaitable Execute(CheatContext context)
        {
            await Awaitable.MainThreadAsync();
            if (!target.IsValid)
                return;

            StatusData[] datas = status != null
                ? new[] { status }
                : Resources.LoadAll<StatusData>("Database/Status");

            foreach (StatusData data in datas)
                new ApplyStatusCommand(target, data).Run(player.BattlePhase);
        }
    }
}
