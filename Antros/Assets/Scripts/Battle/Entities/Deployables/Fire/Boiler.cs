using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Construction;
using ATCG.Capacities.Fire;

namespace ATCG.Battle.Entities.Deployables.Fire
{
    public partial struct Boiler : IConstruction<BoilerData>
    {
        public void SetupEntity(BoilerData data, ConstructionAspect aspect)
        {
        }
    }
}