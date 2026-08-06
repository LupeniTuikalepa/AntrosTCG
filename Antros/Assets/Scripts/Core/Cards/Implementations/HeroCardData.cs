using ATCG.Capacities.Data;
using ATCG.Enums;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Cards.Implementations
{
	[CreateAssetMenu(fileName = "GameCardData", menuName = "ATCG/Cards/Hero")]
	public class HeroCardData : GameCardData
	{
		[field: SerializeField, Range(1, 100), BoxGroup("Heroes")]
		public int Health { get; private set; } = 3;

		[field: SerializeField, Range(1, 10), BoxGroup("Heroes")]
		public int Strength { get; private set; } = 1;

		[field: SerializeField, Range(1, 10), BoxGroup("Heroes")]
		public int Defense { get; private set; } = 1;
		
		[field: SerializeField, Range(1, 5), BoxGroup("Movement")]
		public int Speed { get; private set; } = 1;

		[field: SerializeField, BoxGroup("Movement")]
		public MovementType MovementType { get; private set; }

		[field: SerializeField, Range(0, 10), BoxGroup("Deploy")]
		public int DeployRange { get; private set; } = 1;
		
		[field: SerializeField, BoxGroup("Prefabs")]
		public GameObject HeroPawnPrefab { get; private set; }

		protected override void Reset()
		{
			base.Reset();
			Health = 1;
			Strength = 1;
			Defense = 1;
			Speed = 1;
			DeployRange = 1;
		}
	}
}