namespace ATCG.Battle.Entities.Runtime.Heroes.UI
{
    public interface IHeroUIPanelElement
    {
        public void OnOpen(RuntimeHero hero, HeroUIPanel panel);
        public void OnClose(RuntimeHero hero, HeroUIPanel panel);
    }
}