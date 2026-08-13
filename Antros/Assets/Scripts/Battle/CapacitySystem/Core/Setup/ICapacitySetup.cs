using ATCG.Capacities.Setup;
using Helteix.Tools.DataMapping;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Core.Setup
{
	[GenerateContainer]
	public interface ICapacitySetup<in T>: IBehaviour<T> where T: CapacitySetupData
	{
		[AddToContainer]
		Awaitable <bool> Execute(T data, CapacitySetupContext context);
	}
}