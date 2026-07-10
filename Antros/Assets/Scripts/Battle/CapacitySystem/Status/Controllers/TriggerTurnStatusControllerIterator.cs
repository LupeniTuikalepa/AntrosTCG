using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Players;

namespace ATCG.Battle.Entities.Components.Status
{
    public readonly struct TriggerTurnStatusControllerIterator : IStatusTurnControllerIterator
    {
        private readonly StatusContext statusContext;
        private readonly bool isStart;
        private readonly IBattlePlayer playerTurn;

        public TriggerTurnStatusControllerIterator(StatusContext statusContext, bool isStart, IBattlePlayer playerTurn)
        {
            this.statusContext = statusContext;
            this.isStart = isStart;
            this.playerTurn = playerTurn;
        }

        void IStatusTurnControllerIterator.Process<T>()
        {
            foreach (var componentRef in statusContext.battlePhase.world.Query<T>())
            {
                ref T component = ref componentRef.GetValue();

                if (componentRef.EntityAddress.TryGetComponentRO(out BelongsToPlayerComponent belongsToPlayer))
                {
                    if(!belongsToPlayer.IsAllieOf(playerTurn))
                        continue;
                }

                if (isStart)
                    component.OnTurnStarted();
                else
                    component.OnTurnEnded();
            }
        }
    }
}