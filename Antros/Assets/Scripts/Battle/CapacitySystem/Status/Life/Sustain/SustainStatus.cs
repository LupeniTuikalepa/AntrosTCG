using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status.Controllers;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Capacities.Data.Status.Life;
using ATCG.Cards.Implementations;

namespace ATCG.Battle.CapacitySystem.Status.Life.Sustain
{
	public partial class SustainStatus : Status<SustainStatusData, SustainStatusComponent,StatusDurationController>
	{
		protected override SustainStatusComponent CreateStatusComponent(SustainStatusData data, in StatusContext context)
		{
			SustainStatusComponent sustainStatusComponent = new SustainStatusComponent();
			return sustainStatusComponent;
		}

		protected override StatusDurationController CreateStatusController(SustainStatusData data, in StatusContext context)
		{
			return new StatusDurationController(data.Duration);
		}
		protected override void OnApply(SustainStatusData data, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnApply(data, in statusInfos, in context);
			ref var statusComponent = ref statusInfos.statusComponentRef.GetValue();
			statusComponent.Watch(statusInfos.targetAddress);
		}

		protected override void OnRemove(SustainStatusData data, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnRemove(data, in statusInfos, in context);
			
		}
	}
}