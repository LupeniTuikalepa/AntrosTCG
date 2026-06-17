using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players;
using ATCG.Cards;

namespace ATCG.Battle.Cards
{
    public interface IBattleCard : IGameCard
    {
        IBattlePlayer Player { get; }
        BattleID ID { get; }
    }
}