using System;

namespace ATCG.Battle.Entities.Components
{
    public interface IComponentStore
    {
        void Add(int entityId);

        void Remove(int entityId);
        bool Has(int entityID);

        // --- Non-generic introspection (editor / debug tooling) ---

        /// <summary>Number of components currently stored.</summary>
        int Count { get; }

        /// <summary>The component element type .</summary>
        Type ComponentType { get; }

        /// <summary>Entity IDs that own a component, contiguous, length == Count.</summary>
        ReadOnlySpan<int> AllEntities { get; }

        /// <summary>
        /// Boxed copy of the component for the given entity, or null if absent.
        /// Editor-only convenience: boxes a struct, so never call this on a hot path.
        /// </summary>
        object GetBoxed(int entityId);
    }
}