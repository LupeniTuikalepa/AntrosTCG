using System.Collections.Generic;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Runtime;
using Helteix.Tools.Phases;

namespace ATCG.Battle
{
    public class SelectEntityActionPhase : PhaseCompletionSource<IEntityAction>, IEntitySelectionController, ISinglePhase
    {
        string ISinglePhase.Channel => nameof(SelectEntityActionPhase);
        bool ISinglePhase.AllowQueuing => false;

        public readonly IReadOnlyList<IEntityAction> actions;
        public readonly EntityAddress entityAddress;


        public SelectEntityActionPhase(EntityAddress entityAddress, List<IEntityAction> actions)
        {
            this.entityAddress = entityAddress;
            this.actions = actions;
        }

        public bool IsEmpty() => actions.Count == 0;

        public IEnumerable<T> GetAll<T>() where T : IEntityAction
        {
            foreach (IEntityAction iAction in actions)
            {
                if (iAction is T t)
                    yield return t;
            }
        }

        public bool Has<T>(out T action) where T : IEntityAction
        {
            foreach (IEntityAction iAction in actions)
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

        public int MaxSelectableEntities => 1;

        void IEntitySelectionController.OnSelected(IRuntimeEntity runtimeEntity) { }

        void IEntitySelectionController.OnUnselected(IRuntimeEntity runtimeEntity)
        {
            if(runtimeEntity.Address == entityAddress)
                SetResult(null);
        }
    }
}