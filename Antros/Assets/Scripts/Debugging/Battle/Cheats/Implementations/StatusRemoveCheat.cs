using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players.Local;
using ATCG.Capacities.Data.Status;
using ATCG.Debugging.Cheats;
using UnityEngine;

namespace ATCG.Debugging.Debugging.Battle.Cheats.Implementations
{
    [CheatGroup("Status")]
    public class StatusRemoveCheat : ICheat
    {
        public string Name => "Remove Status";
        public string Description => "Remove a status from the picked entity (all statuses if none is set).";

        [CheatTarget(nameof(Targets), Label = "Target")]
        public EntityAddress target;

        [CheatParam("Status (optional)")]
        public StatusData status;

        private readonly LocalBattlePlayer player;

        public StatusRemoveCheat(LocalBattlePlayer player) => this.player = player;

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
                new RemoveStatusCommand(target, data).Run(player.BattlePhase);
        }
    }
}
