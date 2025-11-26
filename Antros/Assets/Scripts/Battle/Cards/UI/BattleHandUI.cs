using Helteix.Cards;
using Helteix.Cards.UI.Physical;
using UnityEngine;

namespace ATCG.Battle.Cards.UI
{

    [AddComponentMenu("ATCG/Gameplay/Cards/Hand")]
    public class BattleHandUI : PhysicalCardCollectionUI<IBattleCard>
    {
        protected override bool CanCardBeDragged(ICard card) => false;

        protected override bool CanCardBeClicked(ICard card) => false;
    }
}