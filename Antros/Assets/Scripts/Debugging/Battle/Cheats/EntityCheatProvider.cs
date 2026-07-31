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
    /// Cheats that act on entities in the battle rather than on a specific player (damage, kill,
    /// status, teleport). Targets are picked from the whole world, so these live in a single
    /// "Entities" section, grouped by kind (Combat / Status / Movement). Any live player is used
    /// only as an entry point to the shared battle world.
    /// </summary>
    public class EntityCheatProvider : CheatProvider
    {
        public override IEnumerable<CheatSection> GetSections()
        {
            LocalBattlePlayer player = Object
                .FindObjectsByType<RuntimeLocalBattlePlayer>()
                .Where(p => p != null && p.BattlePlayer != null)
                .OrderBy(p => p.LocalID)
                .Select(p => p.BattlePlayer)
                .FirstOrDefault();

            yield return new CheatSection("Entities", BuildCheats(player), enabled: player != null);
        }

        private static IEnumerable<ICheat> BuildCheats(LocalBattlePlayer player)
        {
            yield return new DamageCheat(player);
            yield return new KillEntityCheat(player);
            yield return new KillAllCheat(player);
            yield return new HealEntityCheat(player);
            yield return new FullHealEntityCheat(player);
            yield return new SetEntityHealthCheat(player);
            yield return new StatusApplyCheat(player);
            yield return new StatusRemoveCheat(player);
            yield return new TeleportEntityCheat(player);
        }
    }
}