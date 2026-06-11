using System;
using System.Collections.Generic;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Local.Runtime;
using Helteix.ChanneledProperties.Conditions;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools;
using Helteix.Tools.Phases;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle
{
    public class PlayerInteractions : RuntimeLocalPlayerComponent, IEntitySelectionController,
        IPhaseListener<LocalPlayerTurnPhase>
    {
        int IEntitySelectionController.MaxSelectableEntities => 1;


        private void OnEnable()
        {
            this.Register();
        }

        private void OnDisable()
        {
            this.Unregister();
        }

        protected override void Connect(LocalBattlePlayer player)
        {

        }

        protected override void Disconnect(LocalBattlePlayer player)
        {

        }

        void IEntitySelectionController.OnSelected(IRuntimeEntity runtimeEntity)
        {
            RuntimeLocalPlayer.Camera.Component.LookAt(runtimeEntity.transform.position);

            if(runtimeEntity.Address.TryGetComponentRO(out BelongsToPlayerComponent belongsToPlayerComponent)
               && !belongsToPlayerComponent.IsAllieOf(Player))
                return;

            SelectAction(runtimeEntity.Address).FireAndForget();
        }

        void IEntitySelectionController.OnUnselected(IRuntimeEntity runtimeEntity)
        {
            RuntimeLocalPlayer.Camera.Component.LookAt(runtimeEntity.transform.position);
        }

        private async Awaitable SelectAction(EntityAddress address)
        {
            IEntityAction action = await SelectEntityActionPhase.RunPhaseFor(address);

            if (action != null)
            {
                await action.Execute(address, BattlePhase);
                RuntimeEntityManager.Unselect(address);
            }
        }

        void IPhaseListener<LocalPlayerTurnPhase>.OnPhaseBegin(LocalPlayerTurnPhase phase)
        {
            if(phase.player == Player)
                RuntimeEntityManager.SelectionController.AddPriority(this, PriorityTags.Small, this);
        }

        void IPhaseListener<LocalPlayerTurnPhase>.OnPhaseEnd(LocalPlayerTurnPhase phase)
        {
            if (phase.player == Player)
                RuntimeEntityManager.SelectionController.RemovePriority(this);
        }
    }
}