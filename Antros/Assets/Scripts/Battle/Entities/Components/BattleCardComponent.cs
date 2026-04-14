using ATCG.Battle.Cards;

namespace ATCG.Battle.Entities.Components
{
    public struct BattleCardComponent : IEntityComponent
    {
        public IBattleCard battleCard;

        public BattleCardComponent(IBattleCard battleCard)
        {
            this.battleCard = battleCard;
        }
    }
}