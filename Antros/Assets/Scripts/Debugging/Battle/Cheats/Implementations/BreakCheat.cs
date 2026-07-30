using ATCG.Debugging.Cheats;
using UnityEngine;

namespace ATCG.Debugging.Debugging.Battle
{
	[CheatGroup("Debug")]
	public class BreakCheat : ICheat
	{
		public string Name => "Break Cheat";
		public string Description => "Break Cheat";
		public async Awaitable Execute(CheatContext context)
		{
			await Awaitable.MainThreadAsync();
			Debug.Break();
		}
	}
}