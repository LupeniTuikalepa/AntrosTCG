using ATCG.Battle.Entities.Components;
using GameSourceGeneration.Sample.Mocks;

namespace ATCG.Battle.Entities;


public partial struct TestAspect : IEntityAspect<TestComponent>, ICreateEntityAspect<SetupInfos>
{
    private static partial void CreateComponents(ref ComponentsFactory componentsFactory, SetupInfos setupInfos)
    {
        throw new System.NotImplementedException();
    }
}