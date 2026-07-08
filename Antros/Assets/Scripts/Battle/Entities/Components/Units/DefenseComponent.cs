using System.Collections.Generic;
using Helteix.ChanneledProperties.Formulas;
using UnityEngine;

namespace ATCG.Battle.Entities.Components
{
	public readonly struct DefenseComponent : IEntityComponent
	{
		public readonly Formula<float> defense;
		public int Defense => Mathf.FloorToInt(defense.Value);
		public DefenseComponent(int baseDefense)
		{
			this.defense = new Formula<float>(baseDefense);
		}
	}
}