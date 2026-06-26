using System;
using System.Collections.Generic;
using ATCG.Battle.Cards;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Battle.Players.Runtime;
using ATCG.Debugging.Debugging.Battle.Cheats.Implementations;
using Cheats.Core;
using UnityEngine;

namespace ATCG.Debugging.Debugging.Battle
{
    public class PlayerProvider : CheatProvider
    { 
		[SerializeField] 
		private RuntimeLocalBattlePlayer player;
		
		public override IEnumerable<ICheat> GetCheats()
		{
			yield return new StatusApplyCheat(player.BattlePlayer);
			yield return new StatusRemoveCheat(player.BattlePlayer);
			yield return new StatusAllCheat(player.BattlePlayer);
			yield return new KillEntityCheat(player.BattlePlayer);
			yield return new TeleportEntityCheat(player.BattlePlayer);
			yield return new AddHealthCheat(player);
			yield return new RemoveHealthCheat(player);
			yield return new AddManaCheat(player);
			yield return new RemoveManaCheat(player);
			yield return new BreakCheat();
		}
	    
    }
}
