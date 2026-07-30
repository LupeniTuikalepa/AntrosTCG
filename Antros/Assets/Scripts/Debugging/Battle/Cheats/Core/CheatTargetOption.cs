using ATCG.Battle.Entities;

namespace ATCG.Debugging.Cheats
{
    /// <summary>
    /// One entry in a <see cref="CheatTargetAttribute"/> dropdown: a human-readable label paired
    /// with the entity it selects.
    /// </summary>
    public readonly struct CheatTargetOption
    {
        public readonly string Label;
        public readonly EntityAddress Address;

        public CheatTargetOption(string label, EntityAddress address)
        {
            Label = label;
            Address = address;
        }
    }
}
