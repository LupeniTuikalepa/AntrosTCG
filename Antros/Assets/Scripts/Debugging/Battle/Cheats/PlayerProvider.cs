using System.Collections.Generic;
using System.Linq;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Debugging.Debugging.Battle;
using ATCG.Debugging.Debugging.Battle.Cheats.Implementations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ATCG.Debugging.Cheats
{
    /// <summary>
    /// The single player-facing cheat provider. It isn't a scene component: it finds the live
    /// RuntimeLocalBattlePlayers itself and exposes one section per player ("Player 1", "Player 2",
    /// … — override <see cref="SectionName"/> to rename). Each section carries that player's cheats.
    /// </summary>
    public class PlayerProvider : CheatProvider
    {
        public override IEnumerable<CheatSection> GetSections()
        {
            RuntimeLocalBattlePlayer[] players =
                Object.FindObjectsByType<RuntimeLocalBattlePlayer>(FindObjectsSortMode.None);

            List<RuntimeLocalBattlePlayer> live = players
                .Where(p => p != null && p.BattlePlayer != null)
                .OrderBy(p => p.LocalID)
                .ToList();

            if (live.Count == 0)
            {
                // No live player: still expose the cheats (disabled) so they stay discoverable.
                yield return new CheatSection("Player", BuildCheats(null), enabled: false);
                yield break;
            }

            for (int index = 0; index < live.Count; index++)
                yield return new CheatSection(SectionName(index, live[index]), BuildCheats(live[index]));
        }

        /// <summary>Section header for a player. Override to customise (e.g. use the player's name).</summary>
        protected virtual string SectionName(int index, RuntimeLocalBattlePlayer player) => $"Player {index + 1}";

        // Only cheats that act on the player itself. Entity-targeted cheats live in EntityCheatProvider.
        private static IEnumerable<ICheat> BuildCheats(RuntimeLocalBattlePlayer runtime)
        {
            yield return new AddHealthCheat(runtime);
            yield return new RemoveHealthCheat(runtime);
            yield return new FullHealPlayerCheat(runtime);
            yield return new SetPlayerHealthCheat(runtime);
            yield return new AddManaCheat(runtime);
            yield return new RemoveManaCheat(runtime);
            yield return new FullManaCheat(runtime);
            yield return new SetPlayerManaCheat(runtime);
        }
    }
}
