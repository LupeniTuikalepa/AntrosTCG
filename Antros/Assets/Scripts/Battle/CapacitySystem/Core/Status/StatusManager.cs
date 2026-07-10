using System.Threading;
using ATCG.Battle.CapacitySystem.Core.Status.Signals;
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

        public static void RemoveAllFinishedStatus()
        {
            
        }
        /*
        public static void Tick<TStatus>(EntityAddress address, StatusContext statusContext) where TStatus : struct, IStatusComponent
        {
            if (address.TryGetComponent<TStatus>(out var componentRef))
            {
                ref var component = ref componentRef.GetValue();
                component.Trigger(address, statusContext.battlePhase);

                var tickStatusSignal = new StatusSignal(address, StatusAction.Tick, component.StatusData);
                tickStatusSignal.Run(statusContext.battlePhase);
            }
        }

        private static bool IsFinished<TStatus, TController>(EntityAddress address)
            where TStatus : struct, IStatusComponent
            where TController : struct, IStatusController<TStatus>
        {
            if (address.TryGetComponent<TController>(out var componentRef))
            {
                ref TController component = ref componentRef.GetValue();
                if (componentRef.EntityAddress.TryGetComponent<TStatus>(out var statusRef))
                    if (component.IsFinished(statusRef))
                        return true;
            }
            return false;
        }

        public static void ApplyStatus<TStatus, TController>(this EntityAddress address, TStatus status, TController controller, StatusContext statusContext)
            where TStatus : struct, IStatusComponent
            where TController : struct, IStatusController
        {
            if (address.TryGetComponent<StatusReceiver>(out var receiverRef))
            {
                ref StatusReceiver statusReceiver = ref receiverRef.GetValue();
                if (statusReceiver.TryGetStatus<TStatus>(out var statusRef))
                {
                    statusRef.GetValue().Trigger();
                }

                Entity entity = address.entity;

                address.AddOrSetComponent(status);
                address.AddOrSetComponent(controller);

                var removeStatusSignal = new StatusSignal(address, StatusAction.Apply, status.StatusData);
                removeStatusSignal.Run(statusContext.battlePhase);

            }
        }

        public static void RemoveStatus<TStatus>(this EntityAddress address, EntityAddress entityAddress,
	        StatusContext statusContext)
            where TStatus : struct, IStatusComponent
        {
            if (address.TryGetComponentRO<StatusInfos<TStatus>>(out var component))
            {
                address.RemoveAll(component.componentMask);
                address.RemoveComponent<StatusInfos<TStatus>>();

                var removeStatusSignal = new StatusSignal(address, StatusAction.Remove, component.statusData);
                removeStatusSignal.Run(statusContext.battlePhase);
            }
        }

        public static void UpdateAllStatusController<TStatus, TController>(StatusContext statusContext)
            where TStatus : struct, IStatusComponent
            where TController : struct, IStatusController<TStatus>
        {
            using (ListPool<EntityAddress>.Get(out var list))
            {
                foreach (var componentRef in statusContext.battlePhase.world.Query<TController>())
                {
                    ref TController component = ref componentRef.GetValue();
                    if (componentRef.EntityAddress.TryGetComponent<TStatus>(out var statusRef))
                    {
                        if(component.IsFinished(statusRef))
                            list.Add(componentRef.EntityAddress);
                    }
                }

                foreach (var address in list)
                {
                    address.RemoveStatus<TStatus>(address,statusContext);
                }

            }
        }

        public static void ProcessAllStatus<TStatus>(StatusContext statusContext) where TStatus : struct, IStatusComponent
        {
            var world = statusContext.battlePhase.world;
            foreach (var entity in world.Query(EntityQuery.With<TStatus>()))
            {
                EntityAddress address = new EntityAddress(world, entity);
                Tick<TStatus>(address, statusContext);
            }
	        UpdateAllStatusController<TStatus, StatusDurationController<TStatus>>(statusContext);
        }
        */
    }
}