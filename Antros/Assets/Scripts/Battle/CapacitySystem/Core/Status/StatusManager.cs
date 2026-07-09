using System.Threading;
using ATCG.Battle.CapacitySystem.Core.Status.Signals;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Battle.Entities.Queries;
using UnityEngine.Pool;

namespace ATCG.Battle.CapacitySystem.Core.Status
{
    public static class StatusManager
    {
	    private class StatusControllerUpdater : IStatusComponentIterator
	    {
		    private StatusContext statusContext;
		    public StatusControllerUpdater(StatusContext statusContext)
		    {
			    this.statusContext = statusContext;
		    }

		    public void Process<TStatusComponent>() where TStatusComponent : struct, IStatusComponent
		    {
			    UpdateAllStatusController< TStatusComponent, StatusDurationController<TStatusComponent>>(statusContext);
			    UpdateAllStatusController< TStatusComponent, StatusVolatileController<TStatusComponent>>(statusContext);
			    UpdateAllStatusController< TStatusComponent, StatusCustomController<TStatusComponent>>(statusContext);
		    }
	    }
	    
        public static void Trigger<TStatus>(EntityAddress address, StatusContext statusContext) where TStatus : struct, IStatusComponent
        {
            if (address.TryGetComponent<TStatus>(out var componentRef))
            {
                ref var component = ref componentRef.GetValue();
                component.Trigger(address, statusContext.battlePhase);

                var tickStatusSignal = new StatusSignal(address, StatusAction.Tick, component.StatusData);
                tickStatusSignal.Run(statusContext.battlePhase);
            }

           
        }

        private static void UpdateControllers<TStatus>(EntityAddress address, StatusContext statusContext) where TStatus : struct, IStatusComponent
        {
            if (IsFinished<TStatus, StatusDurationController<TStatus>>(address))
            {
                address.RemoveStatus<TStatus>(address,statusContext);
            }

            if (IsFinished<TStatus, StatusCustomController<TStatus>>(address))
            {
                address.RemoveStatus<TStatus>(address,statusContext);
            }
        }

        public static void UpdateControllers(StatusContext context)
        {
	        StatusControllerUpdater statusControllerUpdater = new StatusControllerUpdater(context);
	        statusControllerUpdater.ForeachStatusComponent();
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
            where TController : struct, IStatusController<TStatus>
        {
            address.AddOrSetComponent(status);
            address.AddOrSetComponent(controller);
            ComponentMask mask = ComponentMask.With<TStatus>().With<TController>();
            address.AddOrSetComponent(new StatusInfos<TStatus>(mask, status.StatusData));

            var removeStatusSignal = new StatusSignal(address, StatusAction.Apply, status.StatusData);
            removeStatusSignal.Run(statusContext.battlePhase);

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
                Trigger<TStatus>(address, statusContext);
            }
	        UpdateAllStatusController<TStatus, StatusDurationController<TStatus>>(statusContext);
        }
    }
}