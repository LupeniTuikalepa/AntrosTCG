using ATCG.Battle.Entities.Aspects;
using ATCG.Capacities;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.Entities.Deployables
{
    [GenerateContainer]
    public interface IDeployable<in TDeployableData > : IBehaviour<DeployableData> where TDeployableData : DeployableData
    {
        [AddToContainer]
        void SetupEntity(TDeployableData data, DeployableAspect aspect);
    }
}