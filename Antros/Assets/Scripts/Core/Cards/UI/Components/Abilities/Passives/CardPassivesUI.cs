using System.Linq;
using ATCG.Cards;
using ATCG.Passives.UI;
using UnityEngine;

namespace ATCG.Passives
{
    public class CardPassivesUI : AbilitiesUITab
    {
        [SerializeField]
        private PassiveUIList passiveUIList;

        public override bool Build(IGameCard gameCard)
        {
            if (!gameCard.Passives.Any())
                return false;

            passiveUIList.Connect(gameCard.Passives);
            return passiveUIList.UIItems.Count > 0;
        }

        public override void Clear()
        {
            passiveUIList.Disconnect();
        }
    }
}