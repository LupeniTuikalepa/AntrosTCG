using System.Collections.Generic;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Debugging.Cheats;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Debugging.Debugging.Battle
{
    [CheatGroup("Movement")]
    public class TeleportEntityCheat : ICheat
    {
        public string Name => "Teleport";
        public string Description => "Pick an entity, then click a cell in-game to move it there.";

        [CheatTarget(nameof(Targets), Label = "Entity")]
        public EntityAddress target;

        private readonly LocalBattlePlayer player;

        public TeleportEntityCheat(LocalBattlePlayer player) => this.player = player;

        private IEnumerable<CheatTargetOption> Targets() 
	        => CheatUtilities.EnumerateTargets<MovementComponent>(player);

        public async Awaitable Execute(CheatContext context)
        {
            if (!target.IsValid)
                return;

            BattlePatternController controller = new BattlePatternController(player.BattlePhase.BattleGrid);
            using HexPatternBuilder allCells = new HexPatternBuilder(HexCoordinates.Zero, controller)
                .With(new EverythingPattern());

            AspectFilter<BattleCellAspect> filter = new AspectFilter<BattleCellAspect>();
            SelectEntityPhase<AspectFilter<BattleCellAspect>> phase =
                new SelectEntityPhase<AspectFilter<BattleCellAspect>>(player, filter, allCells);

            EntityAddress[] picked = await phase.Run();
            if (picked.Length > 0 && picked[0].TryGetComponentRO(out GridMemberComponent gridMember))
                new MoveCommand(target, gridMember.coordinates).Run(player.BattlePhase);
        }
    }
}
