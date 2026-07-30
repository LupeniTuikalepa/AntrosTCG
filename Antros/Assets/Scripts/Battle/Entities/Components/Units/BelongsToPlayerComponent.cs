using ATCG.Battle.GameModes;
using ATCG.Battle.Players;

namespace ATCG.Battle.Entities.Components
{
    public readonly struct BelongsToPlayerComponent : IEntityComponent
    {
        public readonly BattleID playerId;
        public readonly int playerNumber;

        public BelongsToPlayerComponent(BattleID playerId, int playerNumber)
        {
            this.playerId = playerId;
            this.playerNumber = playerNumber;
        }

        public IBattlePlayer GetPlayer(BattlePhase battlePhase) => battlePhase.GetPlayer(playerId);

        public bool IsAllieOf(IBattlePlayer player) => IsAllieOf(player.Profile.ID);
        public bool IsAllieOf(BattleID id) => playerId == id;
    }

    public static class BelongsToPlayerComponentExtensions
    {
        public static bool IsAlly(this EntityAddress address, IBattlePlayer player)
        {
            if(address.TryGetComponentRO(out BelongsToPlayerComponent belongsToPlayer))
                return belongsToPlayer.IsAllieOf(player.Profile.ID);

            return false;
        }
    }
}