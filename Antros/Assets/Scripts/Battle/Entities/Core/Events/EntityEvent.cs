using System;
using System.Collections.Generic;
using ATCG.Battle.Entities.Core;
using UnityEngine.Pool;

namespace ATCG.Battle.Timelines
{
    [Serializable]
    public abstract class EntityEvent : IDisposable
    {
        public EntityEvent Parent { get; }
        public EntityAddress[] TargetedEntities { get; private set; }
        public EntityAddress TargetEntity => TargetedEntities[0];

        private readonly List<EntityEvent> subEvents;
        
        
        protected EntityEvent(EntityEvent parent, params EntityAddress[] targetedEntities)
        {
            subEvents = ListPool<EntityEvent>.Get();
            
            Parent = parent;
            Parent?.subEvents.Add(this);
            
            TargetedEntities = targetedEntities;
        }


        public void Dispose()
        {
            ListPool<EntityEvent>.Release(subEvents);
            foreach (EntityEvent subEvent in subEvents)
                subEvent.Dispose();
        }

        public abstract void Apply();

        public IEnumerable<EntityEvent> GetChildren()
        {
            foreach (EntityEvent subEvent in subEvents)
            {
                yield return subEvent;
                
                foreach (EntityEvent embed in subEvent.GetChildren())
                    yield return embed;
            }
        }

        public IEnumerable<T> GetChildren<T>() where T : EntityEvent
        {
            foreach (EntityEvent subEvent in GetChildren())
            {
                if (subEvent is T t)
                    yield return t;
            }
        }

        public bool HasAnyChildrenOfType<T>(out T firstFound)
        {
            foreach (EntityEvent subEvent in GetChildren())
            {
                if (subEvent is T t)
                {
                    firstFound = t;
                    return true;
                }
            }

            firstFound = default;
            return false;
        }
        
        public bool HasAnyAncestorOfType<T>(out T firstFound) where T : EntityEvent
        {
            foreach (var e in GetAncestors())
            {
                if (e is T t)
                {
                    firstFound = t;
                    return true;
                }
            }
            
            firstFound = null;
            return false;
        }
        
        public IEnumerable<T> GetAncestorsOfType<T>() where T : EntityEvent
        {
            foreach (EntityEvent entityEvent in GetAncestors())
            {
                if (entityEvent is T t)
                    yield return t;
            }
        }
        
        public IEnumerable<EntityEvent> GetAncestors()
        {
            EntityEvent parent = Parent;
            while (parent != null)
            {
                yield return parent;
                parent = Parent.Parent;
            }
        }
    }
}