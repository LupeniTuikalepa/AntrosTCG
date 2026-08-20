using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Metrics;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class ImpactDamageCommand : EntityCommand<NoInfos>
    {
        private readonly EntityAddress other;

        public ImpactDamageCommand(EntityAddress address, EntityAddress other, string source = DEFAULT_SOURCE) : base(address, source)
        {
            this.other = other;
        }

        protected override void Process(in CommandContext context)
        {
            var collisionsDamage = GameMetrics.Current.CollisionsDamage;
            
            var ownerDamageCommand = new DamageCommand(collisionsDamage, Target.ToAddress(context.World));
            var otherDamageCommand = new DamageCommand(collisionsDamage, other);
            
            Inject(context, ownerDamageCommand);
            Inject(context, otherDamageCommand);
        }
    }
}