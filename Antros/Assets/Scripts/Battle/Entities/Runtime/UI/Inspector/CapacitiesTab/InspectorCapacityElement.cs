using ATCG.Capacities;
using ATCG.Cards.UI.Components;
using TMPro;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.UI.Inspector.CapacitiesTab
{
    public class InspectorCapacityElement : MonoBehaviour
    {
        [SerializeField]
        private ManaCostUI manaCostUI;

        [SerializeField]
        private TMP_Text label;

        public void Connect(CapacityData capacity)
        {
            label.text = capacity.Name;
            manaCostUI.SetCost(capacity.Cost);
        }
    }
}