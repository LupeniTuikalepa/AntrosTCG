using ATCG.Battle.Heroes.Runtime;

namespace ATCG.Battle.Heroes.Deployed
{
    public interface IHeroUIPanelElement
    {
        public void OnOpen(RuntimeHero hero, HeroUIPanel panel);
        public void OnClose(RuntimeHero hero, HeroUIPanel panel);
    }
}