using Cheats.Core;
using UnityEngine;

namespace Cheats.Samples.Samples
{
	public class SampleRotationCheat : ICheat
	{
		private readonly GameObject espion;
		public string Name { get; }
		public string Description { get; }
		
		
		public SampleRotationCheat(GameObject espion)
		{
			Name = nameof(SampleRotationCheat);
			Description = "Change la taille de la victim";
			this.espion =  espion;
		}
		
		public async Awaitable Execute(CheatContext context)
		{
			espion.transform.localRotation = new Quaternion(10,30,20,40);
			await Awaitable.MainThreadAsync();
			
		}
	}
}