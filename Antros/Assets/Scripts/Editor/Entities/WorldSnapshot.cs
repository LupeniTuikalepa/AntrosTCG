using System.Collections.Generic;

namespace ATCG.Battle.Entities.EditorTools
{
    /// <summary>
    /// Frozen list of entity ids captured at an explicit refresh. The inspector reads
    /// from a snapshot rather than from the live world span so that, between snapshots,
    /// the UI is stable: a foldout the user closed stays closed, and the entity list
    /// doesn't reshuffle under the cursor. Live component values are still read on demand
    /// when an entity is selected, so a snapshot doesn't go stale in a misleading way.
    /// </summary>
    public sealed class WorldSnapshot
    {
        private readonly List<int> entityIds = new();

        public IReadOnlyList<int> EntityIds => entityIds;
        public int Count => entityIds.Count;
        public bool HasData { get; private set; }

        public void Capture(World world)
        {
            entityIds.Clear();
            HasData = false;

            if (world == null)
                return;

            System.ReadOnlySpan<int> ids = world.Entities;
            for (int i = 0; i < ids.Length; i++)
                entityIds.Add(ids[i]);

            HasData = true;
        }

        public void Clear()
        {
            entityIds.Clear();
            HasData = false;
        }
    }
}
