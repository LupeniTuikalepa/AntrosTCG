using System.Linq;
using Helteix.Cards.UI;
using TMPro;
using UnityEngine;

namespace ATCG.Battle.Cards.UI
{

    [AddComponentMenu("ATCG/Gameplay/Cards/Battle Deck")]
    public class BattleDeckUI : CardCollectionUI<IBattleCard>
    {
        [SerializeField]
        private TMP_Text count;


        protected override void ConnectWithCurrent()
        {
            base.ConnectWithCurrent();
            Refresh();
        }

        protected override void OnCardAdded(IBattleCard card)
        {
            base.OnCardAdded(card);
            Refresh();
        }

        protected override void OnCardRemoved(IBattleCard card)
        {
            base.OnCardRemoved(card);
            Refresh();
        }

        private void Refresh()
        {
            count.text = Current.Cards.Count().ToString();
        }
    }
}