using System.Collections.Generic;

namespace ATCG.Battle.Entities.EditorTools
{
    /// <summary>
    /// Decides which entities pass the active filters. Pure logic, no UI.
    ///
    /// Two independent filters, combined with AND:
    ///   - Component filter: entity must have ALL selected components.
    ///   - Aspect filter: entity must match the selected aspect (its generated IsAspect).
    /// An empty filter passes everything.
    /// </summary>
    public sealed class EntityFilter
    {
        private readonly HashSet<int> requiredComponentIds = new();
        private AspectCatalog.Entry aspect;

        public IReadOnlyCollection<int> RequiredComponentIds => requiredComponentIds;
        public AspectCatalog.Entry Aspect => aspect;
        public bool HasAspect => aspect != null;
        public bool IsEmpty => requiredComponentIds.Count == 0 && aspect == null;

        public void ToggleComponent(int id)
        {
            if (!requiredComponentIds.Add(id))
                requiredComponentIds.Remove(id);
        }

        public void RemoveComponent(int id) => requiredComponentIds.Remove(id);

        public bool IsComponentSelected(int id) => requiredComponentIds.Contains(id);

        public void ClearComponents() => requiredComponentIds.Clear();

        public void SetAspect(AspectCatalog.Entry entry) => aspect = entry;

        public void ClearAspect() => aspect = null;

        public void ClearAll()
        {
            ClearComponents();
            ClearAspect();
        }

        /// <summary>True if the entity passes every active filter.</summary>
        public bool Passes(World world, int entityId)
        {
            if (IsEmpty)
                return true;

            Entity entity = new(entityId);
            if (!world.IsAlive(entity))
                return false;

            EntityMeta meta = world.GetMeta(entity);

            foreach (int id in requiredComponentIds)
            {
                if (!meta.HasComponent(id))
                    return false;
            }

            if (aspect != null)
            {
                EntityAddress address = new(world, entity);
                if (!aspect.Matches(in address))
                    return false;
            }

            return true;
        }
    }
}
