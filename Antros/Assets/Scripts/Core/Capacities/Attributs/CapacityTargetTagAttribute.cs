using System;

namespace ATCG.Capacities.Attributs
{
    /// <summary>
    /// Marks a <c>public const string</c> on a capacity Data class as a target tag, e.g.
    /// <code>[CapacityTargetTag] public const string FROZEN = nameof(FROZEN);</code>
    /// Tags are applied to targets inside GetTargets and queried back in steps via
    /// <c>ctx.Targets.WithTags(...)</c>. The capacity editor discovers, adds and removes
    /// these by this attribute (it is what distinguishes a tag const from any other const).
    /// The base tags Cell/Member live in <see cref="ATCG.Capacities.CapacityTags"/> and are
    /// always available without declaring them.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public sealed class CapacityTargetTagAttribute : Attribute
    {
    }
}
