using ATCG.Battle.Entities.Queries;
using ATCG.Battle.GameModes;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Entities.Components.Status
{
    public static class StatusManager
    {
        public static void Trigger<TStatus>(EntityAddress address, BattlePhase battlePhase) where TStatus : struct, IStatusComponent
        {
            Debug.Log($"Battle phase: {battlePhase}");

            if (address.TryGetComponent<TStatus>(out var componentRef))
            {
                ref var component = ref componentRef.GetValue();
                component.Trigger(address, battlePhase);
            }

            UpdateControllers<TStatus>(address);
        }

        private static void UpdateControllers<TStatus>(EntityAddress address) where TStatus : struct, IStatusComponent
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
        
        public static void ApplyStatus<TStatus, TController>(this EntityAddress address, TStatus status, TController controller)
            where TStatus : struct, IStatusComponent 
            where TController : struct, IStatusController<TStatus>
        {
            address.AddOrSetComponent(status);
            address.AddOrSetComponent(controller);
            ComponentMask mask = ComponentMask.With<TStatus>().With<TController>();
            address.AddOrSetComponent(new StatusInfos<TStatus>(mask));
        }

        public static void RemoveStatus<TStatus>(this EntityAddress address)
            where TStatus : struct, IStatusComponent
        {
            if (address.TryGetComponentRO<StatusInfos<TStatus>>(out var component))
            {
                address.RemoveAll(component.componentMask);
                address.RemoveComponent<StatusInfos<TStatus>>();
            }
        }
        
        public static void UpdateAllStatusController<TStatus, TController>(World world) 
            where TStatus : struct, IStatusComponent 
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
        
        public static void ProcessAllStatus<TStatus>(BattlePhase battlePhase) where TStatus : struct, IStatusComponent
        {
            
            var world = battlePhase.world;
            foreach (var entity in world.Query(EntityQuery.With<TStatus>()))
            {
                EntityAddress address = new EntityAddress(world, entity);
                Trigger<TStatus>(address, battlePhase);
            }
            
        }
    }
}