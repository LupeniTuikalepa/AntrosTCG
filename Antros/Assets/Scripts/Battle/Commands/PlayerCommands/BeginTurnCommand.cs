using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.CapacitySystem.Status.Iterations;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.GameCommands.Players
{
    public class BeginTurnCommand : PlayerCommand<NoInfos>
    {
        public readonly struct ControllerIterator : IUpdateControllerOnTurnBeginIterator
        {
            private readonly StatusContext statusContext;
            private readonly IBattlePlayer playerTurn;

            public ControllerIterator(StatusContext statusContext, IBattlePlayer playerTurn)
            {
                this.statusContext = statusContext;
                this.playerTurn = playerTurn;
            }

            void IUpdateControllerOnTurnBeginIterator.Process<T>()
            {
                World world = statusContext.battlePhase.world;
                foreach (var componentRef in world.Query<T>())
                {
                    // Only tick a controller on ITS OWNER's turn. Ownership is read from
                    // the status TARGET (the source of truth), and the guard fails CLOSED:
                    // if the owner can't be resolved we skip, instead of processing for
                    // everyone. The old guard only skipped when a BelongsToPlayerComponent
                    // was present AND not an ally, so any controller whose entity lacked
                    // that component ticked on every player's turn — durations lost a tick
                    // each turn regardless of who was playing.
                    if (!componentRef.EntityAddress.TryGetComponentRO(out StatusTag statusTag))
                        continue;

                    EntityAddress target = new EntityAddress(world, statusTag.targetEntity);
                    if (!target.TryGetComponentRO(out BelongsToPlayerComponent belongsToPlayer)
                        || !belongsToPlayer.IsAllieOf(playerTurn))
                        continue;

                    ref T component = ref componentRef.GetValue();
                    component.Process();
                }
            }
        }

        public readonly struct StatusIterator : ITickOnTurnBeginIterator
        {
            private readonly StatusContext statusContext;
            private readonly IBattlePlayer playerTurn;

            private readonly List<ComponentRef<StatusTag>> output;

            public StatusIterator(StatusContext statusContext, IBattlePlayer playerTurn, List<ComponentRef<StatusTag>> output)
            {
                this.statusContext = statusContext;
                this.playerTurn = playerTurn;
                this.output = output;
            }

            public void Process<T>() where T : ITickOnTurnBegin
            {
                foreach (var componentRef in statusContext.battlePhase.world.Query<StatusSignature<T>>())
                {
                    ref var component = ref componentRef.GetValue();

                    EntityAddress target = componentRef.EntityAddress;
                    if (target.TryGetComponentRO(out BelongsToPlayerComponent belongsToPlayer))
                    {
                        if(!belongsToPlayer.IsAllieOf(playerTurn))
                            continue;
                    }

                    if(target.TryGetComponent<StatusTag>(out var statusTag))
                        output.Add(statusTag);
                }
            }
        }


        public BeginTurnCommand(IBattlePlayer battlePlayer) : base(battlePlayer)
        {
            this.battlePlayer = battlePlayer;
        }
        private readonly IBattlePlayer battlePlayer;


        protected override void Process(in CommandContext context)
        {
            var statusContext = new StatusContext(battlePlayer.BattlePhase);
            ControllerIterator iterator = new ControllerIterator(statusContext, battlePlayer);
            iterator.ForeachUpdateControllerOnTurnBegin();

            using (ListPool<ComponentRef<StatusTag>>.Get(out var statusToTick))
            {
                StatusIterator statusIterator = new StatusIterator(statusContext, battlePlayer, statusToTick);
                statusIterator.ForeachTickOnTurnBegin();

                foreach (var statusRef in statusToTick)
                {
                    StatusTag statusTag = statusRef.GetValue();
                    // Aim the tick at the status TARGET (the entity that owns the
                    // StatusReceiver), NOT at the status entity. Status.Tick resolves the
                    // status through the target's StatusReceiver via HasStatusWithData; the
                    // status entity has no StatusReceiver, so passing its address made that
                    // check fail and OnTick never ran — the status never ticked.
                    EntityAddress target = new EntityAddress(statusContext.World, statusTag.targetEntity);
                    StatusTickCommand statusTickCommand = new StatusTickCommand(target, statusTag.data);
                    Inject(in context, statusTickCommand);
                }
            }

            using (ListPool<ComponentRef<StatusTag>>.Get(out var statusToRemove))
            {
                StatusManager.GetAllFinishedStatus(in statusContext, statusToRemove);
                foreach (var statusRef in statusToRemove)
                {
                    StatusTag statusTag = statusRef.GetValue();

                    EntityAddress target = new EntityAddress(statusContext.World, statusTag.targetEntity);
                    StatusRemoveCommand statusRemoveCommand = new StatusRemoveCommand(target, statusTag.data);
                    Inject(in context, statusRemoveCommand);
                }
            }

        }
    }
}