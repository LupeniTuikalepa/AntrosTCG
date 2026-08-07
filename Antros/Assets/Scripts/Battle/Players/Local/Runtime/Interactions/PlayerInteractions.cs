using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Tags;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Local.Runtime;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle
{
    public class PlayerInteractions : RuntimeLocalPlayerComponent, IEntitySelectionController,
        IPhaseListener<LocalPlayerTurnPhase>,
        IPhaseListener<InspectEntityPhase>
    {

	    private InspectEntityPhase inspectEntityPhase;
        int IEntitySelectionController.MaxSelectableEntities => 1;
        private void OnEnable()
        {
            this.Register<LocalPlayerTurnPhase>();
            this.Register<InspectEntityPhase>();
        }

        private void OnDisable()
        {
            this.Unregister<LocalPlayerTurnPhase>();
            this.Unregister<InspectEntityPhase>();
        }

        protected override void Connect(RuntimeLocalBattlePlayer runtimeLocalBattlePlayer)
        {
            RuntimeEntityManager.SelectionController.AddPriority(this, PriorityTags.Small, this);
        }

        protected override void Disconnect(RuntimeLocalBattlePlayer runtimeLocalBattlePlayer)
        {
            RuntimeEntityManager.SelectionController.RemovePriority(this);
        }

        private bool isInActionSelection = false;

        void IEntitySelectionController.OnSelected(IRuntimeEntity runtimeEntity, ref EntityAddress selectedEntity)
        {
            RuntimeLocalBattlePlayer.Camera.Component.LookAt(runtimeEntity.transform.position);

            if(isInActionSelection)
                return;

            if(!IsPlayerTurn())
                return;

            if(runtimeEntity.Address.TryGetComponentRO(out BelongsToPlayerComponent belongsToPlayerComponent)
               && !belongsToPlayerComponent.IsAllieOf(Player))
                return;


            SelectAction(runtimeEntity).ListenForExceptions();
        }

        void IEntitySelectionController.OnUnselected(IRuntimeEntity runtimeEntity, ref EntityAddress address)
        {
            RuntimeLocalBattlePlayer.Camera.Component.LookAt(runtimeEntity.transform.position);

            if(!IsPlayerTurn())
                return;
        }

        void IEntitySelectionController.OnHoverBegin(IRuntimeEntity runtimeEntity, ref EntityAddress selectedEntity)
        {
	        if (selectedEntity.TryGetComponentRO(out InspectableTag _) && inspectEntityPhase == null)
	        {
		        inspectEntityPhase =  new InspectEntityPhase(Player, selectedEntity);
                inspectEntityPhase.isActive.AddPriority(this, PriorityTags.Small, true);
                inspectEntityPhase.RunAndForget();
	        }

            if (inspectEntityPhase != null && inspectEntityPhase.EntityAddress == selectedEntity)
            {
                inspectEntityPhase.isActive.Write(this, true);
            }
        }

        void IEntitySelectionController.OnHoverEnd(IRuntimeEntity runtimeEntity, ref EntityAddress selectedEntity)
        {
	        if (inspectEntityPhase != null && inspectEntityPhase.EntityAddress == selectedEntity )
                inspectEntityPhase.isActive.Write(this, false);
        }

        private async Awaitable SelectAction(IRuntimeEntity runtimeEntity)
        {
            EntityAddress address = runtimeEntity.Address;
            using (ListPool<EntityAction>.Get(out var actions))
            {
                address.GetActionsFor(actions, Player);

                if (actions.Count == 0)
                    return;

                isInActionSelection = true;
                await Awaitable.EndOfFrameAsync();
                SelectEntityActionPhase phase = new SelectEntityActionPhase(Player, address, actions);

                RuntimeEntityManager.SelectionController.AddPriority(runtimeEntity.gameObject, PriorityTags.Default, this);

                PhaseResult<EntityAction> result = await phase;
                EntityAction action = result.value;

                if(actions.Contains(action))
                    await action.Execute(address, BattlePhase);

                isInActionSelection = false;
                RuntimeEntityManager.SelectionController.RemovePriority(runtimeEntity.gameObject);
                RuntimeEntityManager.Unselect(address);
            }
        }

        void IPhaseListener<LocalPlayerTurnPhase>.OnPhaseBegin(LocalPlayerTurnPhase phase)
        {
        }

        void IPhaseListener<LocalPlayerTurnPhase>.OnPhaseEnd(LocalPlayerTurnPhase phase)
        {
        }

        void IPhaseListener<InspectEntityPhase>.OnPhaseBegin(InspectEntityPhase phase)
        {
        }

        void IPhaseListener<InspectEntityPhase>.OnPhaseEnd(InspectEntityPhase phase)
        {
            inspectEntityPhase = null;
        }
    }
}