using System.Collections.Generic;
using System.Linq;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Debugging.Debugging.Battle;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ATCG.Debugging.Cheats
{
    /// <summary>
    /// Battle-wide cheats (turn flow, etc.) exposed in a single "Battle" section. Uses any live
    /// player only as an entry point to the battle.
    /// </summary>
    public class BattleCheatProvider : CheatProvider
    {
        public override IEnumerable<CheatSection> GetSections()
        {
            LocalBattlePlayer player = Object
                .FindObjectsByType<RuntimeLocalBattlePlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(p => p != null && p.BattlePlayer != null)
                .OrderBy(p => p.LocalID)
                .Select(p => p.BattlePlayer)
                .FirstOrDefault();

            yield return new CheatSection("Battle", BuildCheats(player), enabled: player != null);
        }

        private static IEnumerable<ICheat> BuildCheats(LocalBattlePlayer player)
        {
            yield return new EndTurnCheat(player);
        }
    }
}
