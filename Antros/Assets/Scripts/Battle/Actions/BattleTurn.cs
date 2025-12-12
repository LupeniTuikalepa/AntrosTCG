using System;
using ATCG.Battle.Actions.TurnInfos;
using UnityEngine;

namespace ATCG.Battle.Actions
{
    [Serializable]
    public struct BattleTurn
    {
        [SerializeField]
        public string turnID;
        [SerializeField]
        public int playerID;

        [SerializeReference]
        public IBattleActionInfos[] actions;
    }

}