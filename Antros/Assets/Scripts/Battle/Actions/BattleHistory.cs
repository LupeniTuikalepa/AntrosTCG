using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Actions
{
    [Serializable]
    public struct BattleHistory : IDisposable
    {
        [field: SerializeField]
        public int Seed { get; private set; }

        [field: SerializeField]
        public int WinningPlayer { get; private set; }

        [SerializeField]
        private List<BattleTurn> turns;

        public BattleHistory(int seed)
        {
            Seed = seed;
            turns = ListPool<BattleTurn>.Get();
            WinningPlayer = -1;
        }

        public void RegisterTurn(BattleTurn turn)
        {
            turns.Add(turn);
        }

        public void SetWinningPlayer(int winningPlayer) => WinningPlayer = winningPlayer;

        public void Dispose()
        {
            ListPool<BattleTurn>.Release(turns);
        }
    }
}