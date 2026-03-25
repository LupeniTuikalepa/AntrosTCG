using ATCG.Metrics;

namespace ATCG.Battle.Heroes.Deployed
{
    public class MoveButtonUI : HeroButtonUI
    {
        protected override int GetCost() => GameMetrics.Current.MovementCost;

        public override void OnClick() => _ = RuntimeHero.Move();
    }
}