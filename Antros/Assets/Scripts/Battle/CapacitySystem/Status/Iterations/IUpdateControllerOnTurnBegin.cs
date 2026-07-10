using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Utilities.Iterations;

namespace ATCG.Battle.CapacitySystem.Status.Iterations
{
    [GenerateComponentIterator]
    public interface IUpdateControllerOnTurnBegin : IStatusController
    {
        void Process();
    }
}