using System.Collections.Generic;
using ATCG.Battle.Players.Local.Runtime;
using Cheats.Core;
using Cheats.Core.Resources.Cheats;
using UnityEngine;

namespace Cheats.Samples.Samples
{
	public class SampleCheatProvider : CheatProvider
	{
		[SerializeField] private GameObject victim;
		public override IEnumerable<ICheat> GetCheats()
		{
			yield return new SampleColorCheat(victim);
			yield return new SampleSizeCheat(victim);
			yield return new SampleRotationCheat(victim);
			
			yield return new BreakCheat();
		}
		
	}
}