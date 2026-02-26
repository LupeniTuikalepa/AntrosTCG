using ATCG.Battle.Metrics;

namespace ATCG.Battle.Heroes.Deployed
{
    public class BasicAttackButtonUI : HeroButtonUI
    {
        protected override int GetCost()
        {
            return GameplayMetrics.Current.BasicAttackCost;
        }

        public override void OnClick() => RuntimeHero.DoBasicAttack();
    }
}