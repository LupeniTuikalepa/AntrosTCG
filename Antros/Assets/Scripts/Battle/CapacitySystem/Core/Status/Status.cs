using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Capacities.Data.Status;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Core.Status
{
    public abstract class Status<TData, TController> : Status<TData, DefaultStatusComponent<TData>, TController>
        where TData : StatusData
        where TController : struct, IStatusController
    {
        protected sealed override DefaultStatusComponent<TData> CreateStatusComponent(TData data, in StatusContext context)
        {
            return new DefaultStatusComponent<TData>(data);
        }
    }

    public abstract class Status<TData, TComponent, TController> : IStatus<TData>
        where TData : StatusData
        where TComponent : struct, IStatusComponent
        where TController : struct, IStatusController
    {
        public struct EntityStatusInfos
        {
            public EntityAddress targetAddress;
            public EntityAddress statusAddress;

            public ref TComponent StatusComponent => ref statusComponentRef.GetValue();

            public ref TController StatusController => ref statusControllerRef.GetValue();

            public ComponentRef<TComponent> statusComponentRef;
            public ComponentRef<TController> statusControllerRef;
        }

        void IStatus<TData>.Apply(TData data, EntityAddress target, StatusContext context)
        {
            if (!target.TryGetComponent<StatusReceiver>(out var statusReceiverRef))
            {
                Debug.LogWarning("Trying to apply status on entity without StatusReceiver component");
                return;
            }

            ref StatusReceiver statusReceiver = ref statusReceiverRef.GetValue();

            if (statusReceiver.Has<TData>(out var statusTagRef))
            {
                EntityAddress statusAddress = statusTagRef.EntityAddress;
                if(!statusAddress.TryGetComponent<TComponent>(out var componentRef))
                    Debug.LogWarning($"{typeof(TComponent).Name} component not found on entity {statusAddress.entity.id} of status {GetType().Name}");
                if(!statusAddress.TryGetComponent<TController>(out var controllerRef))
                    Debug.LogWarning($"{typeof(TController).Name} controller not found on entity {statusAddress.entity.id} of status {GetType().Name}");

                EntityStatusInfos statusInfos = new EntityStatusInfos()
                {
                    targetAddress = target,
                    statusAddress = statusAddress,
                    statusComponentRef = componentRef,
                    statusControllerRef = controllerRef,
                };

                OnStack(data, in statusInfos, in context);
            }
            else
            {
                World world = context.World;

                Entity statusEntity = world.CreateEntity(target);
                EntityAddress statusEntityAddress = new EntityAddress(world, statusEntity);

                StatusTag statusTag = new StatusTag(data, ComponentID<TComponent>.ID, ComponentID<TController>.ID, target);
                world.AddOrSetComponent(statusEntityAddress, statusTag);
                world.AddOrSetComponent(statusEntityAddress, CreateStatusComponent(data, context));
                world.AddOrSetComponent(statusEntityAddress, CreateStatusController(data, context));
                AddSignature(statusEntityAddress);

                if(target.TryGetComponentRO(out BelongsToPlayerComponent belongsToPlayerComponent))
                    statusEntityAddress.AddOrSetComponent(new BelongsToPlayerComponent(belongsToPlayerComponent.playerId, belongsToPlayerComponent.playerNumber));

                statusReceiver.RegisterStatus(statusEntityAddress.GetComponentRef<StatusTag>());

                EntityStatusInfos statusInfos = new EntityStatusInfos()
                {
                    targetAddress = target,
                    statusAddress = statusEntityAddress,
                    statusComponentRef = statusEntityAddress.GetComponentRef<TComponent>(),
                    statusControllerRef = statusEntityAddress.GetComponentRef<TController>(),
                };

                OnApply(data, in statusInfos, in context);
            }
        }

        void IStatus<TData>.Remove(TData data, EntityAddress target, StatusContext context)
        {
            if (!target.TryGetComponent<StatusReceiver>(out var statusReceiverRef))
            {
                Debug.LogWarning("Trying to remove status on entity without StatusReceiver component");
                return;
            }

            ref StatusReceiver statusReceiver = ref statusReceiverRef.GetValue();
            if (statusReceiver.Has<TData>(out var statusTagRef))
            {
                EntityAddress statusAddress = statusTagRef.EntityAddress;
                EntityStatusInfos statusInfos = GetInfosFromTag(statusTagRef, in context);
                OnRemove(data, in statusInfos, in context);

                statusReceiver.UnregisterStatus(statusTagRef);
                context.World.DestroyEntity(statusAddress.entity);
            }
        }

        void IStatus<TData>.Tick(TData data, EntityAddress target, StatusContext context)
        {
            if (target.HasStatusWithData<TData>(out ComponentRef<StatusTag> statusTagRef))
            {
                EntityStatusInfos statusInfos = GetInfosFromTag(statusTagRef, in context);
                OnTick(data, in statusInfos, in context);

                //Only checking for the target to avoid non-necessary queries
                StatusManager.RemoveAllFinishedStatus(context, target);
            }
        }


        public static EntityStatusInfos GetInfosFromTag(ComponentRef<StatusTag> tagRef, in StatusContext context)
        {
            return new EntityStatusInfos()
            {
                targetAddress = new EntityAddress(tagRef.GetValue().targetEntity, context.World),
                statusAddress = tagRef.EntityAddress,
                statusComponentRef = tagRef.EntityAddress.GetComponentRef<TComponent>(),
                statusControllerRef = tagRef.EntityAddress.GetComponentRef<TController>(),
            };
        }


        protected abstract TComponent CreateStatusComponent(TData data, in StatusContext context);
        protected abstract TController CreateStatusController(TData data, in StatusContext context);
        protected abstract void AddSignature(EntityAddress address);

        protected virtual void OnApply(TData data, in EntityStatusInfos statusInfos, in StatusContext context)
        {

        }

        protected virtual void OnRemove(TData data, in EntityStatusInfos statusInfos, in StatusContext context)
        {

        }

        protected virtual void OnStack(TData data, in EntityStatusInfos statusInfos, in StatusContext context)
        {

        }

        protected virtual void OnTick(TData data, in EntityStatusInfos statusInfos, in StatusContext context)
        {

        }

    }
}