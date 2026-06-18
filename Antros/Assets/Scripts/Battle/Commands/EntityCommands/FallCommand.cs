using System.Collections.Generic;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids;
using ATCG.Battle.Players.Local;
using ATCG.HexGrids;
using PrimeTween;
using UnityEngine;

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
					Embed(in context, new DeathCommand(address));
				}
			}
		}
	}
}