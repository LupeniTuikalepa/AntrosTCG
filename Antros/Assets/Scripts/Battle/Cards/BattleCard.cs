using ATCG.Battle.Players;
using ATCG.Cards;

namespace ATCG.Battle.Cards
{
    public abstract class BattleCard<T> : GameCard<T>, IBattleCard
        where T : GameCardData
    {
        public IBattlePlayer Player { get; private set; }

        protected BattleCard(T data, IBattlePlayer player) : base(data)
        {
            Player = player;
        }
    }
}