using System.Collections.Generic;
using Cheats.Samples.Samples;
using UnityEngine;

namespace Cheats.Core.Resources.Cheats
{
	public class SampleSizeCheat : ICheat
	{
		private readonly GameObject espion;
		public string Name { get; }
		public string Description { get; }
		
		public SampleSizeCheat(GameObject espion)
		{
			Name = nameof(SampleSizeCheat);
			Description = "Change la taille de la victim";
			this.espion = espion;
			
		}
		
		public void Execute(in CheatContext context)
		{
			Debug.Log($"{Name}: {Description}");
			espion.transform.localScale = new Vector3(10, 10, 10);
		}
	}
}