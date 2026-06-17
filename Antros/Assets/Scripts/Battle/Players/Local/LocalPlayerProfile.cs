using System;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using ATCG.Cards;
using ATCG.Players;
using UnityEngine;

namespace ATCG.Battle.Players.Local
{
    public interface IBattlePlayerProfile
    {
        BattleID ID { get; }
        PlayerInfos Infos { get; }
        GameCardData[] Cards { get; }

        IBattlePlayer Convert(BattlePhase phase);
    }

    [Serializable]
    public struct LocalPlayerProfile : IBattlePlayerProfile
    {
        [field: SerializeField]
        public BattleID ID { get; private set; }

        [field: SerializeField]
        public PlayerInfos Infos { get; private set; }

        [field: SerializeReference]
        public GameCardData[] Cards { get; private set; }


        public LocalPlayerProfile(BattleID id, PlayerInfos infos, GameCardData[] cards)
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