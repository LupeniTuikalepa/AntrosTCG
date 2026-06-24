using System.Collections.Generic;
using Cheats.Samples.Samples;
using UnityEngine;

namespace Cheats.Core.Resources.Cheats
{
	public class SampleColorCheat : ICheat
	{
		private readonly GameObject espion;
		public string Name { get; }
		public string Description { get; }
		
		public SampleColorCheat(GameObject espion)
		{
			Name = nameof(SampleSizeCheat);
			Description = "Change la Couleur";
			this.espion = espion;
			
		}
		
		public void Execute(in CheatContext context)
		{
			espion.gameObject.GetComponent<Renderer>().material.color = Color.red;
		}
	}
}