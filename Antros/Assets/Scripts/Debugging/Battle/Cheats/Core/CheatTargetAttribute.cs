using System;

namespace ATCG.Debugging.Cheats
{
    /// <summary>
    /// Marks an <c>EntityAddress</c> field as a target picked from a dropdown in the editor Cheats
    /// tool. <see cref="CandidatesMethod"/> names an instance method on the same cheat returning
    /// <c>IEnumerable&lt;CheatTargetOption&gt;</c> (label + address) — typically built with
    /// <c>CheatUtilities.EnumerateTargets&lt;TComponent&gt;(player)</c>. The cheat decides which
    /// entities are eligible, so the tool needs no knowledge of the world.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class CheatTargetAttribute : Attribute
    {
        /// <summary>Name of the instance method supplying the candidate targets.</summary>
        public string CandidatesMethod { get; }

        /// <summary>Optional display label (defaults to the nicified field name).</summary>
        public string Label { get; set; }

        /// <summary>Optional tooltip shown on the control.</summary>
        public string Tooltip { get; set; }

        public CheatTargetAttribute(string candidatesMethod) => CandidatesMethod = candidatesMethod;
    }
}
