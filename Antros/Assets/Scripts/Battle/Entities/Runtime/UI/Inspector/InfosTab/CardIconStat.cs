using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Runtime.UI.Inspector;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Elements;
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
                if (aspect.Card.CardData.Element.TryGetData(out ElementData data))
                {
                    icon.sprite = data.Icon;
                    icon.color = data.Color;
                    return true;
                }
            }

            return false;
        }

        public override void Disconnect(InspectEntityPhase phase)
        {

        }
    }
}