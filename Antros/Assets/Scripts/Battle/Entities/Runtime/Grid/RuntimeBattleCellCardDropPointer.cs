
using ATCG.Battle.Cards;
using ATCG.Battle.Entities.Runtime.Grid;
using Helteix.Cards.UI.Physical.Drag;
using UnityEngine;

namespace ATCG.Battle
{
    public class RuntimeBattleCellCardDropPointer : MonoBehaviour, ICardDropTargetPointer<IBattleCard>
    {
        ICardDropTarget<IBattleCard> ICardDropTargetPointer<IBattleCard>.DropTarget => runtimeBattleCell;

        [SerializeField]
        private RuntimeBattleCell runtimeBattleCell;
    }
}