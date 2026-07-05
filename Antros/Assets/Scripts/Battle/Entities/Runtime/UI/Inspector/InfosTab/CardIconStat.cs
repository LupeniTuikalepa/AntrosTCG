using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Runtime.UI.Inspector;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG.Battle.Entities.Runtime.Actions.UI.EntityStat
{
    public class CardIconStat : EntityInspectorTabElement
    {
        [SerializeField]
        private Image icon;

        public override bool Connect(InspectEntityPhase phase)
        {
            if (phase.EntityAddress.Is<HeroEntityAspect>(out var aspect))
            {
                ElementInfos elementInfos = aspect.Card.CardData.Element.GetInfos();

                icon.sprite = elementInfos.Icon;
                icon.color = elementInfos.Color;
                return true;
            }

            return false;
        }

        public override void Disconnect(InspectEntityPhase phase)
        {

        }
    }
}