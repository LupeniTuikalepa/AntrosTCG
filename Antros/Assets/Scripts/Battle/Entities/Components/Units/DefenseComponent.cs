using System.Collections.Generic;
using ATCG.Utilities;
using Helteix.ChanneledProperties.Formulas;
using UnityEngine;

namespace ATCG.Battle.Entities.Components
{
	public readonly struct DefenseComponent : IEntityComponent
	{
		public readonly Formula<float> defense;
		public int Defense => GameMaths.Round(defense.Value);
		public DefenseComponent(int baseDefense)
		{
			this.defense = new Formula<float>(baseDefense);
		}
	}
}