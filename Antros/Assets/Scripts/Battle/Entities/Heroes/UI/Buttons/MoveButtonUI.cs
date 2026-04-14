using ATCG.Metrics;

namespace ATCG.Battle.Entities.Heroes.UI.Buttons
{
    public class MoveButtonUI : HeroButtonUI
    {
        protected override int GetCost()
        {
            return GameMetrics.Current.MovementCost;
        }

        public override void OnClick()
        {
            _ = RuntimeHero.Move();
        }
    }
}