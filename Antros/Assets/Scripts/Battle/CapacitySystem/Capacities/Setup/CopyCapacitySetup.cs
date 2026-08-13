using ATCG.Battle.CapacitySystem.Core.Setup;
using ATCG.Battle.CapacitySystem.Core.Setup.SelectCapacities;
using ATCG.Battle.Entities.Components;
using ATCG.Capacities;
using ATCG.Capacities.Setup;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.CapacitySystem.Capacities.Setup
{
	public partial struct CopyCapacitySetup : ICapacitySetup<CopyCapacitySetupData>
	{
		public const string COPIED_CAPACITY = nameof(CopyCapacitySetup);
		
		public async Awaitable<bool> Execute(CopyCapacitySetupData data, CapacitySetupContext context)
		{
			using (HashSetPool<CapacityData>.Get(out var list))
			{
				foreach (var capacityTarget in context.targets)
				{
					if (!capacityTarget.TryGetComponentRO(out CapacityCasterComponent capacityCasterComponent))
						continue;
					foreach (var capacityData in capacityCasterComponent.capacities)
					{
						list.Add(capacityData);
					}
				}
				var result = await new SelectCapacitiesPhase(context.player,list);
				if (result.value == null)
					return false;
				context.castCapacityPhase.InjectProperty(COPIED_CAPACITY,  result.value);
				
				return true;
			}
		}
	}
}