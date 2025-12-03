using ATCG.Battle.Players.Local;
using ATCG.Cards;
using ATCG.Players;
using UnityEngine.InputSystem.Users;

namespace ATCG.Battle.Players
{
    public interface IBattlePlayerProfile
    {
        int ID { get; }
        PlayerProfile Profile { get; }
        PlayerDeck Deck { get; }

        IBattlePlayer Convert(BattleGameMode gameMode);
    }

    [System.Serializable]
    public struct LocalPlayerProfile : IBattlePlayerProfile
    {
        public int ID { get; set; }
        public PlayerProfile Profile { get; set; }
        public PlayerDeck Deck { get; set; }


        public IBattlePlayer Convert(BattleGameMode gameMode) => new LocalBattlePlayer(gameMode, this);
    }
}