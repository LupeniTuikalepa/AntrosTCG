using ATCG.Battle.Entities.Aspects;
using ATCG.Capacities;
using ATCG.Construction;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.Entities.Construction
{
    [GenerateContainer]
    public interface IConstruction<in TConstructionData > : IBehaviour<ConstructionData> where TConstructionData : ConstructionData
    {
        [AddToContainer]
        void SetupEntity(TConstructionData data, ConstructionAspect aspect); 
    }
}