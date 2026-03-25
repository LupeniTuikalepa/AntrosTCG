using ATCG.Battle.Cards;
using ATCG.Battle.Players;

namespace ATCG.Battle.Grids.Entities.Heroes
{
    public class HeroEntity : GridEntity
    {
        public string Name => Card.Title;
        public IBattlePlayer Player => Card.Player;

        public HeroBattleCard Card { get; }

        public HeroEntity(HeroBattleCard card, BattlePhase battlePhase) : base(battlePhase)
        {
            this.Card = card;
        }


    }
}