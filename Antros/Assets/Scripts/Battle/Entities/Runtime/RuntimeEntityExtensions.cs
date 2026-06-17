using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Runtime;
using ATCG.Capacities;
using ATCG.HexGrids;

namespace ATCG.Battle.Entities.Runtime
{
    public static class RuntimeEntityExtensions
    {
        public static bool TryGetOwner(this IRuntimeEntity entity, out IBattlePlayer entityOwner)
        {
            if (entity.Address.TryGetComponent<BelongsToPlayerComponent>(out var componentRef))
            {
                BattleID playerId = componentRef.GetValue().playerId;
                entityOwner = entity.BattlePhase.GetPlayer(playerId);
                return entityOwner != null;
            }

            entityOwner = null;
            return false;
        }
    }
}