using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Battle.Players.Local.Phases;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG.Battle.Entities.Runtime.UI.Inspector.StatusTab
{
    public class InspectorStatusElement : MonoBehaviour
    {
        [SerializeField]
        private Image icon;
        [SerializeField]
        private Image outline;

        [SerializeField]
        private GameObject durationRoot;
        [SerializeField]
        private TMP_Text durationText;

        public void Connect<T>(EntityAddress address, T status) where T : struct, IStatusComponent
        {
            icon.sprite = status.StatusData.Icon;

            Color color = status.StatusData.Color;
            color.a = 1;

            icon.color = color;

            if (address.TryGetComponentRO(out StatusDurationController<T> durationController))
            {
                durationRoot.SetActive(true);
                durationText.text = durationController.RemainingTicks.ToString();
                outline.color = color;
                durationText.color = color;
            }
            else
            {
                durationRoot.SetActive(false);
            }
        }
    }
}