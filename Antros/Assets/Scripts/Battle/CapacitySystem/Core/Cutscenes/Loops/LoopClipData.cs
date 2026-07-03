using System;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Loops
{
    /// <summary>
    /// Thin data carried by a LoopClip: the name of the injected array property to
    /// iterate. The number of loop turns equals that array's length at runtime.
    /// </summary>
    [Serializable]
    public struct LoopClipData
    {
        public string propertyName;
    }
}