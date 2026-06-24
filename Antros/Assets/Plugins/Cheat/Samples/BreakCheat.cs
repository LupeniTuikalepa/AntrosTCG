using System.Collections.Generic;
using UnityEngine;

namespace Cheats.Core.Resources.Cheats
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