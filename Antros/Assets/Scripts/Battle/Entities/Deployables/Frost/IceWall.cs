using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Capacities.Frost;

namespace ATCG.Battle.Entities.Deployables.Frost
{
    public partial struct IceWall : IDeployable<IceWallData>
    {
        public void SetupEntity(IceWallData data, DeployableAspect aspect)
        {
            aspect.EntityAddress.AddOrSetComponent(new HealthComponent(data.Health));
        }
    }
}