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
    public class EndTurnCommand : PlayerCommand<NoInfos>
    {
        public readonly struct ControllerIterator : IUpdateControllerOnTurnEndIterator
        {
            private readonly StatusContext statusContext;
            private readonly IBattlePlayer playerTurn;

            public ControllerIterator(StatusContext statusContext, IBattlePlayer playerTurn)
            {
                this.statusContext = statusContext;
                this.playerTurn = playerTurn;
            }

            void IUpdateControllerOnTurnEndIterator.Process<T>()
            {
                foreach (var componentRef in statusContext.battlePhase.world.Query<T>())
                {
                    ref T component = ref componentRef.GetValue();

                    if (componentRef.EntityAddress.TryGetComponentRO(out BelongsToPlayerComponent belongsToPlayer))
                    {
                        if(!belongsToPlayer.IsAllieOf(playerTurn))
                            continue;
                    }

                    component.Process();
                }
            }
        }

        public readonly struct StatusIterator : ITickOnTurnEndIterator
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

            public void Process<T>() where T : ITickOnTurnEnd
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


        public EndTurnCommand(IBattlePlayer battlePlayer) : base(battlePlayer)
        {
            this.battlePlayer = battlePlayer;
        }
        private readonly IBattlePlayer battlePlayer;


        protected override void Process(in CommandContext context)
        {
            var statusContext = new StatusContext(battlePlayer.BattlePhase);
            ControllerIterator iterator = new ControllerIterator(statusContext, battlePlayer);
            iterator.ForeachUpdateControllerOnTurnEnd();

            using (ListPool<ComponentRef<StatusTag>>.Get(out var statusToTick))
            {
                StatusIterator statusIterator = new StatusIterator(statusContext, battlePlayer, statusToTick);
                statusIterator.ForeachTickOnTurnEnd();

                foreach (var statusRef in statusToTick)
                {
                    var component = statusRef.GetValue();
                    StatusTickCommand statusTickCommand = new StatusTickCommand(statusRef.EntityAddress, component.data);
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