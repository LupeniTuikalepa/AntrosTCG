using System;

namespace ATCG.Battle.Turns
{
    [Serializable]
    public struct BattleHistory
    {
        public int WinningPlayer { get; private set; }

        public readonly int seed;

        public BattleHistory(int seed)
        {
            this.seed = seed;
            WinningPlayer = -1;
        }


        public void RegisterTurn(BattleTurn turn)
        {
        }

        public void SetWinningPlayer(int profileID)
        {
            WinningPlayer = profileID;
        }
    }
}