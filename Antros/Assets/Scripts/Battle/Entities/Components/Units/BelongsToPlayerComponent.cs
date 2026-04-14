using ATCG.Battle.Players;

namespace ATCG.Battle.Entities.Components
{
    public readonly struct BelongsToPlayerComponent : IEntityComponent
    {
        public readonly int playerId;

        public BelongsToPlayerComponent(int playerId)
        {
            this.playerId = playerId;
        }

        public bool IsAllieOf(IBattlePlayer player) => IsAllieOf(player.Profile.ID);
        public bool IsAllieOf(int id) => playerId == id;
    }
}