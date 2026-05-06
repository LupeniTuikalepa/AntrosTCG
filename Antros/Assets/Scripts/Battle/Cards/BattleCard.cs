using System;
using ATCG.Battle.Players;
using ATCG.Cards;

namespace ATCG.Battle.Cards
{
    public abstract class BattleCard<T> : GameCard<T>, IBattleCard
        where T : GameCardData
    {
        protected BattleCard(T data, IBattlePlayer player) : base(data)
        {
            Player = player;
        }

        public IBattlePlayer Player { get; }
    }
}