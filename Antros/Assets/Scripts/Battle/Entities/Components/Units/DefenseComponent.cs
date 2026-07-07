using System.Collections.Generic;
using UnityEngine;

namespace ATCG.Battle.Entities.Components
{
	public class DefenseComponent : IEntityComponent
	{
		[SerializeField] private int defense;
		private readonly List<DefenseModifier> temporaryModifiers = new List<DefenseModifier>();

		public int Defense => defense;

		public int TotalDefense
		{
			get
			{
				int total = defense;
				for (int i = 0; i < temporaryModifiers.Count; i++)
				{
					total += temporaryModifiers[i].value;
				}
				return Mathf.Max(0, total); 
			}
		}

		public void AddModifier(DefenseModifier modifier) => temporaryModifiers.Add(modifier);
		public void RemoveModifier(DefenseModifier modifier) => temporaryModifiers.Remove(modifier);
	}

	public struct DefenseModifier
	{
		public int value;
		public string sourceDescription;
	}
}