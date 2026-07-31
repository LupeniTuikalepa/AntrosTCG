using System.Collections.Generic;
using ATCG.Debugging.Debugging.Battle;

namespace ATCG.Debugging.Cheats
{
    /// <summary>
    /// Global, player-agnostic cheats (grouped under a "System" section).
    /// </summary>
    public class SystemProvider : CheatProvider
    {
        public override IEnumerable<CheatSection> GetSections()
        {
            yield return new CheatSection("System", new ICheat[] { new BreakCheat() });
        }
    }
}
