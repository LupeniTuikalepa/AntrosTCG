using ATCG.Battle.Metrics;

namespace ATCG.Battle.Heroes.Deployed
{
    public class MoveButtonUI : HeroButtonUI
    {
        protected override int GetCost() => GameplayMetrics.Current.MovementCost;

        public override void OnClick() => _ = RuntimeHero.Move();
    }
}