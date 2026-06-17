using System;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players;
using ATCG.Cards;

namespace ATCG.Battle.Cards
{
    public abstract class BattleCard<T> : GameCard<T>, IBattleCard
        where T : GameCardData
    {
        public BattleID ID { get; private set; }

        protected BattleCard(T data, IBattlePlayer player) : base(data)
        {
            Player = player;
            ID = BattleID.CreateNew();
        }

        public IBattlePlayer Player { get; }
    }
}