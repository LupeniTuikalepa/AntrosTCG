using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Enums;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns.Building;
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

            if(!address.TryGetComponentRO(out MovementComponent movementComponent))
                return;

            HexCoordinates center = gridMemberComponent.coordinates;
            PatternGroup movementPatternData = movementComponent.pattern;


            Awaitable<PhaseResult<HexCoordinates[]>> awaitable;

            switch (movementComponent.movementType)
            {
                case MovementType.Walk:
                    WalkingPathGenerator walkingPathGenerator = new WalkingPathGenerator();
                    CreatePathPhase<WalkingPathGenerator> walkPhase =  new CreatePathPhase<WalkingPathGenerator>(fromPlayer, center, speed, movementPatternData, walkingPathGenerator);
                    awaitable = walkPhase.Run();
                    break;
                case MovementType.Flight:
                    FlightPathGenerator flightPathGenerator = new FlightPathGenerator();
                    CreatePathPhase<FlightPathGenerator> flightPhase =  new CreatePathPhase<FlightPathGenerator>(fromPlayer, center, speed, movementPatternData, flightPathGenerator);
                    awaitable = flightPhase.Run();
                    break;
                case MovementType.Teleportation:
                    TeleportationPathGenerator teleportationPathGenerator = new TeleportationPathGenerator();
                    CreatePathPhase<TeleportationPathGenerator> teleportationPhase =  new CreatePathPhase<TeleportationPathGenerator>(fromPlayer, center, speed, movementPatternData, teleportationPathGenerator);
                    awaitable = teleportationPhase.Run();
                    break;
                default:
                    awaitable = null;
                    break;
            }

            if(awaitable == null)
                return;

            HexCoordinates[] result = await awaitable;
            if (result.Length == 0)
                return;

            using (CommandManager.BeginGroup($"[{address.entity.id}] Entity Movement"))
            {
                ModifyPlayerManaCommand manaCommand = new ModifyPlayerManaCommand(fromPlayer, -ManaCost);
                manaCommand.Run(battlePhase);

                MoveAlongPathCommand pathCommand = new MoveAlongPathCommand(address, result);
                await pathCommand.RunAsync(battlePhase);
            }
        }
    }
}