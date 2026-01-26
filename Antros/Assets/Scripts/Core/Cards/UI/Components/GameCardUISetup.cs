using ATCG.Capacities.UI;
using Helteix.Cards.UI.Physical;
using Helteix.Cards.UI.Physical.Components;
using TMPro;
using UnityEngine;

namespace ATCG.Cards.UI.Components
{
    [AddComponentMenu("ATCG/Cards/Card Setup")]
    public class GameCardUISetup : CardUIComponent<IGameCard>
    {
        [SerializeField]
        private TMP_Text title;
        [SerializeField]
        private CapacityUIList capacities;

        public override void Connect(IGameCard current)
        {
            base.Connect(current);
            title.text = Current.Title;
            capacities.Connect(Current);
        }

        public override void Disconnect(IGameCard current)
        {
            base.Disconnect(current);
        }
    }
}