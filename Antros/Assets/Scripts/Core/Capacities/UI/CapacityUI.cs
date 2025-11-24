using Helteix.Tools.UI;
using TMPro;
using UnityEngine;

namespace ATCG.Capacities.UI
{
    public class CapacityUI : UIItem<ICapacityDescriptions>
    {
        private const string NULL = "NULL";

        [SerializeField]
        private TMP_Text title;
        [SerializeField]
        private TMP_Text cost;

        [SerializeField]
        private CanvasGroup group;

        protected override void SyncUI(ICapacityDescriptions current)
        {
            title.text = current.Name;
            cost.text = current.Cost.ToString();

            group.interactable = current.IsValid;
        }

        protected override void ClearUI()
        {
            title.text = NULL;
            cost.text = NULL;

            group.interactable = false;
        }
    }
}