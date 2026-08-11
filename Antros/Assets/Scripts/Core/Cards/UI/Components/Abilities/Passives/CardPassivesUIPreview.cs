using System.Linq;
using ATCG.Cards;
using ATCG.Passives.UI;
using Helteix.Cards.UI.Physical.Components;
using UnityEngine;

namespace ATCG.Passives
{
    public class CardPassivesUIPreview : CardUIComponent<IGameCard>
    {
        [SerializeField]
        private PassiveUIList passiveUIList;

        public override void Connect(IGameCard current)
        {
            passiveUIList.Connect(current.Passives);
            base.Connect(current);
        }

        public override void Disconnect(IGameCard current)
        {
            passiveUIList.Disconnect();
            base.Disconnect(current);
        }

    }
}