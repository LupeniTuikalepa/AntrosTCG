using System;
using ATCG.Battle.GameModes;
using ATCG.Cards;
using ATCG.Players;
using UnityEngine;

namespace ATCG.Battle.Players.Local
{
    public interface IBattlePlayerProfile
    {
        int ID { get; }
        PlayerInfos Infos { get; }
        GameCardData[] Cards { get; }

        IBattlePlayer Convert(BattlePhase phase);
    }

    [Serializable]
    public struct LocalPlayerProfile : IBattlePlayerProfile
    {
        [field: SerializeField]
        public int ID { get; private set; }

        [field: SerializeField]
        public PlayerInfos Infos { get; private set; }

        [field: SerializeReference]
        public GameCardData[] Cards { get; private set; }


        public LocalPlayerProfile(int id, PlayerInfos infos, GameCardData[] cards)
        {
            ID = id;
            Infos = infos;
            Cards = cards;
        }

        public IBattlePlayer Convert(BattlePhase phase)
        {
            return new LocalBattlePlayer(phase, this);
        }
    }
}