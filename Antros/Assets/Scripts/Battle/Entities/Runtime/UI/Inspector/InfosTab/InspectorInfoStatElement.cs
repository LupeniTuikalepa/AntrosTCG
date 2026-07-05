using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Runtime.UI.Inspector;
using ATCG.Battle.Players.Local.Phases;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG.Battle.Entities.Runtime.Actions.UI.EntityStat
{
    public abstract class InspectorInfoStatElement<T> : EntityInspectorTabElement where T : struct, IEntityComponent
    {
        [SerializeField]
        private Image fill;
        [SerializeField]
        private TMP_Text numberText;

        public override bool Connect(InspectEntityPhase phase)
        {
            if (phase.EntityAddress.TryGetComponentRO(out T component))
            {
                numberText.text = GetText(component);
                fill.fillAmount = GetFillAmount(component);
                return true;
            }

            return false;
        }

        public override void Disconnect(InspectEntityPhase phase)
        {

        }


        protected abstract string GetText(T component);
        protected abstract float GetFillAmount(T component);


    }
}