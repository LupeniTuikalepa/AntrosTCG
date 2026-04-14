using ATCG.Battle.Cards;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players;
using ATCG.HexGrids;

namespace ATCG.Battle.Entities.Aspects
{
    public struct HeroEntityAspect : IEntityAspect<HeroComponent, BattleCardComponent, GridMemberComponent>
    {
        public ref HeroComponent HeroComponent => ref EntityAddress.GetComponent<HeroComponent>();
        public ref BattleCardComponent BattleCardComponent => ref EntityAddress.GetComponent<BattleCardComponent>();
        public ref GridMehermberComponent GridMemberComponent => ref EntityAddress.GetComponent<GridMemberComponent>();

        public string Name => HeroCard.Title;
        public IBattlePlayer Player => HeroCard.Player;
        public HeroBattleCard HeroCard => BattleCardComponent.battleCard as HeroBattleCard;
        public HexCoordinates Coordinates => GridMemberComponent.Coordinates;

        public IBattleCard Card => BattleCardComponent.battleCard;

        public EntityAddress EntityAddress { get; set; }
    }
}