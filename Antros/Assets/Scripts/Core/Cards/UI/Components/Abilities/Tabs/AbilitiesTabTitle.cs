using TMPro;
using UnityEngine;

namespace ATCG.Cards.UI.Components.Abilities.Tabs
{
    public class AbilitiesTabTitle : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text text;

        public void Show(AbilitiesUITab tab)
        {
            gameObject.SetActive(true);
            text.text = tab.TabName;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}