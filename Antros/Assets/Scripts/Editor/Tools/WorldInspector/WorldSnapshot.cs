using System.Collections.Generic;

namespace ATCG.Battle.Entities.EditorTools
{
    /// <summary>
    /// Frozen list of entity ids captured at an explicit refresh. The inspector reads
    /// from a snapshot rather than from the live world span so that, between snapshots,
    /// the UI is stable: a foldout the user closed stays closed, and the entity list
    /// doesn't reshuffle under the cursor. Live component values are still read on demand
    /// when an entity is selected, so a snapshot doesn't go stale in a misleading way.
    ///
    /// The generation of each id is captured alongside it. Entity ids are sparse-set
    /// indices that the world RECYCLES (CreateEntity pops a freed id and bumps its
    /// generation), so an index alone does not identify an entity across time: a
    /// destroyed slot re-occupied by a new entity carries the same index but a higher
    /// generation. Consumers must compare (id, generation), not the bare id, or a
    /// recycled slot looks unchanged and the view freezes on the dead entity.
    /// </summary>
    public sealed class WorldSnapshot
    {
        private readonly List<int> entityIds = new();
        private readonly Dictionary<int, int> generationById = new();

        public IReadOnlyList<int> EntityIds => entityIds;
        public int Count => entityIds.Count;
        public bool HasData { get; private set; }

        /// <summary>Generation captured for <paramref name="id"/>, or -1 if the id is not in this snapshot.</summary>
        public int GenerationOf(int id) => generationById.TryGetValue(id, out int gen) ? gen : -1;

        public void Capture(World world)
        {
            entityIds.Clear();
            generationById.Clear();
            HasData = false;

            if (world == null)
                return;

            System.ReadOnlySpan<int> ids = world.Entities;
            for (int i = 0; i < ids.Length; i++)
            {
                int id = ids[i];
                entityIds.Add(id);
                generationById[id] = world.GetGeneration(id);
            }

            HasData = true;
        }

        public void Clear()
        {
            entityIds.Clear();
            generationById.Clear();
            HasData = false;
        }
    }
}
