using System;
using ATCG.Capacities.Data;
using UnityEngine;

namespace ATCG.Battle.Grids.Patterns.Building
{
	[Serializable]
	public struct PatternGroup
	{
		[field: SerializeReference] public PatternData [] Data { get; private set; }
	}
}