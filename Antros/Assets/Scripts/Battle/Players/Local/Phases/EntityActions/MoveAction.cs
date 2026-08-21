using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.HexGrids;
using ATCG.Metrics;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle
{
    public class MoveAction : EntityAction
    {
        public override int ManaCost => GameMetrics.Current.MovementCost;

        private readonly int speed;
        public MoveAction(LocalBattlePlayer fromPlayer, int speed) : base(fromPlayer)
        {
            this.speed = speed;
        }

        public override async Awaitable Execute(EntityAddress address, BattlePhase battlePhase)
        {
            if (!address.TryGetComponentRO(out GridMemberComponent gridMemberComponent))
                return;

            if(!address.HasComponent<MovementComponent>())
                return;

            HexCoordinates center = gridMemberComponent.coordinates;

            // The game imposes a ring-1 pattern; movement behaviour comes from the entity's
            // PathfindingAgentComponent (resolved inside the phase from `address`). Speed is the
            // number of tiles the unit can cross.
            CreatePathPhase phase = new CreatePathPhase(fromPlayer, address, center, speed);
            HexCoordinates[] result = await phase.Run();
            if (result.Length == 0)
                return;

            using (CommandManager.BeginGroup($"[{address.entity.id}] Entity Movement"))
            {
                ModifyPlayerManaCommand manaCommand = new ModifyPlayerManaCommand(fromPlayer, -ManaCost);
                manaCommand.Run(battlePhase);

                MoveAlongPathCommand pathCommand = new MoveAlongPathCommand(address, result, speed);
                await pathCommand.RunAsync(battlePhase);
            }
        }
    }
}