using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Entities;
using ATCG.Metrics;

namespace ATCG.Battle.Commands.EntityCommands
{
	public class BasicAttackCommand : EntityCommand<BasicAttackCommand.Infos>
	{
		public struct Infos
		{

		}
		private readonly int strength;
		private readonly Entity victim;

		public BasicAttackCommand(EntityAddress sourceEntity, Entity victim, int strength) : base(sourceEntity)
		{
			this.strength = strength;
			this.victim = victim;
		}

		protected override void Process(in CommandContext context)
		{
			DamageCommand command = new DamageCommand(strength, victim);
			Embed(in context, command);
		}
	}
}