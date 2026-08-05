using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Commands;
using ATCG.Battle.Entities.Components;
using ATCG.Capacities.Fire;
using ATCG.Enums;

namespace ATCG.Battle.Entities.Deployables.Fire
{
    public partial struct WillOWisp : IDeployable<WillOWispData>
    {
        public void SetupEntity(WillOWispData data, DeployableAspect aspect)
        {
            aspect.EntityAddress.AddOrSetComponent(new HealthComponent(data.Health));
            aspect.EntityAddress.ListenForEntityCommand<MoveCommand>(DropFlame);
        }

        private void DropFlame(in CommandContext context, in MoveCommand command)
        {
            
        }
    }
}