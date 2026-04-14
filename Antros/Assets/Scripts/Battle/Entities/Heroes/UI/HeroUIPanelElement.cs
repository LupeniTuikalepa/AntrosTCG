using ATCG.Battle.Entities.Heroes.Runtime;

namespace ATCG.Battle.Entities.Heroes.UI
{
    public interface IHeroUIPanelElement
    {
        public void OnOpen(RuntimeHero hero, HeroUIPanel panel);
        public void OnClose(RuntimeHero hero, HeroUIPanel panel);
    }
}