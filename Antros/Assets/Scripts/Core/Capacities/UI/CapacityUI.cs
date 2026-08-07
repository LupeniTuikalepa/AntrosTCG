using ATCG.Cards.UI.Components;
using Helteix.Tools.UI;
using TMPro;
using UnityEngine;

namespace ATCG.Capacities.UI
{
    public class CapacityUI : UIItem<CapacityData>
    {
        private const string NULL = "NULL";

        [SerializeField]
        private TMP_Text title;
        [SerializeField]
        private ManaCostUI cost;

        [SerializeField]
        private CanvasGroup group;

        protected override void SyncUI(CapacityData current)
        {
            title.text = current.Name;
            cost.SetCost(current.Cost);

            group.interactable = true;
        }

        protected override void ClearUI()
        {
            title.text = NULL;

            group.interactable = false;
        }
    }
}