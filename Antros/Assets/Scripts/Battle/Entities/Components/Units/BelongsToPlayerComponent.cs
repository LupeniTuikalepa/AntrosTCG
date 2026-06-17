using ATCG.Battle.GameModes;
using ATCG.Battle.Players;

namespace ATCG.Battle.Entities.Components
{
    public readonly struct BelongsToPlayerComponent : IEntityComponent
    {
        public readonly BattleID playerId;

        public BelongsToPlayerComponent(BattleID playerId)
        {
            this.playerId = playerId;
        }

        public IBattlePlayer GetPlayer(BattlePhase battlePhase) => battlePhase.GetPlayer(playerId);

        public bool IsAllieOf(IBattlePlayer player) => IsAllieOf(player.Profile.ID);
        public bool IsAllieOf(BattleID id) => playerId == id;
    }
}