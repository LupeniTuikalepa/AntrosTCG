using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Commands.EntityCommands
{
	public class FallCommand : EntityCommand<NoInfos>
	{
		public FallCommand(EntityAddress address) : base(address)
		{
			
		}

		protected override void Process(in CommandContext context)
		{
			EntityAddress address = TargetEntityAddress(context.World);
			
			if (address.TryGetComponentRO(out GridMemberComponent gridMemberComponent))
			{
				if (!gridMemberComponent.grid.TryGetBattleCell(gridMemberComponent.coordinates, out _))
				{
					Inject(in context, new DeathCommand(address));
				}
			}
		}
	}
}