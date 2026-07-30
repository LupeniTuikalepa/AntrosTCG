using UnityEngine;

namespace ATCG.Debugging.Cheats
{
    /// <summary>
    /// A single runtime cheat action. A <see cref="CheatProvider"/> exposes these; the ATCG
    /// editor's Cheats tool discovers them, groups them, and runs <see cref="Execute"/> while
    /// in Play mode.
    /// </summary>
    public interface ICheat
    {
        string Name { get; }
        string Description { get; }
        Awaitable Execute(CheatContext context);
    }
}
