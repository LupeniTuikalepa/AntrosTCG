using System.Collections.Generic;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Local.UI;
using Helteix.Tools.Phases;

namespace ATCG.Battle
{
    public class SelectEntityActionPhase : LocalPlayerPhaseCompletionSource<EntityAction>, IEntitySelectionController, ISinglePhase, ILocalHUDPhase<SelectEntityActionPhase>
    {
        string ISinglePhase.Channel => nameof(SelectEntityActionPhase);
        bool ISinglePhase.AllowQueuing => false;

        public readonly IReadOnlyList<EntityAction> actions;
        public readonly EntityAddress entityAddress;


        public SelectEntityActionPhase(LocalBattlePlayer player, EntityAddress entityAddress, List<EntityAction> actions) : base(player)
        {
            this.entityAddress = entityAddress;
            this.actions = actions;
        }

        public bool IsEmpty() => actions.Count == 0;

        public IEnumerable<T> GetAll<T>() where T : EntityAction
        {
            foreach (EntityAction iAction in actions)
            {
                if (iAction is T t)
                    yield return t;
            }
        }

        public bool Has<T>(out T action) where T : EntityAction
        {
            foreach (EntityAction iAction in actions)
            {
                if (iAction is T t)
                {
                    action = t;
                    return true;
                }
            }
            action = default;
            return false;
        }

        int IEntitySelectionController.MaxSelectableEntities => 1;

        void IEntitySelectionController.OnSelected(IRuntimeEntity runtimeEntity, ref EntityAddress selectedEntity) { }

        void IEntitySelectionController.OnUnselected(IRuntimeEntity runtimeEntity, ref EntityAddress selectedEntity)
        {
            if(runtimeEntity.Address == entityAddress)
                SetResult(null);
        }

        void IEntitySelectionController.OnHoverBegin(IRuntimeEntity runtimeEntity, ref EntityAddress selectedEntity)
        {
        }

        void IEntitySelectionController.OnHoverEnd(IRuntimeEntity runtimeEntity, ref EntityAddress selectedEntity)
        {
        }
    }
}