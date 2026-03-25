using System;
using System.Collections.Generic;
using ATCG.Battle.Entities.Core;
using UnityEngine.Pool;

namespace ATCG.Battle.Timelines
{
    [Serializable]
    public abstract class EntityEvent : IDisposable
    {
        public EntityEvent Root { get; private set; }
        public Entity[] TargetedEntities { get; private set; }

        public Entity TargetEntity => TargetedEntities[0];

        private readonly List<EntityEvent> subEvents;

        private EntityEvent(Entity[] targetedEntities)
        {
            TargetedEntities = targetedEntities;
            subEvents = ListPool<EntityEvent>.Get();
        }

        public void EnqueueSubEvent(EntityEvent entityEvent)
        {
            entityEvent.Root = this;
            subEvents.Add(entityEvent);
        }

        public void Dispose()
        {
            ListPool<EntityEvent>.Release(subEvents);
            foreach (EntityEvent subEvent in subEvents)
                subEvent.Dispose();
        }

        public abstract void Apply(World world);
    }
}