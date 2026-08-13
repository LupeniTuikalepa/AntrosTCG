using System.Collections.Generic;

namespace ATCG.Cutscenes
{
    /// <summary>
    /// A simple, open keyed/typed property store backing a cutscene context: whatever a consumer
    /// injects (source actor, screen player, per-run values…) is read back by the cutscene elements
    /// through the same keys. Unlike the capacity property bag this one is not a closed schema — the
    /// generic player doesn't own a declaration list, so any key can be written.
    /// </summary>
    public sealed class CutscenePropertyBag
    {
        private readonly Dictionary<string, object> values = new();

        public void Set<T>(string name, T value) => values[name] = value;

        public bool TryGet<T>(string name, out T value)
        {
            if (values.TryGetValue(name, out object boxed) && boxed is T typed)
            {
                value = typed;
                return true;
            }
            value = default;
            return false;
        }

        public bool Has(string name) => values.ContainsKey(name);

        public void Clear() => values.Clear();
    }
}
