using ATCG.Battle.CapacitySystem.Core.Setup;
using ATCG.Battle.CapacitySystem.Core.Setup.SelectCapacities;
using ATCG.Capacities;
using ATCG.Capacities.Setup;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.CapacitySystem.Capacities.Setup
{
	public partial struct SeeAllCapacitySetup : ICapacitySetup<SeeAllCapacitySetupData>
	{
		public const string SEE_ALL_CAPACITY = nameof(SeeAllCapacitySetup);
		public async Awaitable<bool> Execute(SeeAllCapacitySetupData data, CapacitySetupContext context)
		{
			using (HashSetPool<CapacityData>.Get(out var list))
			{
				foreach (CapacityData capacity in  GameController.GameDatabase.GetAll<CapacityData>())
				{
					list.Add(capacity);
				}

				var result = await new SelectCapacitiesPhase(context.player, list);
				
				if(result.value == null)
					return false;

				context.castCapacityPhase.InjectProperty(SEE_ALL_CAPACITY, result.value);

				return true;
			}
		}
	}


}