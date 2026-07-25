using ATCG.Battle.Entities;
using ATCG.Metrics;

namespace ATCG.Battle.Players.Local.Phases
{
    /// <summary>
    /// Refines the base highlight state a phase computes for an entity — e.g. a movement phase
    /// splits the generic "selectable" into Preview1 (direct ring) / Preview2 (reachable) using
    /// its own data. Return <paramref name="fallback"/> to keep the default.
    /// </summary>
    public delegate HighlightState HighlightClassifier(EntityAddress address, HighlightState fallback);
}
