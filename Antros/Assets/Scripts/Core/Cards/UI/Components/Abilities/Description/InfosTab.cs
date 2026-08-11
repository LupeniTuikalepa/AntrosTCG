using ATCG.Cards;
using TMPro;
using UnityEngine;

namespace ATCG
{
    public class InfosTab : AbilitiesUITab
    {
        [SerializeField]
        private TMP_Text description;

        public override bool Build(IGameCard gameCard)
        {
            description.text = gameCard.Description;
            return !string.IsNullOrWhiteSpace(gameCard.Description);
        }

        public override void Clear()
        {
            description.text = string.Empty;
        }
    }
}