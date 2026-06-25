using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities.Components.Status.Signals;
using ATCG.Battle.Entities.Queries;
using ATCG.Battle.GameModes;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Entities.Components.Status
{
    public static class StatusManager
    {
        public static void Trigger<TStatus>(EntityAddress address, StatusContext statusContext) where TStatus : struct, IStatusComponent
        {
            if (address.TryGetComponent<TStatus>(out var componentRef))
            {
                ref var component = ref componentRef.GetValue();
                component.Trigger(address, statusContext.battlePhase);
            }
            
            UpdateControllers<TStatus>(address, statusContext);
        }

        private static void UpdateControllers<TStatus>(EntityAddress address, StatusContext statusContext) where TStatus : struct, IStatusComponent
        {
            if (IsFinished<TStatus, StatusDurationController<TStatus>>(address))
            {
                address.RemoveStatus<TStatus>(statusContext);
            }
            
            if (IsFinished<TStatus, StatusCustomController<TStatus>>(address))
            {
                address.RemoveStatus<TStatus>(statusContext);
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
            where TController : struct, IStatusController<TStatus>
        {
            address.AddOrSetComponent(status);
            address.AddOrSetComponent(controller);
            ComponentMask mask = ComponentMask.With<TStatus>().With<TController>();
            address.AddOrSetComponent(new StatusInfos<TStatus>(mask));
            
            var removeStatusSignal = new StatusSignal(address.entity.id, StatusAction.Apply);
            removeStatusSignal.Run(statusContext.battlePhase);

        }

        public static void RemoveStatus<TStatus>(this EntityAddress address, StatusContext statusContext)
            where TStatus : struct, IStatusComponent
        {
            if (address.TryGetComponentRO<StatusInfos<TStatus>>(out var component))
            {
                address.RemoveAll(component.componentMask);
                address.RemoveComponent<StatusInfos<TStatus>>();
                
                var removeStatusSignal = new StatusSignal(address.entity.id, StatusAction.Remove);
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
                    address.RemoveStatus<TStatus>(statusContext);
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
        }
    }
}