using System.Collections;
using System.Collections.Generic;
using ATCG.Battle.Entities;

namespace ATCG.Battle.CapacitySystem.Core
{
    /// <summary>
    /// A per-cast collection of tagged targets. Capacities register EntityAddresses with
    /// one or more string tags (see <see cref="ATCG.Capacities.CapacityTags"/> plus any
    /// per-capacity data consts) from GetTargets; steps query them back by tag through
    /// <see cref="WithTags"/> (AND semantics). Registering the same address twice merges
    /// the tag sets, so a single entity can carry several tags.
    /// </summary>
    public sealed class CapacityTargets : IEnumerable<EntityAddress>
    {
        private sealed class Entry
        {
            public EntityAddress address;
            public readonly HashSet<string> tags = new HashSet<string>();
        }

        private readonly List<Entry> entries = new List<Entry>();
        private readonly Dictionary<EntityAddress, Entry> byAddress = new Dictionary<EntityAddress, Entry>();

        public int Count => entries.Count;

        /// <summary>
        /// Registers an address under the given tags. If the address is already present,
        /// the tags are added to its existing set.
        /// </summary>
        public void Add(EntityAddress address, params string[] tags)
        {
            if (!byAddress.TryGetValue(address, out Entry entry))
            {
                entry = new Entry { address = address };
                entries.Add(entry);
                byAddress.Add(address, entry);
            }

            if (tags == null)
                return;

            for (int i = 0; i < tags.Length; i++)
                if (!string.IsNullOrEmpty(tags[i]))
                    entry.tags.Add(tags[i]);
        }

        /// <summary>
        /// Every address carrying ALL of the given tags (AND). Passing no tag returns
        /// every registered address.
        /// </summary>
        public IEnumerable<EntityAddress> WithTags(params string[] tags)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];
                if (HasAll(entry, tags))
                    yield return entry.address;
            }
        }

        /// <summary>True if the address is registered and carries the given tag.</summary>
        public bool Has(EntityAddress address, string tag)
            => byAddress.TryGetValue(address, out Entry entry) && entry.tags.Contains(tag);

        public void Clear()
        {
            entries.Clear();
            byAddress.Clear();
        }

        private static bool HasAll(Entry entry, string[] tags)
        {
            if (tags == null || tags.Length == 0)
                return true;

            for (int i = 0; i < tags.Length; i++)
                if (!entry.tags.Contains(tags[i]))
                    return false;

            return true;
        }

        public IEnumerator<EntityAddress> GetEnumerator()
        {
            for (int i = 0; i < entries.Count; i++)
                yield return entries[i].address;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
