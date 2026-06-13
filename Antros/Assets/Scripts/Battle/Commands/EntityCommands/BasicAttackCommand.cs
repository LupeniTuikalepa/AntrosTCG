using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Entities;
using ATCG.Metrics;

namespace ATCG.Battle.Commands.EntityCommands
{
	public class BasicAttackCommand : EntityCommand
	{
		private readonly int strength;
		private readonly Entity victime;

		public BasicAttackCommand(EntityAddress sourceEntity, Entity victime, int strength) : base(sourceEntity)
		{
			this.strength = strength;
			this.victime = victime;
		}

		protected override void Process(in GameCommandContext context)
		{
			var manaCost = new ModifyPlayerManaCommand(TargetEntity, GameMetrics.Current.BasicAttackCost);
			Embed(in context, manaCost);

			DamageCommand command = new DamageCommand(strength, victime);
			Embed(in context, command);
		}
	}
}