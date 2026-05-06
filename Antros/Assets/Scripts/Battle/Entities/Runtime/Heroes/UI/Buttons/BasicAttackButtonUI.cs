using ATCG.Metrics;

namespace ATCG.Battle.Entities.Runtime.Heroes.UI.Buttons
{
    public class BasicAttackButtonUI : HeroButtonUI
    {
        protected override int GetCost()
        {
            return GameMetrics.Current.BasicAttackCost;
        }

        public override void OnClick()
        {
            _ = RuntimeHero.DoBasicAttack();
        }
    }
}