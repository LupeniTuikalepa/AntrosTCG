using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components.Implementations;
using ATCG.Metrics;

namespace ATCG.Battle.Commands.EntityCommands
{
	public class BasicAttackCommand : EntityCommand<BasicAttackCommand.Infos>
	{
		public struct Infos : ICommandInfos
		{
			public int strength;
			public Entity victim;
			public Entity source;
		}
		
		private readonly int strength;
		private readonly Entity victim;

		public BasicAttackCommand(EntityAddress address, EntityAddress victim, int strength) : base(address)
		{
			this.strength = strength;
			this.victim = victim;
			
			infos.strength = strength;
			infos.victim = victim;
			infos.source = address;
		}

		protected override void Process(in CommandContext context)
		{
			DamageCommand command = new DamageCommand(strength, victim.ToAddress(context.World));
			Embed(in context, command);
		}
	}
}