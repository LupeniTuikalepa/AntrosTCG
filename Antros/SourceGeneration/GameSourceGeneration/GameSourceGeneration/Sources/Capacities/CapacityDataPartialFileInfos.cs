using System;

namespace GameSourceGeneration.Capacities;

public sealed class CapacityDataPartialFileInfos : IEquatable<CapacityDataPartialFileInfos>
{
    public string className = "";
    public string? @namespace;
    public string[] steps = Array.Empty<string>();

    // Whether the target derives from CapacityData: only then do we emit the data-driven part
    // (per-step StepData fields + MapSteps). Any other CutsceneDefinition just gets DeclaredSteps.
    public bool isCapacity;

    public bool Equals(CapacityDataPartialFileInfos? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return className == other.className
               && @namespace == other.@namespace
               && isCapacity == other.isCapacity
               && steps.SequenceEqual(other.steps); // steps MUST be in equality
    }

    public override bool Equals(object? obj) => Equals(obj as CapacityDataPartialFileInfos);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = className.GetHashCode();
            hash = (hash * 397) ^ (@namespace?.GetHashCode() ?? 0);
            hash = (hash * 397) ^ isCapacity.GetHashCode();
            foreach (string s in steps)
                hash = (hash * 397) ^ (s?.GetHashCode() ?? 0);
            return hash;
        }
    }
}