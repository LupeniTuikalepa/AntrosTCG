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
        private TMP_Text cost;

        [SerializeField]
        private CanvasGroup group;

        protected override void SyncUI(CapacityData current)
        {
            title.text = current.Name;
            cost.text = current.Cost.ToString();

            group.interactable = true;
        }

        protected override void ClearUI()
        {
            title.text = NULL;
            cost.text = NULL;

            group.interactable = false;
        }
    }
}