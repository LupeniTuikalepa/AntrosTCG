using System;

namespace ATCG.Debugging.Cheats
{
    /// <summary>
    /// Groups a cheat under a named sub-section within its provider, in the editor Cheats tool.
    /// Put it above a cheat class, e.g.
    /// <code>[CheatGroup("Health")] public class AddHealthCheat : ICheat</code>
    /// Cheats without it fall under a default "General" group.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class CheatGroupAttribute : Attribute
    {
        public string Group { get; }

        public CheatGroupAttribute(string group) => Group = group;
    }
}
