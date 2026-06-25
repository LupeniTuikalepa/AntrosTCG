using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Battle.Players.Runtime;
using Cheats.Core;
using UnityEngine;

namespace ATCG.Debugging.Debugging.Battle
{
    public class PlayerProvider : CheatProvider
    { 
		[SerializeField] private RuntimeLocalBattlePlayer player;
		
		public override IEnumerable<ICheat> GetCheats()
		{
			yield return new AddHealthCheat(player);
			yield return new RemoveHealthCheat(player);
			yield return new AddManaCheat(player);
			yield return new RemoveMana(player);
			yield return new BreakCheat();
		}
	    
    }
}
