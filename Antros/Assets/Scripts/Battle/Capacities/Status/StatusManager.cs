using ATCG.Battle.Entities.Queries;
using UnityEngine.Pool;

namespace ATCG.Battle.Entities.Components.Status
{
    public static class StatusManager
    {
        public static void Trigger<TStatus>(EntityAddress address) where TStatus : struct, IStatus
        {
            if (address.TryGetComponent<TStatus>(out var componentRef))
            {
                ref var component = ref componentRef.GetValue();
                component.Trigger(address);
            }

            UpdateControllers<TStatus>(address);
        }

        private static void UpdateControllers<TStatus>(EntityAddress address) where TStatus : struct, IStatus
        {
            if (IsFinished<TStatus, StatusDurationController<TStatus>>(address))
            {
                address.RemoveStatus<TStatus>();
            }
            
            if (IsFinished<TStatus, StatusCustomController<TStatus>>(address))
            {
                address.RemoveStatus<TStatus>();
            }
        }

        public static bool IsFinished<TStatus, TController>(EntityAddress address)
            where TStatus : struct, IStatus
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
        
        public static void ApplyStatus<TStatus, TController>(this EntityAddress address, TStatus status, TController controller)
            where TStatus : struct, IStatus 
            where TController : struct, IStatusController<TStatus>
        {
            address.AddOrSetComponent(status);
            address.AddOrSetComponent(controller);
            ComponentMask mask = ComponentMask.With<TStatus>().With<TController>();
            address.AddOrSetComponent(new StatusInfos<TStatus>(mask));
        }

        public static void RemoveStatus<TStatus>(this EntityAddress address)
            where TStatus : struct, IStatus
        {
            if (address.TryGetComponentRO<StatusInfos<TStatus>>(out var component))
            {
                address.RemoveAll(component.componentMask);
                address.RemoveComponent<StatusInfos<TStatus>>();
            }
        }
        
        public static void UpdateAllStatusController<TStatus, TController>(World world) 
            where TStatus : struct, IStatus 
            where TController : struct, IStatusController<TStatus>
        {
            using (ListPool<EntityAddress>.Get(out var list))
            {
                foreach (var componentRef in world.Query<TController>())
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
                    address.RemoveStatus<TStatus>();
                }
                
            }
        }
        
        public static void ProcessAllControllers<TStatus>(World world) where TStatus : struct, IStatus
        {
            foreach (var entity in world.Query(EntityQuery.With<TStatus>()))
            {
                EntityAddress address = new EntityAddress(world, entity);
                UpdateControllers<TStatus>(address);
            }
        }
    }
}