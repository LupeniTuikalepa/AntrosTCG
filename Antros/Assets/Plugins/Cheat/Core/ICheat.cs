using ATCG.Battle.Players.Local;
using UnityEngine;

namespace Cheats.Core
{
	public interface ICheat 
	{
		public string Name { get; }
		public string Description { get; }
		
		public Awaitable Execute( CheatContext context);
		
	}
}
