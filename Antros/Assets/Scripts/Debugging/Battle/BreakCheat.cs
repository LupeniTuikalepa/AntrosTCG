using Cheats.Core;
using UnityEngine;

namespace ATCG.Debugging.Debugging.Battle
{
	public class BreakCheat : ICheat
	{
		public string Name => "Break Cheat";
		public string Description => "Break Cheat";
		public void Execute(in CheatContext context)
		{
			Debug.Break();
		}
	}
}