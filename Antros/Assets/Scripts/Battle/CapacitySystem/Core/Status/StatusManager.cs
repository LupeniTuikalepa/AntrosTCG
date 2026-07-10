using System.Collections.Generic;
using System.Threading;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.CapacitySystem.Core.Status.Signals;
using ATCG.Battle.Commands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Battle.Entities.Queries;
using ATCG.Capacities.Data.Status;
using Helteix.Tools.DataMapping;
using UnityEngine.Pool;

namespace ATCG.Battle.CapacitySystem.Core.Status
{
    public static class StatusManager
    {
        private readonly struct FinishedStatusControllerIterator : IStatusControllerIterator
        {
            private readonly StatusContext statusContext;
            private readonly List<ComponentRef<StatusTag>> statusToRemove;

            public FinishedStatusControllerIterator(StatusContext statusContext, List<ComponentRef<StatusTag>> statusToRemove)
            {
                this.statusContext = statusContext;
                this.statusToRemove = statusToRemove;
            }

            public void Process<T>() where T : struct, IStatusController
            {
                foreach (ComponentRef<T> controller in statusContext.World.Query<T>())
                {
                    ref var controllerComponent = ref controller.GetValue();
                    if (controllerComponent.IsFinished() && controller.EntityAddress.TryGetComponent<StatusTag>(out var tag))
                        statusToRemove.Add(tag);
                }
            }
        }
        private readonly struct FinishedStatusControllerOnEntityIterator : IStatusControllerIterator
        {
            private readonly EntityAddress target;
            private readonly StatusContext statusContext;
            private readonly List<ComponentRef<StatusTag>> statusToRemove;

            public FinishedStatusControllerOnEntityIterator(StatusContext statusContext, List<ComponentRef<StatusTag>> statusToRemove, EntityAddress target)
            {
                this.statusContext = statusContext;
                this.statusToRemove = statusToRemove;
                this.target = target;
            }

            public void Process<T>() where T : struct, IStatusController
            {
                if (target.TryGetComponent<StatusReceiver>(out var statusReceiverRef))
                {
                    foreach (var statusRef in statusReceiverRef.GetValue().AllStatus)
                    {
                        if (!statusRef.EntityAddress.TryGetComponent<T>(out var controller))
                            continue;

                        ref var controllerComponent = ref controller.GetValue();
                        if (controllerComponent.IsFinished() &&
                            controller.EntityAddress.TryGetComponent<StatusTag>(out var tag))
                            statusToRemove.Add(tag);
                    }
                }
            }
        }

        public static bool HasStatusWithData<T>(this EntityAddress address) where T : StatusData
        {
            if (address.TryGetComponentRO(out StatusReceiver statusReceiver))
            {
                foreach (ComponentRef<StatusTag> statusTagRef in statusReceiver.AllStatus)
                {
                    StatusTag statusTag = statusTagRef.GetValue();
                    if(statusTag.data is T)
                        return true;
                }
            }

            return false;
        }


        public static void RemoveAllFinishedStatus(in StatusContext statusContext, EntityAddress target)
        {
            using (ListPool<ComponentRef<StatusTag>>.Get(out var statusToRemove))
            {
                FinishedStatusControllerOnEntityIterator finishedStatusControllerIterator = new(statusContext, statusToRemove, target);

                finishedStatusControllerIterator.ForeachStatusController();

                foreach (var statusRef in statusToRemove)
                {
                    StatusTag statusTag = statusRef.GetValue();
                    StatusRemoveCommand statusRemoveCommand = new StatusRemoveCommand(target, statusTag.data);
                    statusRemoveCommand.Run(statusContext.battlePhase);
                }
            }
        }

        public static void RemoveAllFinishedStatus(in StatusContext statusContext)
        {
            using (ListPool<ComponentRef<StatusTag>>.Get(out var statusToRemove))
            {
                FinishedStatusControllerIterator finishedStatusControllerIterator =
                    new FinishedStatusControllerIterator(statusContext, statusToRemove);

                finishedStatusControllerIterator.ForeachStatusController();

                foreach (var statusRef in statusToRemove)
                {
                    StatusTag statusTag = statusRef.GetValue();

                    EntityAddress target = new EntityAddress(statusContext.World, statusTag.targetEntity);
                    StatusRemoveCommand statusRemoveCommand = new StatusRemoveCommand(target, statusTag.data);
                    statusRemoveCommand.Run(statusContext.battlePhase);
                }
            }
        }

    }
}