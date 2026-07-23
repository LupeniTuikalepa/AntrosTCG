using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status.Controllers;
using ATCG.Battle.Entities.Components;
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

        public void Connect(EntityAddress address, ComponentRef<StatusTag> tagRef)
        {
            var statusTag = tagRef.GetValue();
            icon.sprite = statusTag.data.Icon;

            Color color = statusTag.data.Color;
            color.a = 1;

            icon.color = color;

            if (tagRef.EntityAddress.TryGetComponentRO(out StatusDurationController durationController))
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