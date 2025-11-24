using ATCG.Players;

namespace ATCG.Battle.Players
{
    [System.Serializable]
    public struct BattlePlayerProfile
    {
        public int id;
        public PlayerProfile playerProfile;
        public string[] playerDeck;
    }
}