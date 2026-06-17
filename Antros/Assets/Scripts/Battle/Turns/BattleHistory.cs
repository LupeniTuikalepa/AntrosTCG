using System;
using ATCG.Battle.Entities.Components;
using UnityEngine;

namespace ATCG.Battle.Turns
{
    [Serializable]
    public struct BattleHistory
    {
        [field: SerializeField]
        public BattleID WinningPlayer { get; private set; }

        public readonly int seed;

        public BattleHistory(int seed)
        {
            this.seed = seed;
            WinningPlayer = BattleID.None;
        }


        public void RegisterTurn(BattleTurn turn)
        {
        }

        public void SetWinningPlayer(BattleID profileID)
        {
            WinningPlayer = profileID;
        }
    }
}