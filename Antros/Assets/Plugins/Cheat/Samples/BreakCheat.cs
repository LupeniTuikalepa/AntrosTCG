using System.Collections.Generic;
using UnityEngine;

namespace Cheats.Core.Resources.Cheats
{
	public class BreakCheat : ICheat
	{
		public string Name => "Break Cheat";
		public string Description => "Break Cheat";
		
		public async Awaitable Execute(CheatContext context)
		{
			Debug.Break();
			await Awaitable.MainThreadAsync();
		}
	}
}