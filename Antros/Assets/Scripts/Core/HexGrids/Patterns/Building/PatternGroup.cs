using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.HexGrids.Patterns.Building
{
	[Serializable, InlineProperty]
	public struct PatternGroup
	{

		[field: SerializeReference, ListDrawerSettings(ShowFoldout = false)]
		public PatternData [] Data { get; private set; }

		public bool IsEmpty => Data == null || Data.Length == 0;
		public PatternGroup(params PatternData[] data)
		{
			Data = data;
		}
	}
}