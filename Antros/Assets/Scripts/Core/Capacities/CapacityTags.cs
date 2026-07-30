namespace ATCG.Capacities
{
    /// <summary>
    /// Base tags shared by every capacity's default targeting. A capacity that needs
    /// finer separations declares extra <c>public const string</c> tags on its own Data
    /// class (managed from the capacity editor) and applies them in its GetTargets
    /// override. Targets are queried back in steps via <c>ctx.Targets.WithTags(...)</c>.
    /// </summary>
    public static class CapacityTags
    {
        /// <summary>The hit cell itself.</summary>
        public const string CELL = nameof(CELL);

        /// <summary>An entity standing on a hit cell.</summary>
        public const string MEMBER = nameof(MEMBER);
    }
}
