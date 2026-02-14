using ATCG.Cards;
using ATCG.Players;
using UnityEngine;
using UnityEngine.InputSystem.Users;

namespace ATCG.Battle.Players
{
    public interface IBattlePlayerProfile
    {
        int ID { get; }
        PlayerInfos Infos { get; }
        GameCardData[] Cards { get; }

        IBattlePlayer Convert(BattlePhase phase);
    }

    [System.Serializable]
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

        public IBattlePlayer Convert(BattlePhase phase) => new LocalBattlePlayer(phase, this);
    }
}