using ATCG.Battle.Heroes.Runtime;
using UnityEngine;

namespace ATCG.Battle.Heroes.Deployed
{
    public interface IHeroUIPanelElement
    {
        public void OnOpen(RuntimeHero hero, HeroUIPanel panel);
        public void OnClose(RuntimeHero hero, HeroUIPanel panel);
    }
}