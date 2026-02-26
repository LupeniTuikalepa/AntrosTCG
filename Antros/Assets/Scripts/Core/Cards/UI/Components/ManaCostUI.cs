using TMPro;
using UnityEngine;

namespace ATCG.Cards.UI.Components
{
    public class ManaCostUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI costText;

        public void SetCost(int cost)
        {
            if (cost > 0)
            {
                gameObject.SetActive(true);
                costText.text = cost.ToString();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}